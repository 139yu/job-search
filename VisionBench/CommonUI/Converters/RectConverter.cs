using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CommonUI.Converters;

public class RectConverter: IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        Rect rect = new Rect();
        if (values.Length == 2 && values[0] is double width && values[1] is double height)
        {
            rect = new  Rect(0,0,width,height);
        }
        return rect;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}