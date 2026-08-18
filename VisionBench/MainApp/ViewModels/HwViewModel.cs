using Commons.Base;

namespace MainApp.ViewModels;

public class HwViewModel: RegionBaseViewModel
{
    private readonly IRegionManager _regionManager;
    public HwViewModel(IRegionManager regionManager)
    {
        _regionManager = regionManager;
    }
    
    
}