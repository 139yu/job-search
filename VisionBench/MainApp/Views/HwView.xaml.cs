using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Commons.Enums;
using Commons.Logging;
using CommonUI.Helper;
using CommonUI.Service;
using HalconDotNet;
using MainApp.Display;
using MainApp.Overlays;
using MainApp.ViewModels;
using Vision.Base;
using Vision.Events;

namespace MainApp.Views;

public partial class HwView : UserControl
{
    private static readonly NLog.Logger _logger = Log.For<HwView>(LogModule.App);
    private HwViewModel? _vm;
    private ImageCanvas? _canvas;
    private InfoOverlay _infoOverlay;
    private IMessageDialogService _messageDialogService;

    private IImageProvider? _subscribedSource;

    public HwView(IMessageDialogService messageDialogService)
    {
        InitializeComponent();
        _messageDialogService = messageDialogService;
        Loaded += OnLoaded;
        Unloaded += UnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // 保持比例
        HalconWindow.HKeepAspectRatio = true;
        // 缩放步长
        HalconWindow.HZoomFactor = 1.2;
        // 默认禁止拖拽 缩放
        HalconWindow.HMoveContent = false;
        HalconWindow.HZoomContent = HSmartWindowControlWPF.ZoomContent.Off;
        // Loaded 可能重复触发，只挂一次
        if (_canvas is not null)
            return;
        _vm = DataContext as HwViewModel;
        if (_vm is null)
        {
            _logger.Error("采集窗口数据绑定失败");
            _messageDialogService.ErrorAsync("采集窗口数据绑定失败");
            return;
        }

        _canvas = new ImageCanvas(HalconWindow, () => _vm.IsGrayDisplay);

        HalconWindow.HMouseWheel += OnHMouseWheel;
        HalconWindow.HMouseUp += OnHMouseUp;
        _infoOverlay = new InfoOverlay()
        {
            Visible = true
        };
        _canvas.AddOverlay(_infoOverlay);
        ResubscribeSource();
        _vm.SaveRequested += OnSaveRequested;
        _vm.PropertyChanged += OnViewModelPropertyChanged;
    }
    
    private void UnLoaded(object sender, RoutedEventArgs e)
    {
        if (_subscribedSource is not null)
        {
            _subscribedSource.FrameReady -= OnFrameReady;
            _subscribedSource = null;
        }

        if (_vm is not null)
        {
            _vm.SaveRequested -= OnSaveRequested;
            _vm.PropertyChanged -= OnViewModelPropertyChanged;
        }
        HalconWindow.HMouseWheel -= OnHMouseWheel;
        HalconWindow.HMouseUp -= OnHMouseUp;
        _canvas?.Dispose();
        _canvas = null;
        _infoOverlay = null;
    }

    private void ResubscribeSource()
    {
        var imageProvider = _vm.ActiveImageProvider;
        if (ReferenceEquals(imageProvider, _subscribedSource))
            return;
        if (_subscribedSource is not null)
            _subscribedSource.FrameReady -= OnFrameReady;
        _subscribedSource = imageProvider;
        if (_subscribedSource is not null)
            _subscribedSource.FrameReady += OnFrameReady;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(HwViewModel.IsGrayDisplay))
            _canvas?.RequestRedraw();
        else if (e.PropertyName == nameof(HwViewModel.ActiveImageProvider))
            ResubscribeSource();
    }

    private void OnSaveRequested(object? sender, string path)
    {
        try
        {
            if (_canvas is null || !_canvas.SaveCurrentImage(path))
                GrowlHelper.Warning("当前没有可保存的画面");
            else
                GrowlHelper.Success("保存成功");
        }
        catch (Exception exception)
        {
            _logger.Error(exception, "保存图片失败");
            GrowlHelper.Error($"保存图片失败：{exception.Message}");
        }
    }

    private void OnFrameReady(object? sender, FrameReadyEventArgs e)
    {
        var canvas = _canvas;
        // 视图已卸载或者还没加载完，无法处理图像帧
        if (canvas is null)
        {
            e.Image.Dispose();
            return;
        }

        if (_infoOverlay is not null && _vm is not null)
            _infoOverlay.SourceText = _vm.ImageSourceText;
        canvas.SetImage(e.Image);
    }

    private void OnResetViewClick(object sender, RoutedEventArgs e)
    {
        _canvas?.ResetView();
    }

    private void OnCanvasPreviewMouseWheel(object sender,MouseWheelEventArgs e)
    {
        bool ctrl = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
        HalconWindow.HZoomContent = ctrl ? HSmartWindowControlWPF.ZoomContent.WheelForwardZoomsIn : HSmartWindowControlWPF.ZoomContent.Off;
        if (!ctrl)
            e.Handled = true;
    }

    private void OnCanvasPreviewMouseLeftButtonDown(object sender,MouseButtonEventArgs e)
    {
        HalconWindow.HMoveContent = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
    }

    private void OnCanvasPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (_canvas is null || !_canvas.TryGetPixel(e.GetPosition(HalconWindow), out var row, out var col, out var gray))
        {
            HudX.Text = "—";
            HudY.Text = "—";
            HudGray.Text = "—";
            return;
        }
        HudX.Text = col.ToString();      // X = 列
        HudY.Text = row.ToString();      // Y = 行
        HudGray.Text = gray.ToString("F0");
    }

    private void OnHMouseWheel(object sender, HSmartWindowControlWPF.HMouseEventArgsWPF hMouseEventArgsWpf)
    {
        _canvas?.RequestRedrawDeferred();
    }
    private void OnHMouseUp(object sender, HSmartWindowControlWPF.HMouseEventArgsWPF e)
    {
        _canvas?.RequestRedrawDeferred();
    }
}