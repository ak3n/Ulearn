﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Database;
using Database.Models;
using Database.Repos;
using Database.Repos.Groups;
using Database.Repos.Users;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ulearn.Common.Api.Models.Responses;
using Ulearn.Core.Courses;
using Ulearn.Web.Api.Models.Parameters;
using Ulearn.Web.Api.Models.Responses.User;

namespace Ulearn.Web.Api.Controllers
{
	[Route("/userProgress")]
	public class UserProgressController : BaseController
	{
		private readonly IVisitsRepo visitsRepo;
		private readonly IUserQuizzesRepo userQuizzesRepo;
		private readonly IAdditionalScoresRepo additionalScoresRepo;
		private readonly ICourseRolesRepo courseRolesRepo;
		private readonly IGroupAccessesRepo groupAccessesRepo;
		private readonly IGroupMembersRepo groupMembersRepo;
		private readonly ISlideCheckingsRepo slideCheckingsRepo;

		public UserProgressController(IWebCourseManager courseManager, UlearnDb db, IUsersRepo usersRepo,
			IVisitsRepo visitsRepo, IUserQuizzesRepo userQuizzesRepo, IAdditionalScoresRepo additionalScoresRepo,
			ICourseRolesRepo courseRolesRepo, IGroupAccessesRepo groupAccessesRepo, IGroupMembersRepo groupMembersRepo,
			ISlideCheckingsRepo slideCheckingsRepo)
			: base(courseManager, db, usersRepo)
		{
			this.visitsRepo = visitsRepo;
			this.userQuizzesRepo = userQuizzesRepo;
			this.additionalScoresRepo = additionalScoresRepo;
			this.courseRolesRepo = courseRolesRepo;
			this.groupAccessesRepo = groupAccessesRepo;
			this.groupMembersRepo = groupMembersRepo;
			this.slideCheckingsRepo = slideCheckingsRepo;
		}

		/// <summary>
		/// Прогресс пользователя в курсе 
		/// </summary>
		[HttpPost("{courseId}")]
		[Authorize]
		public async Task<ActionResult<UsersProgressResponse>> UserProgress([FromRoute]string courseId, [FromBody]UserProgressParameters parameters)
		{
			if (!await courseManager.HasCourseAsync(courseId))
				return NotFound(new ErrorResponse($"Course {courseId} not found"));
			var course = await courseManager.FindCourseAsync(courseId);
			var userIds = parameters.UserIds;
			if (userIds == null || userIds.Count == 0)
				userIds = new List<string> { UserId };
			else
			{
				var userIdsWithProgressNotVisibleForUser = await GetUserIdsWithProgressNotVisibleForUser(course.Id, userIds);
				if (userIdsWithProgressNotVisibleForUser?.Any() ?? false)
				{
					var userIdsStr = string.Join(", ", userIdsWithProgressNotVisibleForUser);
					return NotFound(new ErrorResponse($"Users {userIdsStr} not found"));
				}
			}
			var isInstructor = await courseRolesRepo.HasUserAccessToCourseAsync(UserId, courseId, CourseRoleType.Instructor).ConfigureAwait(false);
			var visibleSlides = course.GetSlides(isInstructor).Select(s => s.Id).ToHashSet();

			var scores = await visitsRepo.GetScoresForSlides(course.Id, userIds);
			var additionalScores = await GetAdditionalScores(course.Id, userIds).ConfigureAwait(false);
			var attempts = await userQuizzesRepo.GetUsedAttemptsCountAsync(course.Id, userIds).ConfigureAwait(false);
			var waitingQuizSlides = await userQuizzesRepo.GetSlideIdsWaitingForManualCheckAsync(course.Id, userIds).ConfigureAwait(false);
			var waitingExerciseSlides = await slideCheckingsRepo.GetSlideIdsWaitingForManualExerciseCheckAsync(course.Id, userIds).ConfigureAwait(false);

			var usersProgress = new Dictionary<string, UserProgress>();
			foreach (var userId in scores.Keys)
			{
				var visitedSlides
					= scores[userId]
						.Where(kvp => visibleSlides.Contains(kvp.Key))
						.ToDictionary(kvp => kvp.Key, kvp => new UserProgressSlideResult
					{
						Visited = true,
						Score = kvp.Value,
						UsedAttempts = attempts.GetValueOrDefault(userId)?.GetValueOrDefault(kvp.Key) ?? 0,
						IsWaitingForManualChecking = (waitingExerciseSlides.GetValueOrDefault(userId)?.Contains(kvp.Key) ?? false)
							|| (waitingQuizSlides.GetValueOrDefault(userId)?.Contains(kvp.Key) ?? false)
					});
				var userAdditionalScores = additionalScores.GetValueOrDefault(userId);
					usersProgress[userId] = new UserProgress
				{
					VisitedSlides = visitedSlides,
					AdditionalScores = userAdditionalScores
				};
			}

			return new UsersProgressResponse
			{
				UserProgress = usersProgress,
			};
		}
		
		[ItemCanBeNull]
		private async Task<List<string>> GetUserIdsWithProgressNotVisibleForUser(string courseId, List<string> userIds)
		{
			var isSystemAdministrator = await IsSystemAdministratorAsync().ConfigureAwait(false);
			if (isSystemAdministrator)
				return null;
			if (await groupAccessesRepo.CanUserSeeAllCourseGroupsAsync(UserId, courseId))
				return null;
			var userRole = await courseRolesRepo.GetRoleAsync(UserId, courseId).ConfigureAwait(false);
			var groups = userRole == CourseRoleType.Instructor ? await groupAccessesRepo.GetAvailableForUserGroupsAsync(courseId, UserId, false, true, false) : new List<Group>();
			groups = groups
				.Concat((await groupMembersRepo.GetUserGroupsAsync(courseId, UserId, false)).Where(g=> g.CanUsersSeeGroupProgress))
				.Distinct().ToList();
			var members = new []{UserId}.Concat(await groupMembersRepo.GetGroupsMembersIdsAsync(groups.Select(g => g.Id).ToList())).ToHashSet();
			var allIdsInMembers = members.IsSupersetOf(userIds);
			if (allIdsInMembers)
				return null;
			var notVisibleUserIds = userIds.ToHashSet();
			notVisibleUserIds.ExceptWith(members);
			return notVisibleUserIds.ToList();
		}
		
		private async Task<Dictionary<string, Dictionary<Guid, Dictionary<string, int>>>> GetAdditionalScores(string courseId, List<string> userIds)
		{
			return (await additionalScoresRepo.GetAdditionalScoresForUsers(courseId, userIds).ConfigureAwait(false))
				.Select(kvp => (userId: kvp.Key.Item1, unitId: kvp.Key.Item2, scoringGroupId: kvp.Key.Item3, additionalScore: kvp.Value.Score))
				.GroupBy(t => t.userId)
				.ToDictSafe(g => g.Key,
					g =>
					{
						return g.GroupBy(t => t.unitId)
							.ToDictSafe(g2 => g2.Key,
								g2 =>
									g.ToDictSafe(t => t.scoringGroupId, t => t.additionalScore));
					});
		}

		/// <summary>
		/// Отметить посещение слайда в курсе
		/// </summary>
		/// <returns></returns>
		[HttpPost("{courseId}/visit/{slideId}")]
		[Authorize]
		public async Task<ActionResult<UsersProgressResponse>> Visit([FromRoute] Course course, [FromRoute] Guid slideId)
		{
			var isInstructor = await courseRolesRepo.HasUserAccessToCourseAsync(UserId, course.Id, CourseRoleType.Instructor).ConfigureAwait(false);
			var slide = course.FindSlideById(slideId, isInstructor);
			if (slide == null)
			{
				var instructorNote = course.FindInstructorNoteById(slideId);
				if (instructorNote != null && isInstructor)
					slide = instructorNote.Slide;
			}
			if (slide == null)
				return StatusCode((int)HttpStatusCode.NotFound, $"No slide with id {slideId}");
			await visitsRepo.AddVisit(course.Id, slideId, UserId, GetRealClientIp());
			return await UserProgress(course.Id, new UserProgressParameters());
		}

		private string GetRealClientIp()
		{
			var xForwardedFor = Request.Headers["X-Forwarded-For"].ToString();
			if (string.IsNullOrEmpty(xForwardedFor))
				return Request.Host.Host;
			return xForwardedFor.Split(',').FirstOrDefault() ?? "";
		}
	}
}