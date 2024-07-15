using DotResolution.Data;
using DotResolution.Libraries.Roslyns;
using System;
using System.Windows;

namespace DotResolution.Views
{
    /// <summary>
    /// 時間がかかる処理で表示させる画面です。
    /// </summary>
    public partial class ProgressView : Window
    {
        /// <summary>
        /// ソリューションファイル を取得、または設定します。
        /// </summary>
        public string SolutionFile { get; set; }

        /// <summary>
        /// ソリューションツリー を取得、または設定します。
        /// </summary>
        public TreeViewItemModel Result { get; set; }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        public ProgressView()
        {
            InitializeComponent();

            // 背景色を塗る
            SetResourceReference(BackgroundProperty, SystemColors.ControlBrushKey);

        }

        /// <summary>
        /// コントロールを描画した直後のイベントです。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Window_ContentRendered(object sender, EventArgs e)
        {
            Activate();
            Result = await RoslynHelper.CreateSolutionExplorerTreeAsync(SolutionFile);
            Close();
        }
    }
}
