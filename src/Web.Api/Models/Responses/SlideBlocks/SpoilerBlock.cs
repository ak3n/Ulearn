﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using Ulearn.Core.Courses.Slides.Blocks;

namespace Ulearn.Web.Api.Models.Responses.SlideBlocks
{
	[DataContract]
	public class SpoilerBlockResponse : IApiSlideBlock
	{
		[DefaultValue(false)]
		[DataMember(Name = "hide", EmitDefaultValue = false)]
		public bool Hide { get; set; }

		[DataMember]
		public string Text { get; set; }

		[DataMember(Name = "hideQuizButton", EmitDefaultValue = false)]
		public bool HideQuizButton { get; set; }

		[DataMember(Name = "closable")]
		public bool Closable { get; set; }

		[DataMember(Name = "blocks")]
		public List<IApiSlideBlock> InnerBlocks { get; set; }

		[DataMember(Name = "type")]
		public string Type { get; set; } = "spoiler";

		public SpoilerBlockResponse(SpoilerBlock spoilerBlock, List<IApiSlideBlock> innerBlocks)
		{
			Hide = spoilerBlock.Hide;
			Text = spoilerBlock.Text;
			HideQuizButton = spoilerBlock.HideQuizButton;
			Closable = spoilerBlock.Closable;
			InnerBlocks = innerBlocks;
		}
	}
}