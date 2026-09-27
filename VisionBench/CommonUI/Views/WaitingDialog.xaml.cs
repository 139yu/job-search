using System.ComponentModel;
using System.Windows;

namespace CommonUI.Views;

public partial class WaitingDialog : Window
{
    private bool _canClose;
    public WaitingDialog()
    {
        InitializeComponent();
    }
    public void CloseByService()
    {
        _canClose = true;
        Close();
    }
    protected override void OnClosing(CancelEventArgs e)
    {
        if(!_canClose)
            e.Cancel = true;
        base.OnClosing(e);
    }
}