using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CommonUI.Helper
{
    public static class BoolDynamicBrush
    {
        public static readonly DependencyProperty FlagProperty = DependencyProperty.RegisterAttached(
            "Flag", typeof(bool), typeof(BoolDynamicBrush), new PropertyMetadata(false, OnChanged));

        public static readonly DependencyProperty FgTrueKeyProperty = DependencyProperty.RegisterAttached("FgTrueKey",
            typeof(string), typeof(BoolDynamicBrush), new PropertyMetadata(string.Empty, OnChanged));
        public static readonly DependencyProperty FgFalseKeyProperty = DependencyProperty.RegisterAttached("FgFalseKey",
            typeof(string), typeof(BoolDynamicBrush), new PropertyMetadata(string.Empty, OnChanged));
        public static readonly DependencyProperty BgTrueKeyProperty = DependencyProperty.RegisterAttached("BgTrueKey",
            typeof(string), typeof(BoolDynamicBrush), new PropertyMetadata(string.Empty, OnChanged));
        public static readonly DependencyProperty BgFalseKeyProperty = DependencyProperty.RegisterAttached("BgFalseKey",
            typeof(string), typeof(BoolDynamicBrush), new PropertyMetadata(string.Empty, OnChanged));

        public static bool GetFlag(DependencyObject obj) => (bool)obj.GetValue(FlagProperty);
        public static string? GetFgTrueKey(DependencyObject obj) => (string?)obj.GetValue(FgTrueKeyProperty);
        public static string? GetFgFalseKey(DependencyObject obj) => (string?)obj.GetValue(FgFalseKeyProperty);
        public static string? GetBgTrueKey(DependencyObject obj) => (string?)obj.GetValue(BgTrueKeyProperty);
        public static string? GetBgFalseKey(DependencyObject obj) => (string?)obj.GetValue(BgFalseKeyProperty);

        public static void SetFlag(DependencyObject obj, bool value) => obj.SetValue(FlagProperty, value);
        public static void SetFgTrueKey(DependencyObject obj, string? value) => obj.SetValue(FgTrueKeyProperty, value);
        public static void SetFgFalseKey(DependencyObject obj, string? value) => obj.SetValue(FgFalseKeyProperty, value);
        public static void SetBgTrueKey(DependencyObject obj, string? value) => obj.SetValue(BgTrueKeyProperty, value);
        public static void SetBgFalseKey(DependencyObject obj, string? value) => obj.SetValue(BgFalseKeyProperty, value);

        static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not FrameworkElement fe)
                return;
            var on = GetFlag(d);
            Apply(fe, "Foreground", on ? GetFgTrueKey(d) : GetFgFalseKey(d));
            Apply(fe, "Background", on ? GetBgTrueKey(d) : GetBgFalseKey(d));
        }

        static void Apply(FrameworkElement fe, string propertyName,string? key)
        {
            if(string.IsNullOrEmpty(key))
                return;
            var dp = DependencyPropertyDescriptor.FromName(propertyName, fe.GetType(), fe.GetType())?.DependencyProperty;
            if (dp == null)
                return;
           fe.SetResourceReference(dp, key);
        }
    }
}
