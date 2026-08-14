using System.Drawing;
using System.Windows;
using System.Windows.Controls;

namespace CommonUI
{
    public class IconButton: Button
    {
        static IconButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IconButton), new FrameworkPropertyMetadata(typeof(IconButton)));
        }
        public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register(
            nameof(IconSize), typeof(double), typeof(IconButton), new PropertyMetadata(20d));

        public double IconSize
        {
            get { return (double)GetValue(IconSizeProperty); }
            set { SetValue(IconSizeProperty, value); }
        }

        public static readonly DependencyProperty IconCodeProperty = DependencyProperty.Register(
            nameof(IconCode), typeof(string), typeof(IconButton), new PropertyMetadata(default(string)));

        public string IconCode
        {
            get { return (string)GetValue(IconCodeProperty); }
            set { SetValue(IconCodeProperty, value); }
        }

        public static readonly DependencyProperty IconBrushProperty = DependencyProperty.Register(
            nameof(IconBrush), typeof(Brush), typeof(IconButton), new PropertyMetadata(Brushes.Black));

        public Brush IconBrush
        {
            get { return (Brush)GetValue(IconBrushProperty); }
            set { SetValue(IconBrushProperty, value); }
        }
        
    }
}
