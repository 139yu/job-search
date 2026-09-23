using System.Collections.ObjectModel;
using Commons;
using Commons.Base;

namespace CommonUI.ViewModels;

public class MessageDialogViewModel: BaseDialogAware
{
    public string Message { get; private set; } = string.Empty;
    public ObservableCollection<DialogButtonModel> Buttons { get; } = new();
    public DelegateCommand<DialogButtonModel> ButtonCommand { get; }

    public MessageDialogViewModel()
    {
        ButtonCommand = new DelegateCommand<DialogButtonModel>(OnButton);
    }
    private void OnButton(DialogButtonModel button)
    {
        RequestClose.Invoke(new DialogResult(button.Result));
    }

    public override void OnDialogOpened(IDialogParameters parameters)
    {
        var request = parameters.GetValue<DialogRequest>(DialogParameterKeys.Request);
        Title = request.Title;
        Message = request.Message;
        Buttons.Clear();
        foreach (var button in request.Buttons)
            Buttons.Add(button);
    }
}