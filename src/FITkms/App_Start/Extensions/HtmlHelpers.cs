using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FITkms.App_Start.Extensions
{
    public static class HtmlHelpers
    {
        public static string Truncate (this HtmlHelper helper, string input, int length)
        {
            if (input.Length <= length)
            {
                return input;
            }
            return input.Substring(0, length)+"...";
        
        }

        public static object Truncate(IHtmlString htmlString, int length)
        {
            if (htmlString.ToHtmlString().Length <= length)
            {
                return htmlString.ToHtmlString();
            }
            return htmlString.ToHtmlString().Substring(0, length) + "...";
        }
    }
}