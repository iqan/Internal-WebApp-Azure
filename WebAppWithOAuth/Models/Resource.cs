using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebAppWithOAuth.Models
{
    public class Resource
    {
        public long ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string ResourceName { get; set; }
        public string BillingPeriod { get; set; }
        public int Rate { get; set; }
        public int Leaves { get; set; }
        public int BillingDays { get; set; }
        public int TotalBilling { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime LikelyEntensionTill { get; set; }
        public bool Extension { get; set; }
        public int OverMonth { get; set; }
        public string Worksheet { get; set; }

        public IEnumerable<SelectListItem> listworksheets { get; set; }
        public List<HttpPostedFileBase> Files { get; set; }

        public Resource()
        {
            Files = new List<HttpPostedFileBase>();
        }
    }
}