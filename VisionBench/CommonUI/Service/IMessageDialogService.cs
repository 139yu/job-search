using Commons.Base;

namespace CommonUI.Service;

public interface IMessageDialogService
{
    Task<DialogOutcome> ShowAsync(DialogRequest request);
    Task<bool> ConfirmAsync(string message, string title = "提示");
}