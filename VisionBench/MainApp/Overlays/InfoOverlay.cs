using HalconDotNet;
using MainApp.Display;

namespace MainApp.Overlays;

public sealed class InfoOverlay : ImageOverlayBase
{
    public InfoOverlay() : base("图像信息")
    {
    }

    /// <summary>
    /// 来源说明（相机 / 文件），由视图层在来源切换时更新。
    /// </summary>
    public string SourceText { get; set; } = string.Empty;

    /// <summary>
    /// 文字颜色
    /// </summary>
    public string Color { get; set; } = "white";

    public override void Render(HWindow window, ImageCanvasContext context)
    {
        try
        {
            if (context.ImageWidth <= 0 || context.ImageHeight <= 0)
                return;
            window.GetWindowExtents(out int row, out int column, out int width, out int height);
            window.SetColor(Color);
            window.SetFont("Consolas-12");
            string str;
            if (!string.IsNullOrWhiteSpace(SourceText))
            {
                str = $"{SourceText}·{context.ImageWidth}×{context.ImageHeight}";
            }
            else
            {
                str = $"{context.ImageWidth}×{context.ImageHeight}";
            }
            using HTuple color = new HTuple(Color);
            using HTuple bottom = new HTuple("bottom");
            using HTuple left = new HTuple("left");
            using HTuple paramName = new HTuple(new[]{"box","box_color","shadow"});
            using HTuple paramValue = new HTuple(new[]{"true","#000000CC","false"});
         
            using HTuple message = new HTuple(str);
            window.DispText(message,
                "window",bottom,left,color,paramName,paramValue);
        }
        catch (Exception e)
        {
            
        }
    }
}