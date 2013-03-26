using System;
using System.ComponentModel;

namespace ArcGISWindowsPhoneSDK
{
    public class ItemViewModel : INotifyPropertyChanged
    {
        private string title;

        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                if (value != title)
                {
                    title = value;
                    NotifyPropertyChanged("Title");
                }
            }
        }
        
        private string _xaml;

        public string XAML
        {
            get
            {
                return _xaml;
            }
            set
            {
                _xaml = value;
                NotifyPropertyChanged("XAML");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}