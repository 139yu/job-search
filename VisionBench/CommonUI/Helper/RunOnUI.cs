using System.Windows;
using System.Windows.Threading;

namespace CommonUI.Helper;

public class RunOnUI
{
    public static void Run(Action action)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher == null || dispatcher.CheckAccess())
        {
            action();
            return;
        }

        dispatcher.Invoke(action, DispatcherPriority.Normal);
    }
}