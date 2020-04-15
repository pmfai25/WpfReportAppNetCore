using System;
using System.Collections.Generic;
using System.Text;

namespace WpfAppNetCore.Model
{
    public class stores_sensor
    {
        public int id { get; set; }
        public string sensor_id { get; set; }
        public string title { get; set; }
        public bool is_virtual { get; set; }
        public string eq { get; set; }
        public string detail { get; set; }
        public int device_type_id { get; set; }
    }
}
