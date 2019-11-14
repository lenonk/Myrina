using Amazon.EC2;
using Avalonia.Data.Converters;
using System;

namespace MyrinaUI.Converters {
    public class StringToBoolConverter : IValueConverter {
        public object Convert(object value, Type targetType,
            object parameter, System.Globalization.CultureInfo culture) {
            string s = "";
            PlatformValues pv;

            if (value == null)
                return false;

            if (value.GetType() == typeof(string)) {
                s = (value as string);
            } else if (value.GetType() == typeof(PlatformValues)) {
                pv = (value as PlatformValues);
                s = pv.ToString();
            } else {
                throw new InvalidOperationException();
            }

            //Debug.WriteLine($"Converting {s} to boolean: result == {s.Length == 0}");
            return s.Length != 0;

        }

        public object ConvertBack(object value, Type targetType,
            object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
