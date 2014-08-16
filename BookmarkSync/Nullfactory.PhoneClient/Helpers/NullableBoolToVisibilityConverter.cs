using System;
using System.Windows;
using System.Windows.Data;

namespace Nullfactory.PhoneClient.Helpers
{
    public class NullableBoolToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return Visibility.Collapsed;

            bool isVisible = false;
            bool.TryParse(value.ToString(), out isVisible);

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
