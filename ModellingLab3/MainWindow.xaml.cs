using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ModellingLab3;


public class MainWindowVm : INotifyPropertyChanged
{
    private ProductionLineModel _productionLineModel;
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private Timer timer;
    public MainWindowVm(ProductionLineModel model, double modellingTime)
    {
        _productionLineModel = model;
        model.DetailHasComeToSecondWorker += _addDetailToSecondWorker;
        model.DetailHasComeToFirstWorker += _addDetailToFirstWorker;
        model.DetailHasComeToFirstBuffer += _addDetailToFirstBuffer;
        model.DetailHasBeenPrepared += _addDetailToPreparedList;
        model.DetailHasBeenTakenFromFirstBuffer += _takeDetailToFirstBuffer;
        timer = new Timer(modellingTime);
        timer.EndWork += _endWorkOfModel;
    }
    
    private ObservableCollection<Detail> _detailsForFirstWorker = new();
    public ObservableCollection<Detail> ListOfDetailsForFirstWorker => _detailsForFirstWorker;
    
    private ObservableCollection<Detail> _detailsForSecondWorker = new();
    public ObservableCollection<Detail> ListOfDetailsForSecondWorker => _detailsForSecondWorker;

    private ObservableCollection<Detail> _listOfDetailsInFirstBuffer = new();
    public ObservableCollection<Detail> ListOfDetailsInFirstBuffer => _listOfDetailsInFirstBuffer;
    
    private ObservableCollection<Detail> _listOfPreparedDetails = new();
    public ObservableCollection<Detail> ListOfPreparedDetails => _listOfPreparedDetails;
    
    private string _newDetail;
    public String NewDetail
    {
        get => _newDetail;
        set => _newDetail = value /*OnPropertyChanged("");*/;
    }

    //private int _completedDetails;
    public int CompletedDetails;
    //{
    //    get => _completedDetails;
    //    set => _completedDetails = value;
    //}
    public ICommand AddDetailToFirstWorker
    {
        get
        {
            return new CommandAsync(async () =>
            {
                _productionLineModel.BeginWork();
                await timer.timer();
                //await _productionLineModel.BeginWork();
                
            }); 
        }
    }
    private void _addDetailToFirstWorker(Detail detail)
    {
        _detailsForFirstWorker.Add(detail);
    }

    private void _takeItemFromFirstWorker(Detail detail)
    {
        _detailsForFirstWorker.Remove(detail);
    }
    private void _takeItemFromSecondWorker(Detail detail)
    {
        _detailsForSecondWorker.Remove(detail);
    }
    private void _addDetailToSecondWorker(Detail detail)
    {
        _detailsForSecondWorker.Add(detail);
        _takeItemFromFirstWorker(detail);
    }
    private void _addDetailToFirstBuffer(Detail detail)
    {
        _listOfDetailsInFirstBuffer.Add(detail);
    }
    private void _takeDetailToFirstBuffer(Detail detail)
    {
        _listOfDetailsInFirstBuffer.Remove(detail);
    }
    private void _addDetailToPreparedList(Detail detail)
    {
        _listOfPreparedDetails.Add(detail);
        _takeItemFromSecondWorker(detail);
    }

    private void _endWorkOfModel()
    {
        _productionLineModel.EndWork();
    }
}

public class CommandAsync : ICommand
{
    public event EventHandler? CanExecuteChanged = (_, _) => { };
    private readonly Func<Task> _action;
    public CommandAsync(Func<Task> action)
    {
        _action = action;
    }
    public async Task ExecuteAsync()
    {
        await _action();
    }
    public bool CanExecute(object? obj)
    {
        return true;
    }
    public async void Execute(object? obj)
    {
        await ExecuteAsync();
    }
}

public class RelayCommand : ICommand
{
    public event EventHandler? CanExecuteChanged = (_, _) => { };
    private readonly Action _action;
    public RelayCommand(Action action)
    {
        _action = action;
    }
    public bool CanExecute(object? obj)
    {
        return true;
    }
    public void Execute(object? obj)
    {
        _action();
    }
}

public partial class MainWindow : Window
{
    public MainWindow(TimeChoice userChoice, int numberOfWorkplaces, double firstWorkerSpeed, double secondWorkerSpeed, double modellingTime)
    {
        InitializeComponent();
        ProductionLineModel model = new(userChoice, numberOfWorkplaces, firstWorkerSpeed, secondWorkerSpeed, modellingTime);
        DataContext = new MainWindowVm(model, modellingTime);
        
    }
}