using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace OBSWebSocketLibrary.Models.TypeDefs.FilterTypes
{
    public class BaseFilter : INotifyPropertyChanged
    {
        private string name;
        private OBSWebSocketLibrary.Data.SourceTypes type;
        private bool enabled;

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                NotifyPropertyChanged();
            }
        }

        public OBSWebSocketLibrary.Data.SourceTypes Type
        {
            get { return type; }
            set
            {
                type = value;
                NotifyPropertyChanged();
            }
        }

        public bool Enabled
        {
            get { return enabled; }
            set
            {
                enabled = value;
                NotifyPropertyChanged();
            }
        }

        public static implicit operator BaseFilter(RequestReplies.GetSourceFilters.Filter v)
        {
            BaseFilter converted = v.SettingsObj as BaseFilter;
            converted.Name = v.Name;
            converted.Type = (Data.SourceTypes)Enum.Parse(typeof(Data.SourceTypes), v.Type);
            converted.Enabled = v.Enabled;
            return converted;
        }
    }
}
