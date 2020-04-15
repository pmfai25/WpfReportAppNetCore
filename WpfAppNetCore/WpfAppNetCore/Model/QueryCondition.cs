using System;
using System.Collections.Generic;
using System.Text;

namespace WpfAppNetCore.Model
{
    /// <summary>
    /// 查詢條件物件
    /// </summary>
    public class QueryCondition
    {
        public string StoreName { get; set; }
        
        public string DateType { get; set; }

        public string ExportType { get; set; }

        public DateTime ReportFrom { get; set; }
        
        public DateTime ReportTo { get; set; }
    }
}
