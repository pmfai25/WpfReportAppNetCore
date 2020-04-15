using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace WpfAppNetCore.Model
{
    public class QueryDeviceItem : INotifyPropertyChanged
    {
        private int _sensor_real_id;
        private string _sensor_id;
        private string _sensor;
        private int _id;
        private string _device;

        /// <summary>
        /// stores_sensor表的id欄位
        /// </summary>
        public int sensor_real_id
        {
            get { return _sensor_real_id; }
            set
            {
                _sensor_real_id = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// stores_sensor表的sensor_id欄位
        /// </summary>
        public string sensor_id
        {
            get { return _sensor_id; }
            set
            {
                _sensor_id = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// stores_sensor表的title欄位
        /// </summary>
        public string sensor
        {
            get { return _sensor; }
            set
            {
                _sensor = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// stores_device表的id欄位
        /// </summary>
        public int id
        {
            get { return _id; }
            set
            {
                _id = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// stores_device表的device_id欄位
        /// </summary>
        public string device
        {
            get { return _device; }
            set
            {
                _device = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
