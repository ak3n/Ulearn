﻿using System;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace AntiPlagiarism.Web.Database.Models
{
	/* TODO (andgein): Add clientId to TaskStatisticsParameters? */
	public class TaskStatisticsParameters
	{
		[Key]
		public Guid TaskId { get; set; }

		public double Mean { get; set; }

		public double Deviation { get; set; }

		public int SubmissionsCount { get; set; }

		public DateTime? Timestamp { get; set; }
	}
}