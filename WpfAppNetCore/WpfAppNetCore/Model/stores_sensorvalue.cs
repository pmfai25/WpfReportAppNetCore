using System;
using System.Collections.Generic;
using System.Text;

namespace WpfAppNetCore.Model
{
    public class stores_sensorvalue
    {
        public int id { get; set; }
        public double? value1 { get; set; }
        public double? value2 { get; set; }
        public double? value3 { get; set; }
        public DateTime dt { get; set; }
        public string note { get; set; }
        public int device_id { get; set; }
        public int sensor_id { get; set; }
        public int topology_id { get; set; }
    }
}
