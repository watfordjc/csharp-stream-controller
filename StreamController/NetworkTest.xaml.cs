using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using uk.JohnCook.dotnet.NetworkingWrapperLibrary;
using uk.JohnCook.dotnet.StreamController.Controls;

namespace uk.JohnCook.dotnet.StreamController
{
    /// <summary>
    /// Interaction logic for NetworkTest.xaml
    /// </summary>
    public partial class NetworkTest : StyledWindow
    {
        public NetworkTest()
        {
            InitializeComponent();
            DataContext = NetworkHelper.Instance.ConnectionInformation;
        }
    }
}
