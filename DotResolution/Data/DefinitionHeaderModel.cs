using DotResolution.Libraries;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DotResolution.Data
{
    /// <summary>
    /// 継承元ツリー、継承先ツリー、プロジェクト間の参照関係（全体、個別）ツリー、にバインドするためのモデルです。
    /// </summary>
    /// <remarks>
    /// INotifyPropertyChanged を実装していないので、ソースコード側からプロパティを変更しても、バインド先の View 表示更新はできません。
    /// </remarks>
    public class DefinitionHeaderModel
    {
        /// <summary>
        /// この図形が定義一覧ツリーで選択した定義の図形かどうか を取得、または設定します。
        /// </summary>
        /// <remarks>
        /// 定義一覧ツリーで選択した定義の図形なら true、関連先の派生図形なら false
        /// </remarks>
        public bool IsTargetDefinition { get; set; }

        /// <summary>
        /// 一意の文字列 を取得、または設定します。
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 関連付けたい相手の ID を取得、または設定します。
        /// </summary>
        public string RelationID { get; set; }

        /// <summary>
        /// 矢印の向きが２つ目のコントロールの方向かどうか を取得、または設定します。
        /// </summary>
        /// <remarks>
        /// １つ目のコントロールと２つ目のコントロールの間に矢印線をつなげる場合、<br></br>
        /// true ... 矢印の向きが２つ目のコントロールの方向<br></br>
        /// false ... 矢印の向きが１つ目のコントロールの方向<br></br><br></br>
        /// 継承元ツリー ... false<br></br>
        /// 継承先ツリー ... true<br></br>
        /// プロジェクト間の参照関係（全体、個別）ツリー ... false
        /// </remarks>
        public bool IsArrowDirectionEnd { get; set; }



        /// <summary>
        /// 表示文字列 を取得、または設定します。
        /// </summary>
        public string DefinitionName { get; set; }

        /// <summary>
        /// 定義の種類 を取得、または設定します。
        /// </summary>
        public DefinitionTypes DefinitionType { get; set; }

        /// <summary>
        /// 定義ツリー を取得、または設定します。
        /// </summary>
        public TreeViewItemModel ReferenceModel { get; set; }

        /// <summary>
        /// 定義の開始位置 を取得、または設定します。
        /// </summary>
        public int StartOffset { get; set; }



        /// <summary>
        /// 継承元の定義名コレクション を取得、または設定します。
        /// </summary>
        public List<string> BaseTypes { get; set; }

        /// <summary>
        /// 継承元の定義名 を取得します。
        /// </summary>
        public string BaseType => string.Join(", ", BaseTypes);

        /// <summary>
        /// 継承元があるかどうか を取得します。
        /// </summary>
        public bool HasBaseType => !string.IsNullOrWhiteSpace(BaseType);



        /// <summary>
        /// 本項目のノードを選択中かどうか を取得、または設定します。
        /// </summary>
        public bool IsDifferenceFile { get; set; }

        /// <summary>
        /// 定義しているファイル名 を取得、または設定します。
        /// </summary>
        public string DifferenceName { get; set; }

        /// <summary>
        /// 定義しているファイルのパス を取得、または設定します。
        /// </summary>
        public string DifferenceFile { get; set; }

        /// <summary>
        /// 定義の開始位置 を取得、または設定します。
        /// </summary>
        public int DifferenceFileStartOffset { get; set; }



        /// <summary>
        /// 本項目のノードが展開しているかどうか を取得、または設定します。
        /// </summary>
        /// <remarks>
        /// Expander 用です。
        /// </remarks>
        public bool IsExpanded { get; set; }



        /// <summary>
        /// メンバーツリーのコレクション を取得、または設定します。
        /// </summary>
        public ObservableCollection<TreeViewItemModel> MemberTreeItems { get; set; }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        public DefinitionHeaderModel()
        {
            IsTargetDefinition = false;
            ID = string.Empty;
            RelationID = string.Empty;
            IsArrowDirectionEnd = false;
            DefinitionName = string.Empty;
            DefinitionType = DefinitionTypes.None;
            ReferenceModel = null;
            StartOffset = 0;
            BaseTypes = new List<string>();
            IsDifferenceFile = false;
            DifferenceName = string.Empty;
            DifferenceFile = string.Empty;
            DifferenceFileStartOffset = 0;
            IsExpanded = false;
            MemberTreeItems = new ObservableCollection<TreeViewItemModel>();
        }
    }
}
