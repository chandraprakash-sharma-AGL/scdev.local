using Sitecore.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace scdev.local.project.Models
{
    public class Templates
    {
        public struct NewsArticle
        {
            public static readonly ID ID = new ID("{79297A41-EE90-4B07-BEEC-45C60ADF4D5B}");
            public struct Fields
            {
                public static readonly ID Title = new ID("{B929A922-F0AF-47EA-9672-8126E2F7CED2}");
                public const string Title_FieldName = "NewsTitle";
                public static readonly ID Image = new ID("{5B312CFC-6C09-4B0C-8B58-B77FA538CF02}");
                public static readonly ID Date = new ID("{F4BE0A68-BC52-4E98-A6ED-F619768CA61A}");
                public static readonly ID Summary = new ID("{F52D3AEF-5B55-48AD-8DE6-830FCF2B8CA3}");
                public const string Summary_FieldName = "NewsSummary";
                public static readonly ID Body = new ID("{04701F8A-3E81-4AF7-93BF-131A4F0305A0}");
                public const string Body_FieldName = "NewsBody";
            }
        }
    }
}