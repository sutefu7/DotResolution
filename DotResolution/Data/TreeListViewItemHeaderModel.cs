using DotResolution.Libraries;

namespace DotResolution.Data
{
    /// <summary>
    /// TreeListViewItem.Header にセットするためのモデルです。
    /// </summary>
    /// <remarks>
    /// INotifyPropertyChanged を実装していないので、ソースコード側からプロパティを変更しても、バインド先の View 表示更新はできません。
    /// </remarks>
    public class TreeListViewItemHeaderModel
    {
        /// <summary>
        /// 定義の種類 を取得、または設定します。
        /// </summary>
        public DefinitionTypes DefinitionType { get; set; }

        /// <summary>
        /// メンバー名 を取得、または設定します。
        /// </summary>
        public string MemberName { get; set; }

        /// <summary>
        /// 型、または戻り値型 を取得、または設定します。
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// コメント を取得、または設定します。
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        public TreeListViewItemHeaderModel()
        {
            DefinitionType = DefinitionTypes.None;
            MemberName = string.Empty;
            TypeName = string.Empty;
            Comment = string.Empty;
        }
    }
}
