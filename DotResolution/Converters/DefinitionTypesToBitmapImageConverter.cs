using DotResolution.Libraries;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DotResolution.Converters
{
    /// <summary>
    /// DefinitionTypes に対応する 画像ファイルのパス を返却するコンバーターです。
    /// </summary>
    public class DefinitionTypesToBitmapImageConverter : IValueConverter
    {
        /// <summary>
        /// DefinitionTypes に対応する 画像ファイルのパス を返却します。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var img = new BitmapImage(new Uri("/Images/Miscellaneousfile.png", UriKind.Relative));

            if (!(value is DefinitionTypes))
                return img;

            var types = (DefinitionTypes)value;
            switch (types)
            {
                case DefinitionTypes.None: break; // Miscellaneousfile.png

                case DefinitionTypes.Folder: img = new BitmapImage(new Uri("/Images/Folder_Collapse.png", UriKind.Relative)); break;
                case DefinitionTypes.SolutionFile: img = new BitmapImage(new Uri("/Images/Solution.png", UriKind.Relative)); break;

                case DefinitionTypes.CSharpProjectFile: img = new BitmapImage(new Uri("/Images/CSharpProject.png", UriKind.Relative)); break;
                case DefinitionTypes.VisualBasicProjectFile: img = new BitmapImage(new Uri("/Images/VBProject.png", UriKind.Relative)); break;

                case DefinitionTypes.CSharpSourceFileForHeader: img = new BitmapImage(new Uri("/Images/CSharpFile.png", UriKind.Relative)); break;
                case DefinitionTypes.VisualBasicSourceFileForHeader: img = new BitmapImage(new Uri("/Images/VBFile.png", UriKind.Relative)); break;
                case DefinitionTypes.CSharpSourceFile: img = new BitmapImage(new Uri("/Images/CSharpFile.png", UriKind.Relative)); break;
                case DefinitionTypes.VisualBasicSourceFile: img = new BitmapImage(new Uri("/Images/VBFile.png", UriKind.Relative)); break;
                case DefinitionTypes.GeneratedFile: img = new BitmapImage(new Uri("/Images/Generatedfile.png", UriKind.Relative)); break;

                case DefinitionTypes.Dependency: img = new BitmapImage(new Uri("/Images/Dependencies.png", UriKind.Relative)); break;

                case DefinitionTypes.Namespace: img = new BitmapImage(new Uri("/Images/Namespace.png", UriKind.Relative)); break;
                case DefinitionTypes.Class: img = new BitmapImage(new Uri("/Images/Class.png", UriKind.Relative)); break;
                case DefinitionTypes.Struct: img = new BitmapImage(new Uri("/Images/Structure.png", UriKind.Relative)); break;
                case DefinitionTypes.Interface: img = new BitmapImage(new Uri("/Images/Interface.png", UriKind.Relative)); break;
                case DefinitionTypes.Module: img = new BitmapImage(new Uri("/Images/Module.png", UriKind.Relative)); break;

                case DefinitionTypes.Field: img = new BitmapImage(new Uri("/Images/Field.png", UriKind.Relative)); break;
                case DefinitionTypes.Indexer: img = new BitmapImage(new Uri("/Images/Property.png", UriKind.Relative)); break;
                case DefinitionTypes.Property: img = new BitmapImage(new Uri("/Images/Property.png", UriKind.Relative)); break;

                case DefinitionTypes.Constructor: img = new BitmapImage(new Uri("/Images/Method.png", UriKind.Relative)); break;
                case DefinitionTypes.WindowsAPI: img = new BitmapImage(new Uri("/Images/Method.png", UriKind.Relative)); break;
                case DefinitionTypes.EventHandler: img = new BitmapImage(new Uri("/Images/Method.png", UriKind.Relative)); break;
                case DefinitionTypes.Method: img = new BitmapImage(new Uri("/Images/Method.png", UriKind.Relative)); break;
                case DefinitionTypes.Operator: img = new BitmapImage(new Uri("/Images/Operator.png", UriKind.Relative)); break;

                case DefinitionTypes.Delegate: img = new BitmapImage(new Uri("/Images/Delegate.png", UriKind.Relative)); break;
                case DefinitionTypes.Event: img = new BitmapImage(new Uri("/Images/Event.png", UriKind.Relative)); break;

                case DefinitionTypes.Enum: img = new BitmapImage(new Uri("/Images/Enum.png", UriKind.Relative)); break;
                case DefinitionTypes.EnumItem: img = new BitmapImage(new Uri("/Images/EnumItem.png", UriKind.Relative)); break;

                case DefinitionTypes.Unknown: break; // Miscellaneousfile.png
            }

            return img;
        }

        /// <summary>
        /// 画像ファイルのパス に対応する DefinitionTypes を返却します。
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
