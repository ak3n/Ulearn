﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Database;
using Database.DataContexts;
using Database.Models;
using log4net;
using uLearn;

namespace Notifications
{
	public class NotificationSender : INotificationSender
	{
		private readonly ILog log = LogManager.GetLogger(typeof(NotificationSender));

		private readonly IEmailSender emailSender;
		private readonly ITelegramSender telegramSender;
		private readonly CourseManager courseManager;
		private readonly string baseUrl;
		private readonly string secretForHashes;

		public NotificationSender(CourseManager courseManager, IEmailSender emailSender, ITelegramSender telegramSender)
		{
			this.emailSender = emailSender;
			this.telegramSender = telegramSender;
			this.courseManager = courseManager;
			baseUrl = ConfigurationManager.AppSettings["ulearn.baseUrl"] ?? "";
			secretForHashes = ConfigurationManager.AppSettings["ulearn.secretForHashes"] ?? "";
		}

		public NotificationSender(CourseManager courseManager)
			: this(courseManager, new KonturSpamEmailSender(), new TelegramSender())
		{
		}

		public NotificationSender()
			: this(WebCourseManager.Instance)
		{
		}

		private string GetEmailTextSignature()
		{
			return "\n\n—\nВсегда ваши,\nКоманда ulearn.me\n\nВы можете отписаться от получения уведомлений на почту в настройках вашего профиля.";
		}

		private string GetEmailHtmlSignature(int transportId, NotificationType notificationType, string courseId, string courseTitle)
		{
			var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
			var signature = NotificationsRepo.GetNotificationTransportEnablingSignature(transportId, timestamp, secretForHashes);
			var transportUnsubscribeUrl = $"{baseUrl}/Notifications/SaveSettings?courseId={courseId}&transportId={transportId}&notificationType={(int) notificationType}&isEnabled=False&timestamp={timestamp}&signature={signature}";
			return "<br/><br/>" +
					"<div style=\"color: #999; font-size: 12px;\">" +
					$"<a href=\"{transportUnsubscribeUrl}\">Нажмите здесь</a>, если вы не хотите получать такие уведомления от курса «{courseTitle}» на почту.<br/>" +
					$"Если вы вовсе не хотите получать от нас уведомления на почту, выключите их <a href=\"{baseUrl}/Account/Manage\">в профиле</a>." +
					"</div>";
		}

		private string GetEmailHtmlSignature(NotificationDelivery delivery)
		{
			var courseId = delivery.Notification.CourseId;
			var courseTitle = courseManager.GetCourse(courseId).Title;
			return GetEmailHtmlSignature(delivery.NotificationTransportId, delivery.Notification.GetNotificationType(), courseId, courseTitle);
		}

		private async Task SendAsync(MailNotificationTransport transport, NotificationDelivery notificationDelivery)
		{
			if (string.IsNullOrEmpty(transport.User.Email))
				return;

			var notification = notificationDelivery.Notification;
			var course = courseManager.GetCourse(notification.CourseId);

			await emailSender.SendEmailAsync(
				transport.User.Email,
				notification.GetNotificationType().GetDisplayName(),
				notification.GetTextMessageForDelivery(transport, notificationDelivery, course, baseUrl),
				notification.GetHtmlMessageForDelivery(transport, notificationDelivery, course, baseUrl),
				textContentAfterButton: GetEmailTextSignature(),
				htmlContentAfterButton: GetEmailHtmlSignature(notificationDelivery)
			);
		}

		private async Task SendAsync(MailNotificationTransport transport, List<NotificationDelivery> notificationDeliveries)
		{
			if (string.IsNullOrEmpty(transport.User.Email))
				return;

			if (notificationDeliveries.Count <= 0)
				return;

			var firstDelivery = notificationDeliveries[0];
			var subject = firstDelivery.Notification.GetNotificationType().GetGroupName();

			var htmlBodies = new List<string>();
			var textBodies = new List<string>();
			foreach (var delivery in notificationDeliveries)
			{
				var notification = delivery.Notification;
				var course = courseManager.GetCourse(notification.CourseId);

				htmlBodies.Add(notification.GetHtmlMessageForDelivery(transport, delivery, course, baseUrl));
				textBodies.Add(notification.GetTextMessageForDelivery(transport, delivery, course, baseUrl));
			}

			await emailSender.SendEmailAsync(
				transport.User.Email,
				subject,
				string.Join("\n\n", textBodies),
				string.Join("<br/><br/>", htmlBodies),
				textContentAfterButton: GetEmailTextSignature(),
				htmlContentAfterButton: GetEmailHtmlSignature(firstDelivery)
			);
		}

		private async Task SendAsync(TelegramNotificationTransport transport, NotificationDelivery notificationDelivery)
		{
			if (!transport.User.TelegramChatId.HasValue)
				return;

			var notification = notificationDelivery.Notification;
			var course = courseManager.GetCourse(notification.CourseId);

			await telegramSender.SendMessageAsync(
				transport.User.TelegramChatId.Value,
				notification.GetHtmlMessageForDelivery(transport, notificationDelivery, course, baseUrl)
				);
		}

		private async Task SendAsync(TelegramNotificationTransport transport, List<NotificationDelivery> notificationDeliveries)
		{
			if (!transport.User.TelegramChatId.HasValue)
				return;

			if (notificationDeliveries.Count <= 0)
				return;

			var subject = $"<b>{notificationDeliveries[0].Notification.GetNotificationType().GetGroupName().EscapeHtml()}</b>";

			var htmls = new List<string> { subject };
			foreach (var delivery in notificationDeliveries)
			{
				var notification = delivery.Notification;
				var course = courseManager.GetCourse(notification.CourseId);

				htmls.Add(notification.GetHtmlMessageForDelivery(transport, delivery, course, baseUrl));
			}

			await telegramSender.SendMessageAsync(transport.User.TelegramChatId.Value, string.Join("<br><br>", htmls));
		}

		public async Task SendAsync(NotificationDelivery notificationDelivery)
		{
			var transport = notificationDelivery.NotificationTransport;

			if (transport is MailNotificationTransport)
				await SendAsync(transport as MailNotificationTransport, notificationDelivery);
			else if (transport is TelegramNotificationTransport)
				await SendAsync(transport as TelegramNotificationTransport, notificationDelivery);
			else
				throw new Exception($"Unknown notification transport: {transport.GetType()}");
		}

		public async Task SendAsync(List<NotificationDelivery> notificationDeliveries)
		{
			if (notificationDeliveries.Count <= 0)
				return;

			var transport = notificationDeliveries[0].NotificationTransport;
			var notificationType = notificationDeliveries[0].Notification.GetNotificationType();
			foreach (var delivery in notificationDeliveries)
			{
				if (delivery.NotificationTransportId != transport.Id)
					throw new Exception("NotificationSender.SendAsync(List<NotificationDelivery>): all deliveries should be for one transport");
				if (delivery.Notification.GetNotificationType() != notificationType)
					throw new Exception("NotificationSender.SendAsync(List<NotificationDelivery>): all deliveries should be for one notification type");
			}

			if (transport is MailNotificationTransport)
				await SendAsync(transport as MailNotificationTransport, notificationDeliveries);
			else if (transport is TelegramNotificationTransport)
				await SendAsync(transport as TelegramNotificationTransport, notificationDeliveries);
			else
				throw new Exception($"Unknown notification transport: {transport.GetType()}");
		}
	}
}