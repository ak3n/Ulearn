﻿using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Database.Models;
using log4net;

namespace Database.DataContexts
{
	public class FeedRepo
	{
		private readonly ILog log = LogManager.GetLogger(typeof(FeedRepo));

		private readonly ULearnDb db;
		private readonly NotificationsRepo notificationsRepo;
		private readonly VisitsRepo visitsRepo;

		public FeedRepo(ULearnDb db)
		{
			this.db = db;
			notificationsRepo = new NotificationsRepo(db);
			visitsRepo = new VisitsRepo(db);
		}

		public DateTime? GetFeedViewTimestamp(string userId)
		{
			var updateTimestamp = db.FeedViewTimestamps.Where(t => t.UserId == userId).OrderByDescending(t => t.Timestamp).FirstOrDefault();
			return updateTimestamp?.Timestamp;
		}

		public void UpdateFeedViewTimestamp(string userId, DateTime timestamp)
		{
			db.FeedViewTimestamps.AddOrUpdate(t => t.UserId == userId, new FeedViewTimestamp
			{
				UserId = userId,
				Timestamp = timestamp
			});
		}

		public async Task AddFeedNotificationTransportIfNeeded(string userId)
		{
			if (notificationsRepo.FindUsersNotificationTransport<FeedNotificationTransport>(userId, includeDisabled: true) != null)
				return;

			await notificationsRepo.AddNotificationTransport(new FeedNotificationTransport
			{
				UserId = userId,
				IsEnabled = true,
			});
		}

		public FeedNotificationTransport GetUsersFeedNotificationTransport(string userId)
		{
			return notificationsRepo.FindUsersNotificationTransport<FeedNotificationTransport>(userId);
		}

		public FeedNotificationTransport GetCommonFeedNotificationTransport()
		{
			var transport = notificationsRepo.FindUsersNotificationTransport<FeedNotificationTransport>(null);
			if (transport == null)
			{
				log.Error("Can't find common feed notification transport. You should create FeedNotificationTransport with userId = NULL");
				throw new Exception("Can't find common feed notification transport");
			}

			return transport;
		}

		public DateTime? GetLastDeliveryTimestamp(FeedNotificationTransport notificationTransport)
		{
			return notificationsRepo.GetLastDeliveryTimestamp(notificationTransport);
		}

		public int GetNotificationsCount(string userId, DateTime from, params FeedNotificationTransport[] transports)
		{
			return GetFeedNotificationDeliveriesQueryable(userId, transports).Count();
		}

		public List<NotificationDelivery> GetFeedNotificationDeliveries(string userId, params FeedNotificationTransport[] transports)
		{
			return GetFeedNotificationDeliveriesQueryable(userId, transports)
				.OrderByDescending(d => d.CreateTime)
				.Take(20)
				.ToList();
		}

		private IQueryable<NotificationDelivery> GetFeedNotificationDeliveriesQueryable(string userId, params FeedNotificationTransport[] transports)
		{
			var transportsIds = new List<FeedNotificationTransport>(transports).Select(t => t.Id).ToList();
			var userCourses = visitsRepo.GetUserCourses(userId);
			return notificationsRepo.GetTransportsDeliveries(transportsIds, DateTime.MinValue)
				.Where(d => userCourses.Contains(d.Notification.CourseId))
				.Where(d => d.Notification.InitiatedById != userId);
		}
	}
}
