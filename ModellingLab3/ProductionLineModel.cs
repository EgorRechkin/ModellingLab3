
using System.Windows;

namespace ModellingLab3;

public class Detail
{
    private string _id;
    public string Id
    {
        get => _id;
        set => _id = value;
    }
    private double _time;
    public double Time
    {
        get => _time;
        set => _time = value;
    }
    private double _timeWaited2;
    public double TimeWaited2
    {
        get => _timeWaited2;
        set => _timeWaited2 = value;
    }
    private double _timeWaited1;
    public double TimeWaited1
    {
        get => _timeWaited1;
        set => _timeWaited1 = value;
    }
    public Detail(string id, double time)
    {
        _id = id;
        _time = time;
    }

    public void IncreaseTime(double newTime)
    {
        _time += newTime;
    }
}

public class Timer
{
    private static double _time = 0.1;
    public static double Time
    {
        get => _time;
        set => _time = value;
    }

    private double _modellingTime;

    public delegate void endModel();
    public event endModel EndWork;
    public async Task timer()
    {
        while (_modellingTime > _time)
        {
            await Task.Delay(100);
            _time += 0.1;
        }
        EndWork();
    }
    public Timer(double modellingTime) => _modellingTime = modellingTime;
}

public class ProductionLineModel
{
    private readonly int _numberOfWorkersInFirstWorkPlace;

    private double _modellingTime;
    
    private readonly double _firstWorkerTime;
    private readonly double _secondWorkerTime;
    private double _detailLastTime;
    private readonly double _baseTime;
    
    private double t_downtime1 = 0;
    private double t_downtime2 = 0;

    private double t_arrival1 = 0;
    private double t_arrival2 = 0;
    private double t_leaving1 = 0;
    private double t_leaving2 = 0;
    
    private readonly List<Detail> _detailsInTheMainBuffer;
    private readonly List<Detail> _detailsForFirstWorker;
    private readonly List<Detail> _detailsForSecondWorker;
    private readonly List<Detail> _activeDetailForFirstWorker;
    private readonly List<Detail> _preparedDetails;

    private bool _isFirstWorkerHasTask;
    private Detail _activeDetailForSecondWorker;
    private bool _isSecondWorkerHasTask;
    
    public delegate void ShowDetail(Detail detail);
    public event ShowDetail DetailHasComeToFirstWorker;
    public event ShowDetail DetailHasComeToSecondWorker;
    public event ShowDetail DetailHasComeToFirstBuffer;
    public event ShowDetail DetailHasBeenPrepared;
    public event ShowDetail DetailHasBeenTakenFromFirstBuffer;
    
    private async Task SentDetailToFirstWorker(Detail detail)
    {
        if (_activeDetailForFirstWorker.Count < _numberOfWorkersInFirstWorkPlace)
        {
            _activeDetailForFirstWorker.Add(detail);
            int i = _activeDetailForFirstWorker.IndexOf(detail);
            await FirstWorkerJob(_detailsForFirstWorker.First(), i);
        }
    }
    
    private void AddDetailForFirstWorkerBuffer(Detail detail)
    {
        if (_detailsForSecondWorker.Count <= 2) _isFirstWorkerHasTask = true;
        if (_detailsForFirstWorker.Count <= 3*_numberOfWorkersInFirstWorkPlace && _isFirstWorkerHasTask)
        {
            _detailsForFirstWorker.Add(detail);
            DetailHasComeToFirstWorker(detail);
            SentDetailToFirstWorker(detail);
        }
        else
        {
            _detailsInTheMainBuffer.Add(detail);
            DetailHasComeToFirstBuffer(detail);
        }
    }

    private void TakeDetailFromBufferToFirstWorker()
    {
        Detail firstDetail = _detailsInTheMainBuffer.First();
        _detailsInTheMainBuffer.Remove(firstDetail);
        _detailsForFirstWorker.Add(firstDetail);
        DetailHasComeToFirstWorker(firstDetail);
        DetailHasBeenTakenFromFirstBuffer(firstDetail);
    }
    private async Task FirstWorkerJob(Detail detail, int i)
    {
        detail.TimeWaited1 = Timer.Time - detail.Time;
        //if(detail.Time == 0.8)MessageBox.Show(detail.TimeWaited1.ToString());
        detail.Time += detail.TimeWaited1;
        t_arrival1 += Timer.Time;
        t_downtime1 = t_arrival1 - t_leaving1;
        
        
        await Task.Delay((int)(_firstWorkerTime*_baseTime));
        if (_detailsForSecondWorker.Count < 2)
        {
            SendDetailToSecondWorker(detail);
            _detailsForFirstWorker.Remove(detail);
            _activeDetailForFirstWorker.RemoveAt(i);
            t_leaving1 += Timer.Time;
        }
        if (_detailsInTheMainBuffer.Count > 0)
        {
            TakeDetailFromBufferToFirstWorker();
        }
        if (_detailsForFirstWorker.Any() || _detailsForSecondWorker.Count <= 2)
        {
            await SentDetailToFirstWorker(detail);
        }
        else
        {
            _isFirstWorkerHasTask = false;
        }
    }
    private void AddDetailForSecondWorker(Detail detail) => _detailsForSecondWorker.Add(detail);

    private async Task SendDetailToSecondWorker(Detail detail)
    {
        detail.IncreaseTime(_baseTime/1000*_firstWorkerTime);
        //MessageBox.Show(detail.Time.ToString());
        if (_detailsForSecondWorker.Count() <= 2)
        {
            AddDetailForSecondWorker(detail);
            DetailHasComeToSecondWorker(detail);
            if (!_isSecondWorkerHasTask)
            {
                _activeDetailForSecondWorker = _detailsForSecondWorker.First();
                _isSecondWorkerHasTask = true;
                await SecondWorkerJob(_activeDetailForSecondWorker);
            }
        }
    }

    private async Task SecondWorkerJob(Detail detail)
    {
        _detailsForSecondWorker.Remove(detail);
        detail.TimeWaited2 = Timer.Time - detail.Time+0.05;
        t_arrival2 += Timer.Time+0.05;
        t_downtime2 = t_arrival2 - t_leaving2;
        await Task.Delay((int)(_secondWorkerTime*_baseTime));
        if (_detailsInTheMainBuffer.Count > 0 && !_isFirstWorkerHasTask) _isFirstWorkerHasTask = true;
        detail.IncreaseTime(_baseTime/1000*_secondWorkerTime);
        _preparedDetails.Add(detail);
        DetailHasBeenPrepared(detail);
        t_leaving2 += Timer.Time;
        if (_detailsForSecondWorker.Any())
        {
            await SecondWorkerJob(_detailsForSecondWorker.First());
            _isFirstWorkerHasTask = true;
        }
        else _isSecondWorkerHasTask = false;
    }

    public async Task BeginWork()
    {
        double[] times = { 0.4, 0.8, 1.2, 1.6, 2.0, 2.4, 2.8, 3.2, 3.6, 4.0 };
        for(int i = 0 ; i < 10; i ++)//while (true)
        {
            double time = 0.4;//CalculateInterval(times[i]);
            Detail detail = new Detail("Detail", _detailLastTime+time);
            await Task.Delay((int)(time*_baseTime));
            AddDetailForFirstWorkerBuffer(detail);
            _detailLastTime += time;
        }
    }

    public void EndWork()
    {
        double k = (t_downtime1/(_modellingTime-t_downtime1))*(t_downtime2)/(_modellingTime-t_downtime2);
        double twait1 = 0, twait2 = 0;
        foreach (var detail in _preparedDetails)
        {
            twait1 += detail.TimeWaited1;
            twait2 += detail.TimeWaited2;
        }
        
        double tAvWork = ((_firstWorkerTime + _secondWorkerTime) * _preparedDetails.Count + twait1 + twait2) /
                           _preparedDetails.Count;
        double unpreparedDetails = _detailsInTheMainBuffer.Count;
        double alldetails = _detailsInTheMainBuffer.Count + _detailsForSecondWorker.Count + _detailsForFirstWorker.Count + _preparedDetails.Count;
        double p = unpreparedDetails / alldetails;
        MessageBox.Show("k= " + k + "t_av = " + tAvWork + "p = " + p);
    }
    public ProductionLineModel(TimeChoice userChoice, int numberOfWorkplaces, double firstWorkerSpeed, double secondWorkerSpeed, double modellingTime)
    {
        _detailsInTheMainBuffer = new List<Detail>();
        _detailsForFirstWorker = new List<Detail>();
        _detailsForSecondWorker = new List<Detail>();
        _preparedDetails = new List<Detail>();
        _firstWorkerTime = firstWorkerSpeed;
        _secondWorkerTime = secondWorkerSpeed;
        _detailLastTime = 0;
        _numberOfWorkersInFirstWorkPlace = numberOfWorkplaces;
        _activeDetailForFirstWorker = new List<Detail>();
        _isFirstWorkerHasTask = true;
        _modellingTime = modellingTime;
        switch (userChoice)
        {
            case TimeChoice.Second: _baseTime = 1000;
                return;
            case TimeChoice.Minute: _baseTime = 60000;
                return;
            case TimeChoice.Hour: _baseTime = 3600000;
                return;
        }
    }
    
    private double CalculateInterval(double t)
    {
        Random random =  new Random();
        double randomNullOne = random.NextDouble();
        return t; //-0.4*Math.Log2(1-randomNullOne);
    }
}