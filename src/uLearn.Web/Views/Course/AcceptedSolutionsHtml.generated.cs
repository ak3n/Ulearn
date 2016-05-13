﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace uLearn.Web.Views.Course
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Web;
    using System.Web.Helpers;
    using System.Web.Mvc;
    using System.Web.Mvc.Ajax;
    using System.Web.Mvc.Html;
    using System.Web.Routing;
    using System.Web.Security;
    using System.Web.UI;
    using System.Web.WebPages;
    using uLearn;
    using uLearn.Model.Blocks;
    using uLearn.Web.Models;
    using uLearn.Web.Views.Course;
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    public static class AcceptedSolutionsHtml
    {

public static System.Web.WebPages.HelperResult AcceptedSolutions(AcceptedSolutionsPageModel model) {
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {


                                                             

WebViewPage.WriteLiteralTo(@__razor_helper_writer, "\t<div id=\"LikeSolutionUrl\" data-url=\"");


WebViewPage.WriteTo(@__razor_helper_writer, model.LikeSolutionUrl);

WebViewPage.WriteLiteralTo(@__razor_helper_writer, "\"></div>\r\n");



WebViewPage.WriteLiteralTo(@__razor_helper_writer, "\t<p>");


WebViewPage.WriteTo(@__razor_helper_writer, MvcHtmlString.Create(model.Slide.Exercise.CommentAfterExerciseIsSolved.RenderMd(model.Slide.Info.SlideFile)));

WebViewPage.WriteLiteralTo(@__razor_helper_writer, "</p>\r\n");



WebViewPage.WriteLiteralTo(@__razor_helper_writer, "\t<p>Изучите решения ваших коллег. Проголосуйте за решения, в которых вы нашли что" +
"-то новое для себя.</p>\r\n");


	foreach (var solution in model.AcceptedSolutions)
	{
		var id = "solution_" + solution.Id;
		var code = new CodeBlock(solution.Code, model.Slide.Exercise.LangId);

WebViewPage.WriteLiteralTo(@__razor_helper_writer, "\t\t<div id=\"");


WebViewPage.WriteTo(@__razor_helper_writer, id);

WebViewPage.WriteLiteralTo(@__razor_helper_writer, "\">\r\n\t\t\t<button class=\"like-left-location btn ");


WebViewPage.WriteTo(@__razor_helper_writer, solution.LikedAlready ? "btn-primary" : "btn-default");

WebViewPage.WriteLiteralTo(@__razor_helper_writer, " like-button\" onclick=\"likeSolution(");


                                                                                          WebViewPage.WriteTo(@__razor_helper_writer, solution.Id);

WebViewPage.WriteLiteralTo(@__razor_helper_writer, ")\">\r\n\t\t\t\t<i class=\"glyphicon glyphicon-heart\"></i>\r\n\t\t\t\t<span class=\"likes-counte" +
"r\">");


WebViewPage.WriteTo(@__razor_helper_writer, solution.UsersWhoLike.Count);

WebViewPage.WriteLiteralTo(@__razor_helper_writer, "</span>\r\n\t\t\t</button>\r\n\r\n");


 			if (model.User.HasAccessFor(model.CourseId, CourseRole.Instructor))
			{

WebViewPage.WriteLiteralTo(@__razor_helper_writer, "\t\t\t\t<form action=\"");


WebViewPage.WriteTo(@__razor_helper_writer, solution.RemoveSolutionUrl);

WebViewPage.WriteLiteralTo(@__razor_helper_writer, "\" method=\"POST\" novalidate=\"novalidate\">\r\n\t\t\t\t\t<button class=\"btn btn-danger\">\r\n\t" +
"\t\t\t\t\t<i class=\"glyphicon glyphicon-remove\"></i>\r\n\t\t\t\t\t\tУдалить решение\r\n\t\t\t\t\t</b" +
"utton>\r\n\t\t\t\t</form>\r\n");


			}

WebViewPage.WriteLiteralTo(@__razor_helper_writer, "\r\n\t\t\t");


WebViewPage.WriteTo(@__razor_helper_writer, SlideHtml.Block(code, null));

WebViewPage.WriteLiteralTo(@__razor_helper_writer, "\r\n\t\t</div>\r\n");


	}

});

}


    }
}
#pragma warning restore 1591
