using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace StreamController
{
    public partial class MenuItemCollection : ResourceDictionary, ICollection<KeyValuePair<string, object>>
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

        private void MenuWindowItemLoaded(object sender, RoutedEventArgs e)
        {
            if (sender == null) { throw new ArgumentNullException(nameof(sender)); }

            WindowUtilityLibrary.WindowType clickedWindowType = (WindowUtilityLibrary.WindowType)Enum.Parse(typeof(WindowUtilityLibrary.WindowType), ((MenuItem)sender).Name);
            Window parent = Window.GetWindow((MenuItem)sender);
            WindowUtilityLibrary.WindowType windowType = WindowUtilityLibrary.GetWindowTypeEnum(parent.GetType());
            if (clickedWindowType == windowType)
            {
                ((MenuItem)sender).IsEnabled = false;
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
            if (Contains(item.Key)) {
                Remove(item.Key);
                return true;
            } else
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
