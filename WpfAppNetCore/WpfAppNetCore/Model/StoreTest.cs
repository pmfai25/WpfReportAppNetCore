using System;
using System.Collections.Generic;
using System.Text;

namespace WpfAppNetCore.Model
{
    /// <summary>
    /// 查詢結果物件
    /// </summary>
    public class StoreTest
    {
        public string Id { get; set; }
        public string StoreName { get; set; }
        public string Address { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
