using MainApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainApp.ViewModels
{
    public class MainWindowViewModel: BindableBase
    {
        private ObservableCollection<MenuItem> menuItems;

        public ObservableCollection<MenuItem> MenuItems
        {
            get { return menuItems; }
            set { menuItems = value; }
        }

        public MainWindowViewModel()
        {
            InitView();
        }
        private void InitView()
        {
            MenuItems = new ObservableCollection<MenuItem>
            {
                new MenuItem { Title = "相机设置", NavigationPath = "CameraSetting" },
                new MenuItem { Title = "运动设置", NavigationPath = "CameraSetting" },
            };
        }
    }
}
