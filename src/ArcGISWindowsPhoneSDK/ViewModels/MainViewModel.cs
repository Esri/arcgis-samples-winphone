using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ArcGISWindowsPhoneSDK
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            Items = new ObservableCollection<ItemViewModel>(); 
        }

        public ObservableCollection<ItemViewModel> Items { get; private set; }

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