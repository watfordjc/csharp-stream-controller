using OBSWebSocketLibrary.Data;
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
        private OBSWebSocketLibrary.Data.SourceType type;
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

        public OBSWebSocketLibrary.Data.SourceType Type
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

        public static implicit operator BaseFilter(TypeDefs.ObsReplyFilter v)
        {
            if (v == null) { return null; }

            BaseFilter converted = v.SettingsObj as BaseFilter;
            converted.Name = v.Name;
            converted.Type = ObsTypes.ObsTypeNameDictionary[v.Type];
            converted.Enabled = v.Enabled;
            return converted;
        }

        public static BaseFilter FromObsReplyFilter(TypeDefs.ObsReplyFilter v)
        {
            return v;
        }
    }
}
