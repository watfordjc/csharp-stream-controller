using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace uk.JohnCook.dotnet.StreamController
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            StreamController.Properties.Resources.Culture = CultureInfo.CurrentCulture;
        }
    }
}
