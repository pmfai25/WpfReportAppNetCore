using System;
using System.Collections.Generic;
using System.Text;

namespace WpfAppNetCore.Model
{
    public class QueryConditionsReport
    {
        public List<stores_store> QueryConditionStores { get; set; }
        public List<QueryDeviceItem> QueryDeviceItems { get; set; }
        public List<MailUserInfo> MailToUsers { get; set; }
        public DateTime ReportDateFrom { get; set; }
        public DateTime ReportDateTo { get; set; }
        public bool IsSensorReport { get; set; }
        public bool IsStoreReport { get; set; }
    }
}
