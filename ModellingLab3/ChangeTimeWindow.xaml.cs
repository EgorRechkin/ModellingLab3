using System.ComponentModel;
using System.Windows;

namespace ModellingLab3;

public enum TimeChoice
{
    Second, Minute, Hour
}

public class ChangeTimeVM : INotifyPropertyChanged
{
    private TimeChoice _userChoice;
    private int _workerNumber;
    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private int _isSeconds;
    public int IsSeconds
    {
        get => _isSeconds;
        set
        {
            _isSeconds = value;
            _userChoice = TimeChoice.Second;
        }
    }
    
    private int _isMinutes;
    public int IsMinutes
    {
        get => _isMinutes;
        set
        {
            _isMinutes = value;
            _userChoice = TimeChoice.Minute;
        }
    }
    
    private int _isHours;
    public int IsHours
    {
        get => _isHours;
        set
        {
            _isHours = value;
            _userChoice = TimeChoice.Hour;
        }
    }
    
    private int _isOneWorker;
    public int IsOneWorker
    {
        get => _isOneWorker;
        set
        {
            _isOneWorker = value;
            _workerNumber = 1;
        }
    }
    
    private int _areTwoWorkers;
    public int AreTwoWorkers
    {
        get => _areTwoWorkers;
        set
        {
            _areTwoWorkers = value;
            _workerNumber = 2;
        }
    }
    
    private int _areThreeWorkers;
    public int AreThreeWorkers
    {
        get => _areThreeWorkers;
        set
        {
            _areThreeWorkers = value;
            _workerNumber = 3;
        }
    }

    private double _firstWorkerSpeed = 1.25;
    public double FirstWorkerSpeed
    {
        get => _firstWorkerSpeed;
        set => _firstWorkerSpeed = value;
    }
    
    private double _secondWorkerSpeed = 0.5;
    public double SecondWorkerSpeed
    {
        get => _secondWorkerSpeed;
        set => _secondWorkerSpeed = value;
    }

    private double _modellingTime = 4.4;
    public double ModellingTime
    {
        get => _modellingTime;
        set => _modellingTime = value;
    }
    
    public RelayCommand ChangeWindow
    {
        get
        {
            return new RelayCommand(() => _changeWindow());
        }
    }
    
    private void _changeWindow()
    {
        MainWindow window = new MainWindow(_userChoice, _workerNumber, _firstWorkerSpeed, _secondWorkerSpeed, _modellingTime);
        window.Show();
    }
}

public partial class ChangeTimeWindow : Window
{
    public ChangeTimeWindow()
    {
        InitializeComponent();
        DataContext = new ChangeTimeVM();
    }
}