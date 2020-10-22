﻿using System.Collections.Generic;
using Database.Models;
using Ulearn.Core.Courses.Slides.Exercises;
using Ulearn.Web.Api.Models.Responses.Exercise;

namespace Ulearn.Web.Api.Controllers.Slides
{
	public class ExerciseSlideRendererContext
	{
		public ExerciseSlide Slide;
		public List<UserExerciseSubmission> Submissions;
		public List<ExerciseCodeReviewComment> CodeReviewComments;
		public ExerciseAttemptsStatistics AttemptsStatistics;
	}
}