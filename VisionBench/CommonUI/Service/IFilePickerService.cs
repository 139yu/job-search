namespace CommonUI.Service;

public interface IFilePickerService
{
    string? PickImageFile(string title = "请选择图片");
    string? PickImageSavePath(string defaultFileName = "image.png", string title = "保存图片");
}