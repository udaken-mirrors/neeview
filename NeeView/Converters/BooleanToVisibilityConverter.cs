using System.Windows;

namespace NeeView
{
    [System.Windows.Data.ValueConversion(typeof(bool), typeof(Visibility))]
    public sealed class BooleanToVisibilityConverter : BooleanConverter<Visibility>
    {
        public BooleanToVisibilityConverter() :
            base(Visibility.Visible, Visibility.Collapsed)
        { }
    }

    [System.Windows.Data.ValueConversion(typeof(bool), typeof(Visibility))]
    public sealed class BooleanToInverseVisibilityConverter : BooleanConverter<Visibility>
    {
        public BooleanToInverseVisibilityConverter() :
            base(Visibility.Collapsed, Visibility.Visible)
        { }
    }
}

