using System;
using System.Collections.Generic;
using System.Text;

namespace WpfAppNetCore.Model.DeviceTypes
{
    public class TC_RI
    {
        public int id { get; set; }
        public string title { get; set; }
        public List<sensor> sensors { get; set; }
    }
}
