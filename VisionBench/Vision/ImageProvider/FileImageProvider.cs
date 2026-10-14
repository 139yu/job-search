using Commons.Base;
using Commons.Enums;
using Commons.Logging;
using HalconDotNet;
using Vision.Base;
using Vision.Enums;
using Vision.Events;

namespace Vision.ImageProvider;

public sealed class FileImageProvider: IImageProvider
{
    private readonly NLog.Logger _logger = Log.For(LogModule.Camera, typeof(FileImageProvider));
    public event EventHandler<FrameReadyEventArgs>? FrameReady;
    public string? FilePath { get; private set; }
    public void Start()
    {
        if (string.IsNullOrWhiteSpace(FilePath))
            throw new BusinessException("请选择图片文件");
        Open(FilePath);
    }

    public void Stop()
    {
        
    }
    /// <summary>
    /// 推一次帧即结束，永远为False
    /// </summary>
    public bool IsRunning { get; } = false;

    private void Open(string filePath)
    {
        if (string.IsNullOrWhiteSpace(FilePath))
            throw new BusinessException("请选择图片文件");
        FilePath = filePath;
        if (FrameReady is null)
        {
            _logger.Warn("没有订阅者");
            return;
        }

        var image = new HImage();
        try
        {
            image.ReadImage(filePath);
        }
        catch (Exception e)
        {
            image.Dispose();
            throw;
        }

        FrameDistributor.Dispatch(FrameReady, this, image, ImageSourceEnum.File);
    }
}