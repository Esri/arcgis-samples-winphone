using Microsoft.Phone.Controls;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System;
using Microsoft.Phone.Shell;

namespace ArcGISWindowsPhoneSDK
{
    public partial class UsingPointDataSource : PhoneApplicationPage
    {
        public UsingPointDataSource()
        {
            InitializeComponent();
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            ApplicationBarIconButton button = sender as ApplicationBarIconButton;
            PDSMainViewModel pds_mvm = PDSPage.Resources["MyViewModel"] as PDSMainViewModel;
            if (button.Text.Contains("Add"))
            {
                if (pds_mvm.Instance.AddRandom.CanExecute(null))
                    pds_mvm.Instance.AddRandom.Execute(null);
            }
            else if (button.Text.Contains("Remove"))
            {
                if (pds_mvm.Instance.RemoveFirst.CanExecute(null))
                    pds_mvm.Instance.RemoveFirst.Execute(null);
            }
            else if (button.Text.Contains("Randomize"))
            {
                if (pds_mvm.Instance.Randomize.CanExecute(null))
                    pds_mvm.Instance.Randomize.Execute(null);
            }
        }
    }

    public class MainModel : INotifyPropertyChanged
    {
        private ObservableCollection<DataPoint> _PointsOfInterest;
        public ObservableCollection<DataPoint> PointsOfInterest
        {
            get { return _PointsOfInterest; }
            set
            {
                if (_PointsOfInterest != value)
                {
                    _PointsOfInterest = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("PointsOfInterest"));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class DataPoint : INotifyPropertyChanged
    {
        private double _X;
        private double _Y;
        private bool _IsSelected;

        public double X
        {
            get { return _X; }
            set
            {
                if (_X != value)
                {
                    _X = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("X"));
                }
            }
        }

        public double Y
        {
            get { return _Y; }
            set
            {
                if (_Y != value)
                {
                    _Y = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Y"));
                }
            }
        }

        public string _Name;

        public string Name
        {
            get { return _Name; }
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Name"));
                }
            }
        }

        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (_IsSelected != value)
                {
                    _IsSelected = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("IsSelected"));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class PDSMainViewModel
    {
        private const double width = 360;
        private const double height = 180;
        private static Random r = new Random();

        public MainModel Data { get; set; }
        private static PDSMainViewModel instance;
        public PDSMainViewModel Instance
        {
            get
            {
                if (instance == null)
                    instance = new PDSMainViewModel();
                return instance;
            }
        }

        public ICommand Randomize { get; private set; }
        public ICommand AddRandom { get; private set; }
        public ICommand RemoveFirst { get; private set; }

        public PDSMainViewModel()
        {
            Data = new MainModel()
            {
                PointsOfInterest = new
                    System.Collections.ObjectModel.ObservableCollection<DataPoint>()
            };
            GenerateDataSet();

            AddRandom = new DelegateCommand((a) => AddRandomEntry(), (b) => { return true; });
            RemoveFirst = new DelegateCommand((a) =>
            {
                if (Data.PointsOfInterest.Count > 0) Data.PointsOfInterest.RemoveAt(0);
            }, (b) => { return true; });
            Randomize = new DelegateCommand((a) => RandomizeEntries(), (b) => { return true; });
        }

        #region Generate random data
        private void GenerateDataSet()
        {
            for (int i = 0; i < 10; i++)
            {
                AddRandomEntry();
            }
        }

        private void AddRandomEntry()
        {
            Data.PointsOfInterest.Add(CreateRandomEntry(Data.PointsOfInterest.Count));
        }

        private DataPoint CreateRandomEntry(int i)
        {
            return new DataPoint()
            {
                X = r.NextDouble() * width - width * .5,
                Y = r.NextDouble() * height - height * .5,
                Name = string.Format("Item #{0}", i)
            };
        }

        private void RandomizeEntries()
        {
            for (int i = 0; i < Data.PointsOfInterest.Count; i++)
            {
                var pnt = Data.PointsOfInterest[r.Next(Data.PointsOfInterest.Count)];
                pnt.X = r.NextDouble() * width - width * .5;
                pnt.Y = r.NextDouble() * height - height * .5;
            }
        }
        #endregion
    }

    public class DelegateCommand : ICommand
    {
        Func<object, bool> canExecute;
        Action<object> executeAction;
        bool canExecuteCache;

        public DelegateCommand(Action<object> executeAction, Func<object, bool> canExecute)
        {
            this.executeAction = executeAction;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            bool temp = canExecute(parameter);
            if (canExecuteCache != temp)
            {
                canExecuteCache = temp;
                if (CanExecuteChanged != null)
                {
                    CanExecuteChanged(this, new EventArgs());
                }
            }

            return canExecuteCache;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            executeAction(parameter);
        }
    }
}