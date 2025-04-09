using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MemoryGame.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isVisible = (bool)value;
            bool invertVisibility = parameter != null && bool.Parse((string)parameter);

            if (invertVisibility)
                isVisible = !isVisible;

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visibility = (Visibility)value;
            bool invertVisibility = parameter != null && bool.Parse((string)parameter);
            bool isVisible = visibility == Visibility.Visible;

            if (invertVisibility)
                isVisible = !isVisible;

            return isVisible;
        }
    }
}