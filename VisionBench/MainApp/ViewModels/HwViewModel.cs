using Commons.Base;
using Commons.Enums;
using Commons.Logging;
using CommonUI.Helper;
using CommonUI.Service;
using Vision.Base;
using Vision.Enums;
using Vision.Events;
using Vision.ImageProvider;

namespace MainApp.ViewModels;

public class HwViewModel : RegionBaseViewModel
{
    private static readonly NLog.Logger _logger = Log.For<HwViewModel>(LogModule.App);
    private ICameraStationService _stationService;
    private IFilePickerService _filePickerService;
    private IImageProvider _imageProvider;
    private ImageSourceEnum _activeImageSource = ImageSourceEnum.Camera;

    private readonly FileImageProvider _fileImageProvider = new();
    private IImageProvider _activeImageProvider;

    /// <para>
    /// <b>这里刻意不做"帧转发"：</b>视图直接订阅本属性所指的提供器。
    /// 转发一层会把"一帧一个所有者"的规则打穿——提供器的 FrameDistributor
    /// 会为每个订阅者分发独立实例，而中间转发出去的是同一个实例；
    /// 以后算法层再订阅一次（这是最自然的做法），就变成两个消费者共享同一张图，
    /// 先释放的一方让另一方访问已释放的非托管内存，表现为随机崩溃。
    /// </para>
    /// </summary>
    public IImageProvider ActiveImageProvider => _activeImageProvider;

    public event EventHandler<string>? SaveRequested;
    public DelegateCommand OpenImageCommand { get; set; }
    public DelegateCommand SaveImageCommand { get; set; }

    public HwViewModel(ICameraStationService stationService, IFilePickerService filePickerService)
    {
        _stationService = stationService;
        _filePickerService = filePickerService;

        _imageProvider = stationService.GetImageProvider(StationEnum.MainCamera);
        _activeImageProvider = _imageProvider;
        _stationService.StationStateChanged += OnStationStateChanged;
        OpenImageCommand = new DelegateCommand(DoOpenImage);
        SaveImageCommand = new DelegateCommand(DoSaveImage);
    }

    private bool _isGrayDisplay;

    public bool IsGrayDisplay
    {
        get => _isGrayDisplay;
        set => SetProperty(ref _isGrayDisplay, value);
    }

    public string ImageSourceText => _activeImageSource == ImageSourceEnum.File ? "文件" : "相机";

    private void SwitchSource(IImageProvider target,ImageSourceEnum imageSource)
    {
        if (ReferenceEquals(target, _activeImageProvider))
            return;
        _activeImageProvider = target;
        _activeImageSource = imageSource;
        RaisePropertyChanged(nameof(ActiveImageProvider));
        RaisePropertyChanged(nameof(ImageSourceText));
    }
    
    private void OnStationStateChanged(object? sender, StationStateChangedEventArgs e)
    {
        if (e.StationName != StationEnum.MainCamera)
            return;
        if (e.NewState == StationStateEnum.Grabbing)
            SwitchSource(_imageProvider, ImageSourceEnum.Camera);
    }
    private void DoSaveImage()
    {
        try
        {
            var path = _filePickerService.PickImageSavePath();
            if (path is null)
                return;
            SaveRequested?.Invoke(this,path);
        }
        catch (Exception e)
        {
            _logger.Error(e,"保存图片失败");
            GrowlHelper.Error(e.Message);
        }
    }

    private void DoOpenImage()
    {
        var path = _filePickerService.PickImageFile();
        if (path is null)
            return;
        try
        {
            // 先停止采集再切源，不停止相机会继续采集白消耗资源
            // 切回相机源也是通过相机采集状态变更，如果不先采集，无法接收到OnStationStateChanged事件
            _imageProvider.Stop();
        }
        catch (Exception e)
        {
            _logger.Error(e,"打开图片前停止采集失败");
        }
        SwitchSource(_fileImageProvider,ImageSourceEnum.File);
        _fileImageProvider.Open(path);
    }

}