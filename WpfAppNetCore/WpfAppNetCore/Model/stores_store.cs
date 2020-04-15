using System;
using System.Collections.Generic;
using System.Text;

namespace WpfAppNetCore.Model
{
    public class stores_store
    {
        public int id { get; set; }
        public int current_id { get; set; }
        public string name { get; set; }
        public string post_code { get; set; }
        public string city { get; set; }
        public string town { get; set; }
        public string address { get; set; }
        public string phone_code { get; set; }
        public string phone { get; set; }
        public int department_id { get; set; }
        public int? type_id { get; set; }
        public int vendor_id { get; set; }
        public int? electrician_id { get; set; }
    }
}
