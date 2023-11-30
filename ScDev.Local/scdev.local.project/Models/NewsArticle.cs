﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nest;
namespace scdev.local.project.Models
{
    public class NewsArticle
    {
        [Text]
        public string Id { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public string Date { get; set; }
        public string Summary { get; set; }
        public string Body { get; set; }
    }
}