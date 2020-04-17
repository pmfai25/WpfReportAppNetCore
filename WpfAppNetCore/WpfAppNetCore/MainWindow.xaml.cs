using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfAppNetCore.Model;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace WpfAppNetCore
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #region Global Variable
        static HttpClient client;
        readonly static string webapiUrl = ConfigurationManager.AppSettings["WebapiUrl"];
        ObservableCollection<stores_store> allStoreList = new ObservableCollection<stores_store>();
        string StoreListString = string.Empty;
        #endregion

        #region Properties
        private ObservableCollection<MailUserInfo> _userList = new ObservableCollection<MailUserInfo>();
        private ObservableCollection<stores_devicetype> _deviceTypeList = new ObservableCollection<stores_devicetype>();
        private ObservableCollection<stores_store> _queryStoreList = new ObservableCollection<stores_store>();
        private ObservableCollection<stores_sensor> _sensorList = new ObservableCollection<stores_sensor>();
        private ObservableCollection<QueryDeviceItem> _queryDeviceItemList = new ObservableCollection<QueryDeviceItem>();
        private bool _IsAllStore;
        private bool _IsDailyReport = true;
        private bool _IsMonthlyReport;
        private bool _IsSensorReport = true;
        private bool _IsStoreReport = true;
        private DateTime _ReportDateFrom = DateTime.Today;
        private DateTime _ReportDateTo = DateTime.Today;
        private ConfigDeviceTypes _DeviceTypes = new ConfigDeviceTypes();

        public ConfigDeviceTypes DeviceTypes
        {
            get { return _DeviceTypes; }
            set
            {
                _DeviceTypes = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged();
            }
        }

        public ObservableCollection<MailUserInfo> userList
        {
            get { return _userList; }
            set
            {
                _userList = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 設備列表
        /// </summary>
        public ObservableCollection<stores_devicetype> deviceTypeList
        {
            get { return _deviceTypeList; }
            set
            {
                _deviceTypeList = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 店面列表
        /// </summary>
        public ObservableCollection<stores_store> queryStoreList
        {
            get { return _queryStoreList; }
            set
            {
                _queryStoreList = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 感測器列表
        /// </summary>
        public ObservableCollection<stores_sensor> sensorList
        {
            get { return _sensorList; }
            set
            {
                _sensorList = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 查詢列表
        /// </summary>
        public ObservableCollection<QueryDeviceItem> queryDeviceItemList
        {
            get { return _queryDeviceItemList; }
            set
            {
                _queryDeviceItemList = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 顯示所有店面
        /// </summary>
        public bool IsAllStore
        {
            get { return _IsAllStore; }
            set
            {
                _IsAllStore = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged();

                if (value)
                {
                    lblSelectResultAll.Visibility = Visibility.Visible;
                    lblSelectResult.Visibility = Visibility.Collapsed;
                }
                else
                {
                    lblSelectResultAll.Visibility = Visibility.Collapsed;
                    lblSelectResult.Visibility = Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// 分店報表
        /// </summary>
        public bool IsStoreReport
        {
            get { return _IsStoreReport; }
            set
            {
                _IsStoreReport = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 感測器報表
        /// </summary>
        public bool IsSensorReport
        {
            get { return _IsSensorReport; }
            set
            {
                _IsSensorReport = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 日報表
        /// </summary>
        public bool IsDailyReport
        {
            get { return _IsDailyReport; }
            set
            {
                _IsDailyReport = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 月報表
        /// </summary>
        public bool IsMonthlyReport
        {
            get { return _IsMonthlyReport; }
            set
            {
                _IsMonthlyReport = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 開始時間
        /// </summary>
        public DateTime ReportDateFrom
        {
            get { return _ReportDateFrom; }
            set
            {
                _ReportDateFrom = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 結束時間
        /// </summary>
        public DateTime ReportDateTo
        {
            get { return _ReportDateTo; }
            set
            {
                _ReportDateTo = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged();
            }
        }
        #endregion

        private async void Window_Activated(object sender, EventArgs e)
        {
            try
            {
                baseGrid.IsEnabled = false;
                #region 載入Sensor資料
                await GetSensorInfo();
                #endregion
                baseGrid.IsEnabled = true;
            }
            catch (Exception ex)
            {
                if (MessageBox.Show(ex.Message) == MessageBoxResult.OK)
                    this.Close();
            }
        }

        private async Task<bool> GetSensorInfo()
        {
            try
            {
                switch (ConfigurationManager.AppSettings["SensorListSource"])
                {
                    case "0":
                        client = new HttpClient();
                        var queryCondition = new QueryCondition() { StoreName = tbxStoreNameCondition.Text };
                        var dataAsString = JsonConvert.SerializeObject(queryCondition);
                        var content = new StringContent(dataAsString);
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                        var apiResponse = await client.PostAsync($"{webapiUrl}/sensor", content);
                        apiResponse.EnsureSuccessStatusCode();

                        var queryResult = (JsonElement)await JsonSerializer.DeserializeAsync<object>(await apiResponse.Content.ReadAsStreamAsync());

                        var resultObj = new List<stores_sensor>();
                        for (int i = 0; i < queryResult.GetArrayLength(); i++)
                            resultObj.Add(JsonConvert.DeserializeObject<stores_sensor>(queryResult[i].ToString()));

                        sensorList.Clear();

                        resultObj.ForEach(z =>
                        {
                            sensorList.Add(new stores_sensor { id = z.id, sensor_id = z.sensor_id, title = z.title });
                        });
                        break;
                    case "1":
                        sensorList.Clear();


                        LoadDeciceTypeToMemory();


                        //JsonConvert.DeserializeObject<List<stores_sensor>>($@"{ConfigurationManager.AppSettings["VisibaleSensors"]}").ToList().ForEach(z =>
                        //{
                        //    sensorList.Add(new stores_sensor { id = z.id, sensor_id = z.sensor_id, title = z.title });
                        //});
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return true;
        }

        private void LoadDeciceTypeToMemory()
        {
            DeviceTypes.ACTRL.sensors.ForEach(z =>
            {
                if (sensorList.Where(x => x.sensor_id == z.sensor_id && x.title == z.title).FirstOrDefault() == null)
                    sensorList.Add(new stores_sensor { sensor_id = z.sensor_id, title = z.title });
            });

            DeviceTypes.DAO.sensors.ForEach(z =>
            {
                if (sensorList.Where(x => x.sensor_id == z.sensor_id && x.title == z.title).FirstOrDefault() == null)
                    sensorList.Add(new stores_sensor { sensor_id = z.sensor_id, title = z.title });
            });

            DeviceTypes.PA33.sensors.ForEach(z =>
            {
                if (sensorList.Where(x => x.sensor_id == z.sensor_id && x.title == z.title).FirstOrDefault() == null)
                    sensorList.Add(new stores_sensor { sensor_id = z.sensor_id, title = z.title });
            });

            DeviceTypes.TC_2_TEMP.sensors.ForEach(z =>
            {
                if (sensorList.Where(x => x.sensor_id == z.sensor_id && x.title == z.title).FirstOrDefault() == null)
                    sensorList.Add(new stores_sensor { sensor_id = z.sensor_id, title = z.title });
            });

            DeviceTypes.TC_BENTO.sensors.ForEach(z =>
            {
                if (sensorList.Where(x => x.sensor_id == z.sensor_id && x.title == z.title).FirstOrDefault() == null)
                    sensorList.Add(new stores_sensor { sensor_id = z.sensor_id, title = z.title });
            });

            DeviceTypes.TC_BENTO_DESK.sensors.ForEach(z =>
            {
                if (sensorList.Where(x => x.sensor_id == z.sensor_id && x.title == z.title).FirstOrDefault() == null)
                    sensorList.Add(new stores_sensor { sensor_id = z.sensor_id, title = z.title });
            });

            DeviceTypes.TC_CAKE.sensors.ForEach(z =>
            {
                if (sensorList.Where(x => x.sensor_id == z.sensor_id && x.title == z.title).FirstOrDefault() == null)
                    sensorList.Add(new stores_sensor { sensor_id = z.sensor_id, title = z.title });
            });

            DeviceTypes.TC_CHILD.sensors.ForEach(z =>
            {
                if (sensorList.Where(x => x.sensor_id == z.sensor_id && x.title == z.title).FirstOrDefault() == null)
                    sensorList.Add(new stores_sensor { sensor_id = z.sensor_id, title = z.title });
            });

            DeviceTypes.TC_COFFEE.sensors.ForEach(z =>
            {
                if (sensorList.Where(x => x.sensor_id == z.sensor_id && x.title == z.title).FirstOrDefault() == null)
                    sensorList.Add(new stores_sensor { sensor_id = z.sensor_id, title = z.title });
            });

            DeviceTypes.TC_DRAWER.sensors.ForEach(z =>
            {
                if (sensorList.Where(x => x.sensor_id == z.sensor_id && x.title == z.title).FirstOrDefault() == null)
                    sensorList.Add(new stores_sensor { sensor_id = z.sensor_id, title = z.title });
            });

            DeviceTypes.TC_EC.sensors.ForEach(z =>
            {
                if (sensorList.Where(x => x.sensor_id == z.sensor_id && x.title == z.title).FirstOrDefault() == null)
                    sensorList.Add(new stores_sensor { sensor_id = z.sensor_id, title = z.title });
            });

            DeviceTypes.TC_EC_4C.sensors.ForEach(z =>
            {
                if (sensorList.Where(x => x.sensor_id == z.sensor_id && x.title == z.title).FirstOrDefault() == null)
                    sensorList.Add(new stores_sensor { sensor_id = z.sensor_id, title = z.title });
            });

            DeviceTypes.TC_ICE.sensors.ForEach(z =>
            {
                if (sensorList.Where(x => x.sensor_id == z.sensor_id && x.title == z.title).FirstOrDefault() == null)
                    sensorList.Add(new stores_sensor { sensor_id = z.sensor_id, title = z.title });
            });

            DeviceTypes.TC_LIGHT_FOOD.sensors.ForEach(z =>
            {
                if (sensorList.Where(x => x.sensor_id == z.sensor_id && x.title == z.title).FirstOrDefault() == null)
                    sensorList.Add(new stores_sensor { sensor_id = z.sensor_id, title = z.title });
            });

            DeviceTypes.TC_MILK.sensors.ForEach(z =>
            {
                if (sensorList.Where(x => x.sensor_id == z.sensor_id && x.title == z.title).FirstOrDefault() == null)
                    sensorList.Add(new stores_sensor { sensor_id = z.sensor_id, title = z.title });
            });

            DeviceTypes.TC_OC.sensors.ForEach(z =>
            {
                if (sensorList.Where(x => x.sensor_id == z.sensor_id && x.title == z.title).FirstOrDefault() == null)
                    sensorList.Add(new stores_sensor { sensor_id = z.sensor_id, title = z.title });
            });

            DeviceTypes.TC_OC18.sensors.ForEach(z =>
            {
                if (sensorList.Where(x => x.sensor_id == z.sensor_id && x.title == z.title).FirstOrDefault() == null)
                    sensorList.Add(new stores_sensor { sensor_id = z.sensor_id, title = z.title });
            });

            DeviceTypes.TC_RI.sensors.ForEach(z =>
            {
                if (sensorList.Where(x => x.sensor_id == z.sensor_id && x.title == z.title).FirstOrDefault() == null)
                    sensorList.Add(new stores_sensor { sensor_id = z.sensor_id, title = z.title });
            });

            DeviceTypes.TC_RI_4C.sensors.ForEach(z =>
            {
                if (sensorList.Where(x => x.sensor_id == z.sensor_id && x.title == z.title).FirstOrDefault() == null)
                    sensorList.Add(new stores_sensor { sensor_id = z.sensor_id, title = z.title });
            });

            DeviceTypes.TC_SWEET_18C.sensors.ForEach(z =>
            {
                if (sensorList.Where(x => x.sensor_id == z.sensor_id && x.title == z.title).FirstOrDefault() == null)
                    sensorList.Add(new stores_sensor { sensor_id = z.sensor_id, title = z.title });
            });

            DeviceTypes.TC_SWEET_4C.sensors.ForEach(z =>
            {
                if (sensorList.Where(x => x.sensor_id == z.sensor_id && x.title == z.title).FirstOrDefault() == null)
                    sensorList.Add(new stores_sensor { sensor_id = z.sensor_id, title = z.title });
            });

            DeviceTypes.TC_WI.sensors.ForEach(z =>
            {
                if (sensorList.Where(x => x.sensor_id == z.sensor_id && x.title == z.title).FirstOrDefault() == null)
                    sensorList.Add(new stores_sensor { sensor_id = z.sensor_id, title = z.title });
            });

            DeviceTypes.TC_WORK_4C.sensors.ForEach(z =>
            {
                if (sensorList.Where(x => x.sensor_id == z.sensor_id && x.title == z.title).FirstOrDefault() == null)
                    sensorList.Add(new stores_sensor { sensor_id = z.sensor_id, title = z.title });
            });

            DeviceTypes.TC_WORK_ICE.sensors.ForEach(z =>
            {
                if (sensorList.Where(x => x.sensor_id == z.sensor_id && x.title == z.title).FirstOrDefault() == null)
                    sensorList.Add(new stores_sensor { sensor_id = z.sensor_id, title = z.title });
            });
        }

        public MainWindow()
        {

            InitializeComponent();
            page2.Visibility = Visibility.Collapsed;
            page3.Visibility = Visibility.Collapsed;
            page4.Visibility = Visibility.Collapsed;

            this.DataContext = this;

            //讀取sensors.json
            string jsonFromFile;
            using (var reader = new StreamReader("sensors.json"))
            {
                jsonFromFile = reader.ReadToEnd();
                jsonFromFile = jsonFromFile.Replace("-", "_");
            }
            DeviceTypes = JsonConvert.DeserializeObject<ConfigDeviceTypes>(jsonFromFile);


        }

        private void switchPage(Grid preGide, Grid nextGrid)
        {
            Grid grid;
            preGide.Visibility = Visibility.Collapsed;
            grid = nextGrid;
            Canvas.SetLeft(grid, 0);
            Canvas.SetTop(grid, 0);
            nextGrid.Visibility = Visibility.Visible;
            grid.BringIntoView();
        }

        private void InitialPages()
        {
            StoreListString = string.Empty;
            tbxStoreNameList.Text = string.Empty;
            userList = new ObservableCollection<MailUserInfo>();
            deviceTypeList = new ObservableCollection<stores_devicetype>();
            queryStoreList = new ObservableCollection<stores_store>();
            
            //目前不需要清除
            //sensorList = new ObservableCollection<stores_sensor>();
            lbxSensor.SelectedIndex = -1;

            queryDeviceItemList = new ObservableCollection<QueryDeviceItem>();
            IsAllStore = false;
            IsDailyReport = true;
            IsMonthlyReport = false;
            IsSensorReport = true;
            IsStoreReport = true;
            ReportDateFrom = DateTime.Today;
            ReportDateTo = DateTime.Today;
        }

        private async Task GetDataAsync()
        {
            client = new HttpClient();

            HttpResponseMessage response = await client.GetAsync(webapiUrl); 
        }

        private async void btnStoreNameOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tbxStoreNameList.Text) && !IsAllStore)
                    MessageBox.Show("沒有選擇任何門市門市");
                else
                {
                   
                    if(IsAllStore)
                    {
                        baseGrid.IsEnabled = false;
                        allStoreList.Clear();

                        #region 如果選取所有門市
                        client = new HttpClient();

                        var apiResponseAllStore = await client.GetAsync($@"{webapiUrl}/store");
                        apiResponseAllStore.EnsureSuccessStatusCode();

                        var queryResultAllStore = (JsonElement)await JsonSerializer.DeserializeAsync<object>(await apiResponseAllStore.Content.ReadAsStreamAsync());

                        var resultObj = new List<stores_store>();
                        for (int i = 0; i < queryResultAllStore.GetArrayLength(); i++)
                            resultObj.Add(JsonConvert.DeserializeObject<stores_store>(queryResultAllStore[i].ToString()));

                        resultObj.ForEach(z =>
                        {
                            allStoreList.Add(new stores_store {  name = z.name, id = z.id });
                        });
                        #endregion
                        baseGrid.IsEnabled = true;
                    }
                    else
                    {
                        baseGrid.IsEnabled = false;
                        #region 驗證選擇的店面
                        client = new HttpClient();

                        var queryCondition = new QueryCondition() { StoreName = tbxStoreNameList.Text };
                        var dataAsString = JsonConvert.SerializeObject(queryCondition);
                        var content = new StringContent(dataAsString);
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                        HttpResponseMessage apiResponse = await client.PostAsync($"{webapiUrl}/store/validatestores", content);
                        apiResponse.EnsureSuccessStatusCode();


                        var queryResult = await apiResponse.Content.ReadAsStringAsync();

                        if (queryResult.ToString() != "true")
                        {
                            MessageBox.Show(queryResult.ToString());
                            return;
                        }
                        #endregion
                        baseGrid.IsEnabled = true;
                    }

                    switchPage(page1, page2);
                    StoreListString = tbxStoreNameList.Text;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnP2ToP1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switchPage(page2, page1);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void btnDeviceOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switchPage(page2, page3);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void btnP3ToP2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switchPage(page3, page2);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private List<stores_store> parseStoreListString(string source)
        {
            var result = new List<stores_store>();
            foreach (var item in source.Split('\r'))
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    var temp = item.Split(',');
                    result.Add(new stores_store { id = int.Parse(temp[1]), name = temp[0].ToString() });
                }  
            }

            return result;
        }

        private async void btnExportReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                baseGrid.IsEnabled = false;

                if(userList.Count() == 0)
                {
                    MessageBox.Show($"至少需要一個收件人");
                    return;
                }

                //確認email正確性
                foreach (var item in userList)
                {
                    if (!IsValidEmail(item.UserEmail))
                    {
                        MessageBox.Show($"收件人{item.UserName}的email格式不正確:{item.UserEmail}");
                        return;
                    }
                }

                #region 產生報表
                client = new HttpClient();

                var queryCondition = new QueryConditionsReport()
                {
                    QueryDeviceItems = queryDeviceItemList.ToList(),
                    QueryConditionStores = IsAllStore ? allStoreList.ToList() : parseStoreListString(StoreListString),
                    MailToUsers = userList.ToList(),
                    ReportDateFrom = new DateTime(this.ReportDateFrom.Year, this.ReportDateFrom.Month, this.ReportDateFrom.Day, 0, 0, 0),
                    ReportDateTo = new DateTime(this.ReportDateTo.Year, this.ReportDateTo.Month, this.ReportDateTo.Day, 23, 59, 59),
                    IsSensorReport = this.IsSensorReport,
                    IsStoreReport = this.IsStoreReport
                };

                var dataAsString = JsonConvert.SerializeObject(queryCondition);
                var content = new StringContent(dataAsString);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var apiResponse = await client.PostAsync($"{webapiUrl}/report", content);
                apiResponse.EnsureSuccessStatusCode();

                var queryResult = (JsonElement)await JsonSerializer.DeserializeAsync<object>(await apiResponse.Content.ReadAsStreamAsync());

                //若需要看回傳訊息再打開，並把api回傳型態改為List<stores_sensorvalue>
                //var resultObj = new List<stores_sensorvalue>();
                //for (int i = 0; i < queryResult.GetArrayLength(); i++)
                //    resultObj.Add(JsonConvert.DeserializeObject<stores_sensorvalue>(queryResult[i].ToString()));

                #endregion
                baseGrid.IsEnabled = true;

                switchPage(page3, page4);
            }
            catch (Exception ex)
            {
                baseGrid.IsEnabled = true;
                MessageBox.Show(ex.Message);
            }
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    var domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException e)
            {
                return false;
            }
            catch (ArgumentException e)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        private void btnAddMailUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //lbxMailUser.ItemsSource = userList;
                Random r = new Random();
                var userNo = r.Next(0, 100000).ToString();
                userList.Add(new MailUserInfo { UserName = $"User{userNo}", UserEmail = $"User{userNo}@gmail.com" });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }   
        }

        private async void btnStoreNameQuery_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                baseGrid.IsEnabled = false;
                queryStoreList = new ObservableCollection<stores_store>();

                client = new HttpClient();

                var queryCondition = new QueryCondition() { StoreName = tbxStoreNameCondition.Text };
                var dataAsString = JsonConvert.SerializeObject(queryCondition);
                var content = new StringContent(dataAsString);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                #region 呼叫Web Server上的S測試API
                //var apiResponseTest12345 = await client.GetAsync($@"http://13.76.178.165/chisuwebapi/weatherforecast");
                //apiResponseTest12345.EnsureSuccessStatusCode();
                //var queryResultTest12345 = (JsonElement)await JsonSerializer.DeserializeAsync<object>(await apiResponseTest12345.Content.ReadAsStreamAsync());
                #endregion

                var apiResponse = await client.PostAsync($"{webapiUrl}/store", content);
                apiResponse.EnsureSuccessStatusCode();

                var queryResult = (JsonElement)await JsonSerializer.DeserializeAsync<object>(await apiResponse.Content.ReadAsStreamAsync());

                var resultObj = new List<stores_store>();
                for (int i = 0; i < queryResult.GetArrayLength(); i++)
                    resultObj.Add(JsonConvert.DeserializeObject<stores_store>(queryResult[i].ToString()));

                resultObj.ForEach(z =>
                {
                    //lbl1.Content += $@"id:{z.Id}, StoreName:{z.StoreName}, Address:{z.Address}
//";
                    queryStoreList.Add(new stores_store { id = z.id, name = z.name, address = z.address });
                });
                baseGrid.IsEnabled = true;
                //lbxStore.ItemsSource = storeList;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void lbxbtnAddStoreToList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var addedItem = (stores_store)((ListBoxItem)lbxStore.ContainerFromElement((System.Windows.Controls.Button)sender)).Content;

                lbxStore.SelectedIndex = queryStoreList.ToList().IndexOf(addedItem);
                if (!tbxStoreNameList.Text.Contains($"{addedItem.name},{addedItem.id}"))
                {
                    StoreListString += $"{addedItem.name},{addedItem.id}\r";
                    ParseStoreNameList(StoreListString);
                }
                else
                    MessageBox.Show("門市已加入清單");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void tbStoreName_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var storeNameListString = ((TextBox)sender).Text;
                StoreListString = storeNameListString;
                if (storeNameListString.Contains("\n"))
                {
                    ParseStoreNameList(storeNameListString);
                    ((TextBox)sender).Select(storeNameListString.Length - 1, storeNameListString.Length - 1);
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void ParseStoreNameList(string sourceString)
        {
            var tempStoreList = sourceString.Split("\r").ToList();
            var resultStoreList = new List<string>();

            tempStoreList.ForEach(z => 
            {
                if (!string.IsNullOrWhiteSpace(z))
                    resultStoreList.Add(z);
            });

            StoreListString = string.Empty;
            resultStoreList.ForEach(z => 
            {
                StoreListString += $"{z}\r";
            });

            tbxStoreNameList.Text = StoreListString;

            lblSelectResult.Content = $"已選擇{resultStoreList.Count()}家門市";
        }

        private void tbxStoreNameList_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.Back || e.Key == Key.Delete)
            //{
            //    var currentIndex = ((TextBox)sender).SelectionStart;
            //    var storeNameListString = ((TextBox)sender).Text;

            //    ParseStoreNameList(storeNameListString);

            //    ((TextBox)sender).Select(currentIndex, currentIndex);
            //}            
        }

        private async void lbxSensor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if ((stores_sensor)((ListBox)sender).SelectedItem != null)
                {
                    baseGrid.IsEnabled = false;
                    #region 載入Device Type資料
                    switch (ConfigurationManager.AppSettings["SensorListSource"])
                    {
                        case "0":
                            var queryCondition = new QueryConditionDeviceType() { SensorId = ((stores_sensor)((ListBox)sender).SelectedItem).sensor_id };
                            var dataAsString = JsonConvert.SerializeObject(queryCondition);
                            var content = new StringContent(dataAsString);
                            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                            var apiResponse = await client.PostAsync($"{webapiUrl}/devicetype", content);
                            apiResponse.EnsureSuccessStatusCode();

                            var queryResult = (JsonElement)await JsonSerializer.DeserializeAsync<object>(await apiResponse.Content.ReadAsStreamAsync());

                            var resultObj = new List<stores_devicetype>();
                            for (int i = 0; i < queryResult.GetArrayLength(); i++)
                                resultObj.Add(JsonConvert.DeserializeObject<stores_devicetype>(queryResult[i].ToString()));

                            deviceTypeList.Clear();

                            resultObj.ForEach(z =>
                            {
                                deviceTypeList.Add(new stores_devicetype { id = z.id, title = z.title });
                            });
                            break;
                        case "1":
                            FindDeviceBySensorType(((stores_sensor)((ListBox)sender).SelectedItem).sensor_id);
                            break;
                    }

                    lbxDevice.ItemsSource = deviceTypeList;
                    #endregion
                    baseGrid.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                baseGrid.IsEnabled = true;
                MessageBox.Show(ex.Message);
                //throw;
            }
        }

        private void FindDeviceBySensorType(string sensorId)
        {
            deviceTypeList.Clear();

            if (DeviceTypes.ACTRL.sensors.Where(z => z.sensor_id == sensorId).Count() > 0)
            {
                if(deviceTypeList.Where(x => x.id == DeviceTypes.ACTRL.id && x.title == DeviceTypes.ACTRL.title).Count() ==0 )
                    deviceTypeList.Add(new stores_devicetype { id = DeviceTypes.ACTRL.id, title = DeviceTypes.ACTRL.title });
            }      

            if (DeviceTypes.DAO.sensors.Where(z => z.sensor_id == sensorId).Count() > 0)
            {
                if (deviceTypeList.Where(x => x.id == DeviceTypes.DAO.id && x.title == DeviceTypes.DAO.title).Count() == 0)
                    deviceTypeList.Add(new stores_devicetype { id = DeviceTypes.DAO.id, title = DeviceTypes.DAO.title });
            }

            if (DeviceTypes.PA33.sensors.Where(z => z.sensor_id == sensorId).Count() > 0)
            {
                if (deviceTypeList.Where(x => x.id == DeviceTypes.PA33.id && x.title == DeviceTypes.PA33.title).Count() == 0)
                    deviceTypeList.Add(new stores_devicetype { id = DeviceTypes.PA33.id, title = DeviceTypes.PA33.title });
            }

            if (DeviceTypes.TC_2_TEMP.sensors.Where(z => z.sensor_id == sensorId).Count() > 0)
            {
                if (deviceTypeList.Where(x => x.id == DeviceTypes.TC_2_TEMP.id && x.title == DeviceTypes.TC_2_TEMP.title).Count() == 0)
                    deviceTypeList.Add(new stores_devicetype { id = DeviceTypes.TC_2_TEMP.id, title = DeviceTypes.TC_2_TEMP.title });
            }

            if (DeviceTypes.TC_BENTO.sensors.Where(z => z.sensor_id == sensorId).Count() > 0)
            {
                if (deviceTypeList.Where(x => x.id == DeviceTypes.TC_BENTO.id && x.title == DeviceTypes.TC_BENTO.title).Count() == 0)
                    deviceTypeList.Add(new stores_devicetype { id = DeviceTypes.TC_BENTO.id, title = DeviceTypes.TC_BENTO.title });
            }

            if (DeviceTypes.TC_BENTO_DESK.sensors.Where(z => z.sensor_id == sensorId).Count() > 0)
            {
                if (deviceTypeList.Where(x => x.id == DeviceTypes.TC_BENTO_DESK.id && x.title == DeviceTypes.TC_BENTO_DESK.title).Count() == 0)
                    deviceTypeList.Add(new stores_devicetype { id = DeviceTypes.TC_BENTO_DESK.id, title = DeviceTypes.TC_BENTO_DESK.title });
            }

            if (DeviceTypes.TC_CAKE.sensors.Where(z => z.sensor_id == sensorId).Count() > 0)
            {
                if (deviceTypeList.Where(x => x.id == DeviceTypes.TC_CAKE.id && x.title == DeviceTypes.TC_CAKE.title).Count() == 0)
                    deviceTypeList.Add(new stores_devicetype { id = DeviceTypes.TC_CAKE.id, title = DeviceTypes.TC_CAKE.title });
            }

            if (DeviceTypes.TC_CHILD.sensors.Where(z => z.sensor_id == sensorId).Count() > 0)
            {
                if (deviceTypeList.Where(x => x.id == DeviceTypes.TC_CHILD.id && x.title == DeviceTypes.TC_CHILD.title).Count() == 0)
                    deviceTypeList.Add(new stores_devicetype { id = DeviceTypes.TC_CHILD.id, title = DeviceTypes.TC_CHILD.title });
            }

            if (DeviceTypes.TC_COFFEE.sensors.Where(z => z.sensor_id == sensorId).Count() > 0)
            {
                if (deviceTypeList.Where(x => x.id == DeviceTypes.TC_COFFEE.id && x.title == DeviceTypes.TC_COFFEE.title).Count() == 0)
                    deviceTypeList.Add(new stores_devicetype { id = DeviceTypes.TC_COFFEE.id, title = DeviceTypes.TC_COFFEE.title });
            }

            if (DeviceTypes.TC_DRAWER.sensors.Where(z => z.sensor_id == sensorId).Count() > 0)
            {
                if (deviceTypeList.Where(x => x.id == DeviceTypes.TC_DRAWER.id && x.title == DeviceTypes.TC_DRAWER.title).Count() == 0)
                    deviceTypeList.Add(new stores_devicetype { id = DeviceTypes.TC_DRAWER.id, title = DeviceTypes.TC_DRAWER.title });
            }

            if (DeviceTypes.TC_EC.sensors.Where(z => z.sensor_id == sensorId).Count() > 0)
            {
                if (deviceTypeList.Where(x => x.id == DeviceTypes.TC_EC.id && x.title == DeviceTypes.TC_EC.title).Count() == 0)
                    deviceTypeList.Add(new stores_devicetype { id = DeviceTypes.TC_EC.id, title = DeviceTypes.TC_EC.title });
            }

            if (DeviceTypes.TC_EC_4C.sensors.Where(z => z.sensor_id == sensorId).Count() > 0)
            {
                if (deviceTypeList.Where(x => x.id == DeviceTypes.TC_EC_4C.id && x.title == DeviceTypes.TC_EC_4C.title).Count() == 0)
                    deviceTypeList.Add(new stores_devicetype { id = DeviceTypes.TC_EC_4C.id, title = DeviceTypes.TC_EC_4C.title });
            }

            if (DeviceTypes.TC_ICE.sensors.Where(z => z.sensor_id == sensorId).Count() > 0)
            {
                if (deviceTypeList.Where(x => x.id == DeviceTypes.TC_ICE.id && x.title == DeviceTypes.TC_ICE.title).Count() == 0)
                    deviceTypeList.Add(new stores_devicetype { id = DeviceTypes.TC_ICE.id, title = DeviceTypes.TC_ICE.title });
            }

            if (DeviceTypes.TC_LIGHT_FOOD.sensors.Where(z => z.sensor_id == sensorId).Count() > 0)
            {
                if (deviceTypeList.Where(x => x.id == DeviceTypes.TC_LIGHT_FOOD.id && x.title == DeviceTypes.TC_LIGHT_FOOD.title).Count() == 0)
                    deviceTypeList.Add(new stores_devicetype { id = DeviceTypes.TC_LIGHT_FOOD.id, title = DeviceTypes.TC_LIGHT_FOOD.title });
            }

            if (DeviceTypes.TC_MILK.sensors.Where(z => z.sensor_id == sensorId).Count() > 0)
            {
                if (deviceTypeList.Where(x => x.id == DeviceTypes.TC_MILK.id && x.title == DeviceTypes.TC_MILK.title).Count() == 0)
                    deviceTypeList.Add(new stores_devicetype { id = DeviceTypes.TC_MILK.id, title = DeviceTypes.TC_MILK.title });
            }

            if (DeviceTypes.TC_OC.sensors.Where(z => z.sensor_id == sensorId).Count() > 0)
            {
                if (deviceTypeList.Where(x => x.id == DeviceTypes.TC_OC.id && x.title == DeviceTypes.TC_OC.title).Count() == 0)
                    deviceTypeList.Add(new stores_devicetype { id = DeviceTypes.TC_OC.id, title = DeviceTypes.TC_OC.title });
            }

            if (DeviceTypes.TC_OC18.sensors.Where(z => z.sensor_id == sensorId).Count() > 0)
            {
                if (deviceTypeList.Where(x => x.id == DeviceTypes.TC_OC18.id && x.title == DeviceTypes.TC_OC18.title).Count() == 0)
                    deviceTypeList.Add(new stores_devicetype { id = DeviceTypes.TC_OC18.id, title = DeviceTypes.TC_OC18.title });
            }

            if (DeviceTypes.TC_RI.sensors.Where(z => z.sensor_id == sensorId).Count() > 0)
            {
                if (deviceTypeList.Where(x => x.id == DeviceTypes.TC_RI.id && x.title == DeviceTypes.TC_RI.title).Count() == 0)
                    deviceTypeList.Add(new stores_devicetype { id = DeviceTypes.TC_RI.id, title = DeviceTypes.TC_RI.title });
            }

            if (DeviceTypes.TC_RI_4C.sensors.Where(z => z.sensor_id == sensorId).Count() > 0)
            {
                if (deviceTypeList.Where(x => x.id == DeviceTypes.TC_RI_4C.id && x.title == DeviceTypes.TC_RI_4C.title).Count() == 0)
                    deviceTypeList.Add(new stores_devicetype { id = DeviceTypes.TC_RI_4C.id, title = DeviceTypes.TC_RI_4C.title });
            }

            if (DeviceTypes.TC_SWEET_18C.sensors.Where(z => z.sensor_id == sensorId).Count() > 0)
            {
                if (deviceTypeList.Where(x => x.id == DeviceTypes.TC_SWEET_18C.id && x.title == DeviceTypes.TC_SWEET_18C.title).Count() == 0)
                    deviceTypeList.Add(new stores_devicetype { id = DeviceTypes.TC_SWEET_18C.id, title = DeviceTypes.TC_SWEET_18C.title });
            }

            if (DeviceTypes.TC_SWEET_4C.sensors.Where(z => z.sensor_id == sensorId).Count() > 0)
            {
                if (deviceTypeList.Where(x => x.id == DeviceTypes.TC_SWEET_4C.id && x.title == DeviceTypes.TC_SWEET_4C.title).Count() == 0)
                    deviceTypeList.Add(new stores_devicetype { id = DeviceTypes.TC_SWEET_4C.id, title = DeviceTypes.TC_SWEET_4C.title });
            }

            if (DeviceTypes.TC_WI.sensors.Where(z => z.sensor_id == sensorId).Count() > 0)
            {
                if (deviceTypeList.Where(x => x.id == DeviceTypes.TC_WI.id && x.title == DeviceTypes.TC_WI.title).Count() == 0)
                    deviceTypeList.Add(new stores_devicetype { id = DeviceTypes.TC_WI.id, title = DeviceTypes.TC_WI.title });
            }

            if (DeviceTypes.TC_WORK_4C.sensors.Where(z => z.sensor_id == sensorId).Count() > 0)
            {
                if (deviceTypeList.Where(x => x.id == DeviceTypes.TC_WORK_4C.id && x.title == DeviceTypes.TC_WORK_4C.title).Count() == 0)
                    deviceTypeList.Add(new stores_devicetype { id = DeviceTypes.TC_WORK_4C.id, title = DeviceTypes.TC_WORK_4C.title });
            }

            if (DeviceTypes.TC_WORK_ICE.sensors.Where(z => z.sensor_id == sensorId).Count() > 0)
            {
                if (deviceTypeList.Where(x => x.id == DeviceTypes.TC_WORK_ICE.id && x.title == DeviceTypes.TC_WORK_ICE.title).Count() == 0)
                    deviceTypeList.Add(new stores_devicetype { id = DeviceTypes.TC_WORK_ICE.id, title = DeviceTypes.TC_WORK_ICE.title });
            }
        }

        private void btnAddDevice_Click(object sender, RoutedEventArgs e)
        {
            var sensor = lbxSensor.SelectedItem as stores_sensor;
            var device = lbxDevice.SelectedItem as stores_devicetype;
            if(sensor != null && device != null)
                if(queryDeviceItemList.Where(z => z.sensor == sensor.title && z.device == device.title).Count() == 0)
                    queryDeviceItemList.Add(new QueryDeviceItem { sensor_id = sensor.sensor_id, sensor = sensor.title, id = device.id, sensor_real_id = sensor.id , device = device.title });
            //lbxQueryItem.Items.Add(new QueryDeviceItem { sensor = sensor.title, device = device.title });
        }

        private void btnDeleteDevice_Click(object sender, RoutedEventArgs e)
        {
            if(lbxQueryItem.SelectedItem != null)
            {
                var selectItem = lbxQueryItem.SelectedItem as QueryDeviceItem;
                queryDeviceItemList.Remove(selectItem);
            }
        }

        private void btnP4ToP1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //InitialPages();
                switchPage(page4, page1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void btnMailListDel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var deleteItem = (MailUserInfo)((ListBoxItem)lbxMailUser.ContainerFromElement((System.Windows.Controls.Button)sender)).Content;
                lbxMailUser.SelectedIndex = userList.ToList().IndexOf(deleteItem);


                if (lbxMailUser.SelectedItem != null)
                {
                    var selectItem = lbxMailUser.SelectedItem as MailUserInfo;
                    userList.Remove(selectItem);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
