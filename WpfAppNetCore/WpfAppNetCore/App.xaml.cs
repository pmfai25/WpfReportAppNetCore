using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WpfAppNetCore
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            //var test = AppDomain.CurrentDomain.BaseDirectory;
            //var test2 = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            ////var test3 = Application.Current.FindResource("pack://application:,,,/Resources/chisu.png");
            //var splash = new SplashScreen(@"Images\chisu.png");
            //splash.Show(true, true);

            base.OnStartup(e);
        }
    }
}
