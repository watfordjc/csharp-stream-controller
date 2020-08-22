using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace uk.JohnCook.dotnet.StreamController
{
    public partial class MenuItemCollection : ResourceDictionary, ICollection<KeyValuePair<string, object>>
    {
        public static readonly RoutedUICommand routedConnectionMenuCommand = new RoutedUICommand("ConnectionMenuItemCommand", "ConnectionMenuItemCommand", typeof(MenuItemCollection));
        public static readonly RoutedUICommand routedWindowMenuCommand = new RoutedUICommand("WindowMenuItemCommand", "WindowMenuItemCommand", typeof(MenuItemCollection));

        private void MenuItemClose_Click(object sender, RoutedEventArgs e)
        {
            Window parent = Window.GetWindow((MenuItem)sender);
            parent.Close();
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void WindowMenuItemCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            WindowUtilityLibrary.WindowType windowType = (WindowUtilityLibrary.WindowType)Enum.Parse(typeof(WindowUtilityLibrary.WindowType), (e.OriginalSource as MenuItem).Name);
            WindowUtilityLibrary.MakeWindowActive(windowType);
        }

        private void WindowMenuItemCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            Window parent = Window.GetWindow((MenuItem)e.OriginalSource);
            e.CanExecute =
                (e.OriginalSource as MenuItem).Name !=
                Enum.GetName(typeof(WindowUtilityLibrary.WindowType), WindowUtilityLibrary.GetWindowTypeEnum(parent.GetType()));
        }

        private void ConnectItem_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as MenuItem).Name)
            {
                case "AutoReconnect":
                    Preferences.Default.obs_auto_reconnect = !(sender as MenuItem).IsChecked;
                    Preferences.Default.Save();
                    e.Handled = true;
                    break;
                default:
                    break;
            }
        }

        private void ConnectItemLoaded(object sender, RoutedEventArgs e)
        {
            if (sender == null) { throw new ArgumentNullException(nameof(sender)); }

            Window parent = Window.GetWindow((MenuItem)sender);
            WindowUtilityLibrary.WindowType windowType = WindowUtilityLibrary.GetWindowTypeEnum(parent.GetType());
            if (windowType != WindowUtilityLibrary.WindowType.Audiocheck)
            {
                (sender as MenuItem).Visibility = Visibility.Collapsed;
            }
        }

        public void Add(KeyValuePair<string, object> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return Contains(item.Key);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            KeyValuePair<string, object>[] newArray = array;
            ((ICollection<KeyValuePair<string, object>>)this).CopyTo(newArray, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            if (Contains(item.Key))
            {
                Remove(item.Key);
                return true;
            }
            else
            {
                return false;
            }
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return GetEnumerator() as IEnumerator<KeyValuePair<string, object>>;
        }
    }
}
