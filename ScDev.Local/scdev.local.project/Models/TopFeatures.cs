using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace scdev.local.project.Models
{
    public class TopFeatures
    {
        public string CarModel { get; set; }
        public string CarModelCode { get; set; }
        public string CarModelCodeAlpha { get; set; }
        public string VariantCode { get; set; }
        public string VariantName { get; set; }
        public string DesktopImage { get; set; }
        public string MobileImage { get; set; }
        public string TopFeature { get; set; }
        public string TopSelected { get; set; }
    }
}