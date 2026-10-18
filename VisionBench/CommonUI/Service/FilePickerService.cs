using Microsoft.Win32;

namespace CommonUI.Service;

public class FilePickerService : IFilePickerService
{
    private const string ImageFilter = "图片文件|*.png,*.jpg,*.jpeg,*.tif,*.tiff|所有文件|*.*";
    public string? PickImageFile(string title = "请选择图片")
    {
        var dialog = new OpenFileDialog()
        {
            Title = title,
            Filter = ImageFilter,
            CheckFileExists = true,
            Multiselect = false
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? PickImageSavePath(string defaultFileName = "image.png", string title = "保存图片")
    {
        var dialog = new SaveFileDialog()
        {
            FileName = defaultFileName,
            Title = title,
            DefaultExt = ".png",
            AddExtension = true,
            Filter = ImageFilter
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}