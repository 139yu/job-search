namespace Vision.Enums;

/// <summary>
/// 标定方法
/// </summary>
public enum CalibrationKindEnum
{
    /// <summary>未标定：像素坐标尚未转换到物理量。</summary>
    None = 0,

    /// <summary>仅比例标定：只求毫米/像素当量，不做畸变校正与世界坐标映射。</summary>
    ScaleOnly = 1,

    /// <summary>相机平面标定：求内参与畸变系数，并建立测量平面到世界坐标的映射（相机固定、工件位于同一平面）。</summary>
    CameraPlane = 2,

    /// <summary>手眼标定：求相机坐标系与运动机构（机械手/轴）之间的位姿关系。</summary>
    HandEye = 3
}

/// <summary>
/// 测量单位
/// </summary>
public enum MeasureUnitEnum
{
    /// <summary>像素。</summary>
    Pixel = 0,

    /// <summary>毫米。</summary>
    Millimeter = 1,

    /// <summary>微米。</summary>
    Micrometer = 2,

    /// <summary>平方像素。</summary>
    SquarePixel = 3,

    /// <summary>平方毫米。</summary>
    SquareMillimeter = 4,

    /// <summary>度。</summary>
    Degree = 5,

    /// <summary>百分比。</summary>
    Percent = 6
}

/// <summary>
/// 测量语义
/// </summary>
public enum MeasurementKindEnum
{
    /// <summary>两点间距离。</summary>
    Distance = 0,

    /// <summary>轮廓或线段长度。</summary>
    Length = 1,

    /// <summary>直径。</summary>
    Diameter = 2,

    /// <summary>半径。</summary>
    Radius = 3,

    /// <summary>角度。</summary>
    Angle = 4,

    /// <summary>面积。</summary>
    Area = 5,

    /// <summary>数量（如匹配、缺陷的个数）。</summary>
    Count = 6,

    /// <summary>得分（如匹配得分、质量评分）。</summary>
    Score = 7
}

/// <summary>
/// ROI形状
/// </summary>
public enum RoiShapeEnum
{
    /// <summary>轴对齐矩形：由行、列范围定义，不带旋转角（对应 Halcon gen_rectangle1）。</summary>
    Rectangle1 = 0,

    /// <summary>带方向矩形：由中心点、宽高和旋转角定义（对应 Halcon gen_rectangle2）。</summary>
    Rectangle2 = 1,

    /// <summary>圆：由圆心和半径定义。</summary>
    Circle = 2,

    /// <summary>椭圆：由中心、长短轴和旋转角定义。</summary>
    Ellipse = 3,

    /// <summary>多边形：由若干顶点按顺序围成。</summary>
    Polygon = 4
}

/// <summary>
/// 单项判定。Error 表示「没能算出」，不等于 Fail。
/// </summary>
public enum VerdictEnum
{
    /// <summary>合格。</summary>
    Pass = 0,

    /// <summary>不合格：正常算出结果，但超出判定阈值。</summary>
    Fail = 1,

    /// <summary>未能算出（输入无效、测量异常等），不等同于 Fail。</summary>
    Error = 2
}

/// <summary>参数类型。新增类型时同时补编辑器注册（§9）。</summary>
public enum ParamKindEnum
{
    /// <summary>整数参数。</summary>
    Int = 0,

    /// <summary>浮点数参数。</summary>
    Number = 1,

    /// <summary>布尔开关。</summary>
    Bool = 2,

    /// <summary>枚举选项。</summary>
    Enum = 3,

    /// <summary>文本参数。</summary>
    String = 4,

    /// <summary>点集参数（若干坐标点）。</summary>
    Points = 5,

    /// <summary>文件路径参数。</summary>
    File = 6,

    /// <summary>模型参数（模板、分类器等已加载的模型引用）。</summary>
    Model = 7
}

/// <summary>检测项语义（计数/得分/分类，或几何量的判定）。</summary>
public enum InspectionItemKindEnum
{
    /// <summary>计数：统计满足条件的数量。</summary>
    Count = 0,

    /// <summary>得分：输出匹配或质量得分。</summary>
    Score = 1,

    /// <summary>分类：输出类别判定。</summary>
    Classification = 2,

    /// <summary>长度：线状几何量。</summary>
    Length = 3,

    /// <summary>距离：点与点之间的几何量。</summary>
    Distance = 4,

    /// <summary>直径：圆形几何量。</summary>
    Diameter = 5,

    /// <summary>角度：夹角几何量。</summary>
    Angle = 6,

    /// <summary>面积：封闭区域几何量。</summary>
    Area = 7
}

/// <summary>一维测量的边缘极性。</summary>
public enum EdgePolarityEnum
{
    /// <summary>由暗到亮的边缘。</summary>
    Positive = 0,

    /// <summary>由亮到暗的边缘。</summary>
    Negative = 1,

    /// <summary>不限极性，两个方向都接受。</summary>
    All = 2
}

/// <summary>一维测量的边缘选取策略。</summary>
public enum EdgeSelectionEnum
{
    /// <summary>取第一个边缘。</summary>
    First = 0,

    /// <summary>取最后一个边缘。</summary>
    Last = 1,

    /// <summary>返回全部边缘。</summary>
    All = 2
}

public static class MeasureUnitExtensions
{
    /// <summary>
    /// 是否是面积单位，长度与面积不可互换
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    public static bool IsArea(this MeasureUnitEnum unit) =>
        unit is MeasureUnitEnum.SquarePixel or MeasureUnitEnum.SquareMillimeter;

    public static bool IsPhysical(this MeasureUnitEnum unit) =>
        unit is MeasureUnitEnum.Millimeter
            or MeasureUnitEnum.Micrometer
            or MeasureUnitEnum.SquareMillimeter;

    public static string Symbol(this MeasureUnitEnum unit)
    {
        return unit switch
        {
            MeasureUnitEnum.Pixel => "px",
            MeasureUnitEnum.Millimeter => "mm",
            MeasureUnitEnum.Micrometer => "μm",
            MeasureUnitEnum.SquarePixel => "px²",
            MeasureUnitEnum.SquareMillimeter => "mm²",
            MeasureUnitEnum.Degree => "°",
            MeasureUnitEnum.Percent => "%",
            _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, null)
        };
    }

    /// <summary>同一物理量在毫米/微米、像素/平方像素之间的换算系数。</summary>
    public static double FactorTo(this MeasureUnitEnum target, MeasureUnitEnum source)
    {
        if (target == source)
            return 1d;
        return (target, source)switch
        {
            (MeasureUnitEnum.Micrometer, MeasureUnitEnum.Millimeter) => 1000d,
            (MeasureUnitEnum.Millimeter, MeasureUnitEnum.Micrometer) => 0.0001d,
            _ => 1d
        };
    }
}