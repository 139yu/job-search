using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MainApp.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.WindowState = WindowState.Normal;
            this.MaxBtn.Visibility = Visibility.Visible;
            this.NormalBtn.Visibility = Visibility.Collapsed;
        }

        private void MinimizeClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void MaxClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized;
            this.MaxBtn.Visibility = Visibility.Collapsed;
            this.NormalBtn.Visibility = Visibility.Visible;
        }
        private void NormalClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Normal;
            this.MaxBtn.Visibility = Visibility.Visible;
            this.NormalBtn.Visibility = Visibility.Collapsed;
        }
    }
}