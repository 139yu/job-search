using Commons;
using Commons.Base;

namespace CommonUI.Service;

public class MessageDialogService : IMessageDialogService
{
    private IDialogService _dialogService;
    public MessageDialogService(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }
    public Task<DialogOutcome> ShowAsync(DialogRequest request)
    {
        var tcs = new TaskCompletionSource<DialogOutcome>();
        var parameters = new DialogParameters()
        {
            {
                DialogParameterKeys.Request,request
            }
        };
        _dialogService.ShowDialog(DialogNames.Message,parameters, result =>
        {
            var button = result?.Result ?? request.CloseRequest;
            tcs.TrySetResult(new DialogOutcome() { Result = button });
        });
        return tcs.Task;
    }

    public async Task<bool> ConfirmAsync(string message, string title = "提示")
    {
        var outcome = await ShowAsync(DialogRequest.Confirm(message, title));
        return outcome.Is(ButtonResult.OK);
    }

    public async Task ErrorAsync(string message)
    {
        var outcome = await  ShowAsync(DialogRequest.Error(message));
        outcome.Is(ButtonResult.OK);
    }
}