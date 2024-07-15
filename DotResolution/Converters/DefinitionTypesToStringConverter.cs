using DotResolution.Libraries;
using System;
using System.Globalization;
using System.Windows.Data;

namespace DotResolution.Converters
{
    /// <summary>
    /// DefinitionTypes に対応する メンバー名の文字列 を返却するコンバーターです。
    /// </summary>
    public class DefinitionTypesToStringConverter : IValueConverter
    {
        /// <summary>
        /// DefinitionTypes に対応する メンバー名の文字列 を返却します。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is DefinitionTypes))
                return string.Empty;

            var memberName = Enum.GetName(typeof(DefinitionTypes), value);
            return memberName;
        }

        /// <summary>
        /// メンバー名の文字列 に対応する DefinitionTypes を返却します。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
