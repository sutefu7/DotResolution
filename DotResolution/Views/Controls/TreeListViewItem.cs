using System.Windows;
using System.Windows.Controls;

namespace DotResolution.Views.Controls
{
    /// <summary>
    /// TreeListView の１つ分の項目となるコントロールです。
    /// </summary>
    public class TreeListViewItem : TreeViewItem
    {
        private int _level = -1;

        /// <summary>
        /// 本項目の階層レベル（インデントの深さ）を取得します。
        /// </summary>
        public int Level
        {
            get
            {
                if (_level == -1)
                {
                    TreeListViewItem parent = ItemsControl.ItemsControlFromItemContainer(this) as TreeListViewItem;
                    _level = (parent != null) ? parent.Level + 1 : 0;
                }
                return _level;
            }
        }

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
