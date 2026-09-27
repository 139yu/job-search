using Commons.Base;

namespace CommonUI.ViewModels;

public class WaitingDialogViewModel : BindableBase
{
    public string Title { get; set; }
    private string _message;

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    private string _detail;

    public string Detail
    {
        get => _detail;
        set
        {
            SetProperty(ref _detail, value);
        }
    }
    private double? _percent;

    public double? Percent
    {
        get => _percent;
        set
        {
            SetProperty(ref _percent, value);
            RaisePropertyChanged(nameof(IsProgress));
            RaisePropertyChanged(nameof(IsIndeterminate));
        }
    }

    /// <summary>
    /// 进度不可知时显示转圈动画，否则显示环形进度条。
    /// </summary>
    public bool IsIndeterminate
    {
        get => Percent is null;
    }
    public bool IsProgress
    {
        get => Percent is not null;
    }
    public bool Cancelable { get; }
    public bool IsCancelling { get; set; }
    public string CancelText 
    {  
        get => IsCancelling ? "正在取消..." : "取消"; 
    }
    public DelegateCommand CancelCommand { get; }
    public event Action? CancelRequested;

    public WaitingDialogViewModel(BusyRequest request)
    {
        Title = request.Title;
        Message = request.Message;
        Cancelable = request.Cancellable;
        CancelCommand =
            new DelegateCommand(() => CancelRequested?.Invoke(), () => request.Cancellable && !IsCancelling);
    }

    public void UpdateProgress(BusyProgress progress)
    {
        if (progress.Message is not null)
            Message = progress.Message;
        if (progress.Detail is not null)
            Detail = progress.Detail;
        if (progress.Percent.HasValue && progress.Percent != Percent)
        {
            Percent = progress.Percent;
            RaisePropertyChanged(nameof(IsIndeterminate));
            RaisePropertyChanged(nameof(IsProgress));
        }
    }

    public void EnterCancelling()
    {
        IsCancelling = true;
        RaisePropertyChanged(nameof(CancelText));
        CancelCommand.RaiseCanExecuteChanged();
    }

}
