using System.Runtime.InteropServices;
using Commons.Base;
using HalconDotNet;
using Vision.Enums;
using Vision.Models;

namespace Vision.Halcon;

/// <summary>
/// <see cref="CameraFrame"/>（相机原始像素）→ Halcon <see cref="HImage"/> 的转换。
///
/// <para>
/// 本类是<b>像素格式的收敛点</b>：全仓库只有两处能看到 <see cref="ImageLayoutEnum"/> ——
/// 生产它的 <c>HikVisionCamera.TryBuildFrame</c>，和这里消费它的转换。
/// 其余任何代码拿到的都是 <see cref="HImage"/>，不需要知道像素格式。
/// </para>
/// </summary>
public static class CameraFrameConverter
{
    /// <summary>
    /// 把一帧原始像素转换成 Halcon 图像。
    /// <para><b>返回值所有权归调用方</b>，必须释放。</para>
    /// </summary>
    /// <exception cref="BusinessException">尺寸非法、没有像素数据、内存不可固定，或布局不支持。</exception>
    public static HImage ToHImage(CameraFrame frame)
    {
        if (frame is null)
            throw new BusinessException("图像帧未空");
        if (frame.Width < 0 || frame.Height < 0)
            throw new BusinessException("非法图像尺寸");
        if (frame.ImageData.IsEmpty)
            throw new BusinessException("图像数据未空");
        int bytesPerPixel = frame.ImageLayout switch
        {
            ImageLayoutEnum.Gray8 => 1,
            ImageLayoutEnum.Gray16 => 2,
            ImageLayoutEnum.Bgr8 => 3,
            _ => throw new BusinessException("非法图像帧")
        };
        int required = frame.Width * frame.Height * bytesPerPixel;
        if (frame.ImageData.Length < required)
            throw new BusinessException(
                $"图像帧数据长度不足：{frame.ImageLayout} {frame.Width}×{frame.Height}，" +
                $" 需要 {required} 字节，实际 {frame.ImageData.Length} 字节");
        if (!MemoryMarshal.TryGetArray(frame.ImageData, out ArraySegment<byte> segment)
            || segment.Array is null)
            throw new BusinessException("图像帧的数据不是数组支持的连续内存，无法固定地址");
        // .NET 的 GC 是移动式的：整理堆时它把对象换个地址，然后把所有托管引用改成新地址。
        // 托管代码察觉不到 —— 但已经交给非托管代码的裸指针不会被更新
        // GCHandle.Alloc(array, GCHandleType.Pinned) 就是告诉 GC： 这个对象在我释放它之前不许搬动。
        var handle = GCHandle.Alloc(segment.Array, GCHandleType.Pinned);
        try
        {
            // handle.AddrOfPinnedObject：segment.Array起点
            // 偏移segment.Offset后才是图像数据真正起点
            var pointer = IntPtr.Add(handle.AddrOfPinnedObject(), segment.Offset);
            return frame.ImageLayout switch
            {
                ImageLayoutEnum.Gray8 => GenImage1("byte", frame.Width, frame.Height, pointer),
                ImageLayoutEnum.Gray16 => GenImage1("uint16", frame.Width, frame.Height, pointer),
                ImageLayoutEnum.Bgr8 => BuildBgr8(frame, pointer),
                _ => throw new BusinessException($"不支持的像素布局：{frame.ImageLayout}")
            };
        }
        finally
        {
            handle.Free();
        }
    }

    /// <summary>
    /// 交错 BGR → 三平面彩色图。
    /// <para>
    /// 源数据按 B,G,R 依次交错排列，而 Halcon 的 <c>GenImage3</c> 要求三个独立平面、
    /// 且参数顺序是 (Red, Green, Blue)。<b>这里的通道映射写反了就是红蓝互换</b>，
    /// 是这类代码最经典的坑，改动时务必对照注释。
    /// </para>
    /// </summary>
    private static HImage BuildBgr8(CameraFrame frame, IntPtr pointer)
    {
        int pixelCount = frame.Width * frame.Height;
        var interleaved = new byte[pixelCount * 3];
        Marshal.Copy(pointer, interleaved, 0, interleaved.Length);

        var red = new byte[pixelCount];
        var green = new byte[pixelCount];
        var blue = new byte[pixelCount];
        for (int i = 0; i < pixelCount; i++)
        {
            int src = i * 3;
            red[i] = interleaved[src];
            green[i] = interleaved[src + 1];
            blue[i] = interleaved[src + 2];
        }

        return GenImage3(red, green, blue, frame.Width, frame.Height);
    }

    /// <summary>单通道图像。Halcon 会立即拷贝像素，因此 pin 只在本次调用期间需要。</summary>
    private static HImage GenImage1(string type, int width, int height, IntPtr pointer)
    {
        var image = new HImage();
        image.GenImage1(type, width, height, pointer);
        return image;
    }

    private static HImage GenImage3(byte[] red, byte[] green, byte[] blue, int width, int height)
    {
        var hRed = GCHandle.Alloc(red, GCHandleType.Pinned);
        var hGreen = GCHandle.Alloc(green, GCHandleType.Pinned);
        var hBlue = GCHandle.Alloc(blue, GCHandleType.Pinned);
        try
        {
            var image = new HImage();
            image.GenImage3("byte", width, height,
                hRed.AddrOfPinnedObject(), hGreen.AddrOfPinnedObject(), hBlue.AddrOfPinnedObject());
            return image;
        }
        finally
        {
            hRed.Free();
            hGreen.Free();
            hBlue.Free();
        }
    }
}