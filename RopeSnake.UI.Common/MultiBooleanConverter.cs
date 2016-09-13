using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RopeSnake.UI.Common
{
    public sealed class MultiBooleanConverter : IMultiValueConverter
    {
        public enum BooleanMode
        {
            And,
            Or
        }

        public bool InvertResult { get; set; }
        public BooleanMode Mode { get; set; } = BooleanMode.And;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool result;

            switch (Mode)
            {
                case BooleanMode.And:
                    result = values.OfType<IConvertible>().All(c => c.ToBoolean(culture)) ^ InvertResult;
                    break;

                case BooleanMode.Or:
                    result = values.OfType<IConvertible>().Any(c => c.ToBoolean(culture)) ^ InvertResult;
                    break;

                default:
                    throw new Exception("Invalid mode");
            }

            var resultConverter = parameter as IValueConverter;
            if (resultConverter != null)
                return resultConverter.Convert(result, targetType, parameter, culture);

            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private bool ComputeAggregate(bool current, IConvertible next, CultureInfo culture)
        {
            if (Mode == BooleanMode.And)
                return current & next.ToBoolean(culture);
            else
                return current | next.ToBoolean(culture);
        }
    }
}
