using System.Windows;
using System.Windows.Controls;

namespace DotResolution.Views.Controls
{
    /// <summary>
    /// 階層構造でグリッド表示できるコントロールです。
    /// </summary>
    /// <remarks>
    /// TreeView + DataGrid のようなイメージです。
    /// </remarks>
    public class TreeListView : TreeView
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeListViewItem();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TreeListViewItem;
        }
    }
}
