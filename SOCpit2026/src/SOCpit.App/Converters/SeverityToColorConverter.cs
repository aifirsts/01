// SOCpit - Security Operations Center
// Severity to color converter
// Based on MATRIX V3 with WPF

using SOCpit.Core;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SOCpit.App.Converters
{
    /// <summary>Converts threat severity to color.</summary>
    public sealed class SeverityToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ThreatSeverity severity)
            {
                return severity switch
                {
                    ThreatSeverity.Critical => Brushes.Red,
                    ThreatSeverity.High => Brushes.OrangeRed,
                    ThreatSeverity.Medium => Brushes.Orange,
                    ThreatSeverity.Low => Brushes.LightGreen,
                    _ => Brushes.Gray
                };
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>Converts threat severity to background color.</summary>
    public sealed class SeverityToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ThreatSeverity severity)
            {
                return severity switch
                {
                    ThreatSeverity.Critical => new SolidColorBrush(Color.FromRgb(220, 53, 69)),
                    ThreatSeverity.High => new SolidColorBrush(Color.FromRgb(233, 128, 45)),
                    ThreatSeverity.Medium => new SolidColorBrush(Color.FromRgb(255, 167, 38)),
                    ThreatSeverity.Low => new SolidColorBrush(Color.FromRgb(76, 175, 80)),
                    _ => new SolidColorBrush(Color.FromRgb(108, 117, 125))
                };
            }
            return new SolidColorBrush(Color.FromRgb(108, 117, 125));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>Converts threat severity to foreground color.</summary>
    public sealed class SeverityToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ThreatSeverity severity)
            {
                return severity switch
                {
                    ThreatSeverity.Critical => Brushes.White,
                    ThreatSeverity.High => Brushes.White,
                    ThreatSeverity.Medium => Brushes.Black,
                    ThreatSeverity.Low => Brushes.Black,
                    _ => Brushes.White
                };
            }
            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>Converts vulnerability severity to color.</summary>
    public sealed class VulnerabilitySeverityToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is VulnerabilitySeverity severity)
            {
                return severity switch
                {
                    VulnerabilitySeverity.Critical => Brushes.Red,
                    VulnerabilitySeverity.High => Brushes.OrangeRed,
                    VulnerabilitySeverity.Medium => Brushes.Orange,
                    VulnerabilitySeverity.Low => Brushes.LightGreen,
                    _ => Brushes.Gray
                };
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>Converts vulnerability severity to background color.</summary>
    public sealed class VulnerabilitySeverityToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is VulnerabilitySeverity severity)
            {
                return severity switch
                {
                    VulnerabilitySeverity.Critical => new SolidColorBrush(Color.FromRgb(220, 53, 69)),
                    VulnerabilitySeverity.High => new SolidColorBrush(Color.FromRgb(233, 128, 45)),
                    VulnerabilitySeverity.Medium => new SolidColorBrush(Color.FromRgb(255, 167, 38)),
                    VulnerabilitySeverity.Low => new SolidColorBrush(Color.FromRgb(76, 175, 80)),
                    _ => new SolidColorBrush(Color.FromRgb(108, 117, 125))
                };
            }
            return new SolidColorBrush(Color.FromRgb(108, 117, 125));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>Converts vulnerability severity to foreground color.</summary>
    public sealed class VulnerabilitySeverityToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is VulnerabilitySeverity severity)
            {
                return severity switch
                {
                    VulnerabilitySeverity.Critical => Brushes.White,
                    VulnerabilitySeverity.High => Brushes.White,
                    VulnerabilitySeverity.Medium => Brushes.Black,
                    VulnerabilitySeverity.Low => Brushes.Black,
                    _ => Brushes.White
                };
            }
            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>Converts incident severity to color.</summary>
    public sealed class IncidentSeverityToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IncidentSeverity severity)
            {
                return severity switch
                {
                    IncidentSeverity.Critical => Brushes.Red,
                    IncidentSeverity.High => Brushes.OrangeRed,
                    IncidentSeverity.Medium => Brushes.Orange,
                    IncidentSeverity.Low => Brushes.LightGreen,
                    _ => Brushes.Gray
                };
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>Converts incident severity to background color.</summary>
    public sealed class IncidentSeverityToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IncidentSeverity severity)
            {
                return severity switch
                {
                    IncidentSeverity.Critical => new SolidColorBrush(Color.FromRgb(220, 53, 69)),
                    IncidentSeverity.High => new SolidColorBrush(Color.FromRgb(233, 128, 45)),
                    IncidentSeverity.Medium => new SolidColorBrush(Color.FromRgb(255, 167, 38)),
                    IncidentSeverity.Low => new SolidColorBrush(Color.FromRgb(76, 175, 80)),
                    _ => new SolidColorBrush(Color.FromRgb(108, 117, 125))
                };
            }
            return new SolidColorBrush(Color.FromRgb(108, 117, 125));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>Converts incident severity to foreground color.</summary>
    public sealed class IncidentSeverityToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IncidentSeverity severity)
            {
                return severity switch
                {
                    IncidentSeverity.Critical => Brushes.White,
                    IncidentSeverity.High => Brushes.White,
                    IncidentSeverity.Medium => Brushes.Black,
                    IncidentSeverity.Low => Brushes.Black,
                    _ => Brushes.White
                };
            }
            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>Subtract converter for calculations.</summary>
    public sealed class SubtractConverter : IValueConverter, IMultiValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int a && parameter is int b)
                return Math.Max(0, a - b);
            if (value is int val)
                return val;
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is int a && values[1] is int b)
                return Math.Max(0, a - b);
            return 0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
