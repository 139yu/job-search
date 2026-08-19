using System.Windows;
using System.Windows.Input;

namespace CommonUI.Base;

public partial class BaseDialog : Window,IDialogWindow
{
    public BaseDialog()
    {
        InitializeComponent();
    }

    public IDialogResult Result { get; set; }

    object IDialogWindow.Content
    {
        get => DialogContentHost.Content;
        set => DialogContentHost.Content = value;
    }

    private void MinimizeClick(object sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }
    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }
    
    private void CloseDialogClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}