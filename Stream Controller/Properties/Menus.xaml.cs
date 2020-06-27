using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Stream_Controller
{
    partial class MenuItems : ResourceDictionary
    {
        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Window parent = Window.GetWindow((MenuItem)sender);
            parent.Close();
        }

        private void MenuWindowItem_Click(object sender, RoutedEventArgs e)
        {
            WindowUtilityLibrary.WindowType clickedWindowType = (WindowUtilityLibrary.WindowType)Enum.Parse(typeof(WindowUtilityLibrary.WindowType), ((MenuItem)sender).Name);
            WindowUtilityLibrary.MakeWindowActive(clickedWindowType);
        }

        public void MenuWindowItem_Loaded(object sender, RoutedEventArgs e)
        {
            WindowUtilityLibrary.WindowType clickedWindowType = (WindowUtilityLibrary.WindowType)Enum.Parse(typeof(WindowUtilityLibrary.WindowType), ((MenuItem)sender).Name);
            Window parent = Window.GetWindow((MenuItem)sender);
            WindowUtilityLibrary.WindowType windowType = WindowUtilityLibrary.GetWindowTypeEnum(parent.GetType());
            if (clickedWindowType == windowType)
            {
                ((MenuItem)sender).IsEnabled = false;
            }
        }
    }
}
