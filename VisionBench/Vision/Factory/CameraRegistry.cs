using Commons.Base;
using Vision.Base;
using Vision.Enums;
using Vision.Models;

namespace Vision.Factory;

/// <summary>
/// 相机品牌注册表。集中管理所有已注册的品牌 Provider，
/// 是上层获取"枚举器 / 设备实例"的唯一入口。
/// <para>
/// 用注册表取代原先按品牌分支的工厂：新增品牌只注册、不修改本类。
/// </para>
/// </summary>
public class CameraRegistry
{
    private static readonly Lazy<CameraRegistry> _lazy = new(() => new CameraRegistry(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static CameraRegistry Instance => _lazy.Value;
    private readonly Dictionary<CameraEnum, ICameraProvider> _providers = new();

    private CameraRegistry()
    {
    }

    public void Register(ICameraProvider provider)
    {
        if (!_providers.TryAdd(provider.Brand, provider))
            throw new BusinessException($"相机品牌[{provider.Brand}]重复注册");
    }

    public IReadOnlyList<ICameraProvider> Providers => _providers.Values.OrderBy(p => p.Brand).ToList();

    public ICameraEnumerator GetEnumerator(CameraEnum? brand) => Resolve(brand).CameraEnumerator();

    public ICameraDevice Create(CameraInfo cameraInfo)
    {
        if (cameraInfo is null)
            throw new BusinessException("相机信息为空，无法创建相机设备！");
        return Resolve(cameraInfo.CameraType).Create(cameraInfo);
    }

    private ICameraProvider Resolve(CameraEnum? brand)
    {
        if (brand is null)
            throw new BusinessException("未指定相机品牌");
        if (!_providers.TryGetValue(brand.Value, out var provider))
            throw new BusinessException($"相机品牌[{brand}]未注册");
        return provider;
    }
}