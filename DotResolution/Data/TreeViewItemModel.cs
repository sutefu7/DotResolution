using DotResolution.Libraries;
using System.Collections.ObjectModel;

namespace DotResolution.Data
{
    /// <summary>
    /// TreeView にバインドするためのモデルです。
    /// </summary>
    /// <remarks>
    /// INotifyPropertyChanged を実装していないので、ソースコード側からプロパティを変更しても、バインド先の View 表示更新はできません。
    /// </remarks>
    public class TreeViewItemModel
    {
        /// <summary>
        /// 本項目のノードを選択中かどうか を取得、または設定します。
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// 本項目のノードが展開しているかどうか を取得、または設定します。
        /// </summary>
        public bool IsExpanded { get; set; }

        /// <summary>
        /// 定義の種類 を取得、または設定します。
        /// </summary>
        public DefinitionTypes DefinitionType { get; set; }

        /// <summary>
        /// ソースコードの場合、記載のプログラミング言語 を取得、または設定します。
        /// </summary>
        public LanguageTypes LanguageType { get; set; }

        /// <summary>
        /// 表示文字列 を取得、または設定します。
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 対応するファイルのパス を取得、または設定します。
        /// </summary>
        public string TargetFile { get; set; }

        /// <summary>
        /// 定義の開始位置 を取得、または設定します。
        /// </summary>
        public int StartOffset { get; set; }

        /// <summary>
        /// 定義の終了位置 を取得、または設定します。
        /// </summary>
        public int EndOffset { get; set; }

        /// <summary>
        /// 定義名の開始位置 を取得、または設定します。
        /// </summary>
        /// <remarks>
        /// class, struct, interface, module(VB) でのみ使用しています。
        /// </remarks>
        public int IdentifierTokenStartOffset { get; set; }

        /// <summary>
        /// 説明コメント を取得、または設定します。
        /// </summary>
        /// <remarks>
        /// コメントは、XML ドキュメントコメント、単一行コメント、行末コメント、の優先度で取得します。
        /// </remarks>
        public string Comment { get; set; }

        /// <summary>
        /// 任意のインスタンス を取得、または設定します。
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// 子となる項目のコレクション を取得、または設定します。
        /// </summary>
        public ObservableCollection<TreeViewItemModel> Children { get; set; }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        public TreeViewItemModel()
        {
            IsSelected = false;
            IsExpanded = false;
            DefinitionType = DefinitionTypes.None;
            LanguageType = LanguageTypes.None;
            Text = string.Empty;
            TargetFile = string.Empty;
            StartOffset = 0;
            EndOffset = 0;
            IdentifierTokenStartOffset = 0;
            Comment = string.Empty;
            Tag = null;
            Children = new ObservableCollection<TreeViewItemModel>();
        }

        /// <summary>
        /// インスタンスを分けたコピーを返却します。
        /// </summary>
        /// <returns></returns>
        public TreeViewItemModel Clone()
        {
            var model = new TreeViewItemModel
            {
                IsSelected = IsSelected,
                IsExpanded = IsExpanded,
                DefinitionType = DefinitionType,
                LanguageType = LanguageType,
                Text = Text,
                TargetFile = TargetFile,
                StartOffset = StartOffset,
                EndOffset = EndOffset,
                IdentifierTokenStartOffset = IdentifierTokenStartOffset,
                Comment = Comment,
                Tag = Tag,
            };

            foreach (var child in Children)
                model.Children.Add(child.Clone());

            return model;
        }
    }
}
