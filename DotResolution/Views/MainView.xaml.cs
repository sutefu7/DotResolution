using AvalonDock.Layout;
using DotResolution.Data;
using DotResolution.Libraries;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace DotResolution.Views
{
    /// <summary>
    /// メイン画面です。
    /// </summary>
    public partial class MainView : Window
    {
        /// <summary>
        /// ソリューションツリーにバインドするコレクションを取得、または設定します。
        /// </summary>
        public ObservableCollection<TreeViewItemModel> SolutionTreeModels { get; set; }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        public MainView()
        {
            InitializeComponent();

            // 表示倍率を取得
            var cfgFile = AppEnv.ExeFile.Replace(".exe", ".cfg.txt");
            if (!File.Exists(cfgFile))
            {
                File.WriteAllText(cfgFile, "Magnification=1.0\r\n");
                File.AppendAllText(cfgFile, "BothTypeTreeDefaultLocationIsTopLeft=false\r\n");
            }

            var magnification = 1.0d;
            var lines = File.ReadAllLines(cfgFile);
            foreach (var line in lines)
            {
                var items = line.Split('=');
                if (items[0] == "Magnification")
                    magnification = Convert.ToDouble(items[1]);

                if (items[0] == "BothTypeTreeDefaultLocationIsTopLeft")
                    AppEnv.BothTypeTreeDefaultLocationIsTopLeft = Convert.ToBoolean(items[1]);
            }

            scaleTransform1.ScaleX = magnification;
            scaleTransform1.ScaleY = magnification;

            // ウィンドウサイズ: ディスプレイサイズの半分、表示位置: 中央
            var displayWidth = SystemParameters.WorkArea.Width;
            var displayHeight = SystemParameters.WorkArea.Height;

            Width = displayWidth * (1.0 / 2.0);
            Height = displayHeight * (1.0 / 2.0);

            Left = (displayWidth / 2) - (Width / 2);
            Top = (displayHeight / 2) - (Height / 2);

            // 背景色を塗る
            SetResourceReference(BackgroundProperty, SystemColors.ControlBrushKey);

            //
            solutionTree.DataContext = this;
            AppEnv.MainView = this;

            SolutionTreeModels = new ObservableCollection<TreeViewItemModel>();
            BindingOperations.EnableCollectionSynchronization(SolutionTreeModels, new object());
        }

        /// <summary>
        /// ファイル　→　ソリューションの選択... 項目のクリックイベントです。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mniSolutionFileSelector_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            dlg.FileName = "";
            dlg.Filter = "Visual Studio ソリューション ファイル (.sln)|*.sln|すべてのファイル (*.*)|*.*";
            dlg.FilterIndex = 0;
            //dlg.DefaultExt = ".sln";

            var result = dlg.ShowDialog();
            if (result.GetValueOrDefault())
            {
                var fileName = dlg.FileName;
                Parse(fileName);
            }
        }

        private void Parse(string solutionFile)
        {
            ShowStatusBarMessage("ソースコードを読み込み中 ...");

            var dlg = new ProgressView();
            dlg.Owner = this;
            dlg.SolutionFile = solutionFile;
            dlg.ShowDialog();

            if (!IsActive)
                Activate();

            SolutionTreeModels.Add(dlg.Result);

            ShowStatusBarMessage("完了", true);
        }

        /// <summary>
        /// ファイル　→　終了 項目のクリックイベントです。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mniExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// エクスプローラーからのファイルドラッグ移動時のイベントです。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void solutionTree_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.All;
            else
                e.Effects = DragDropEffects.None;

            e.Handled = true;
        }

        /// <summary>
        /// エクスプローラーからのファイルドロップ時のイベントです。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void solutionTree_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
                var fileName = fileNames[0];
                Parse(fileName);
            }
        }

        /// <summary>
        /// 項目の選択イベントです。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void solutionTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var model = e.NewValue as TreeViewItemModel;
            if (model == null)
                return;

            // 後続処理できる種類かどうかを調べる
            var isTargetType = false;

            switch (model.DefinitionType)
            {
                case DefinitionTypes.SolutionFile:
                    isTargetType = true;
                    break;

                case DefinitionTypes.CSharpProjectFile:
                case DefinitionTypes.VisualBasicProjectFile:
                    isTargetType = true;
                    break;

                case DefinitionTypes.CSharpSourceFile:
                case DefinitionTypes.VisualBasicSourceFile:
                case DefinitionTypes.GeneratedFile:
                    isTargetType = true;
                    break;

                case DefinitionTypes.Unknown:

                    if (model.TargetFile.EndsWith(".xaml"))
                        isTargetType = true;

                    break;
            }

            if (!isTargetType)
                return;

            ShowDocumentPane(model);
        }

        private void ShowDocumentPane(TreeViewItemModel model)
        {
            // すでに開かれているドキュメントペインかどうかを調べる
            var targetContent = default(LayoutContent);

            foreach (var content in layoutDocumentPane1.Children)
            {
                if (content.ContentId == model.TargetFile)
                {
                    targetContent = content;
                    break;
                }
            }

            if (targetContent != null)
            {
                // すでに開かれているドキュメントペイン
                targetContent.IsSelected = true;

                if (model.StartOffset > 0)
                {
                    var analyzePanel1 = targetContent.Content as AnalyzePanel;
                    analyzePanel1.MoveTextEditorOffset(model.StartOffset);
                }
            }
            else
            {
                // 新規追加するドキュメントペイン
                var analyzePanel1 = new AnalyzePanel
                {
                    SelectedSolutionTreeModel = model,
                };

                targetContent = new LayoutDocument
                {
                    Title = model.Text,
                    ToolTip = model.TargetFile,
                    ContentId = model.TargetFile,
                    Content = analyzePanel1,
                };

                layoutDocumentPane1.Children.Add(targetContent);
                targetContent.IsSelected = true;

                if (model.StartOffset > 0)
                {
                    // 追加したコントロールが null になってしまうバグの対応。原因は analyzePanel1 のロードイベントが走っていないため。
                    // 追加しただけではなく、いったん表示させた後（ロードイベントを走らせた後）、実行するように対応
                    Task.Run(async () =>
                    {
                        while (!analyzePanel1.IsLoadedCompleted)
                            await Task.Delay(10);

                        Dispatcher.Invoke(new Action(() => { analyzePanel1.MoveTextEditorOffset(model.StartOffset); }));
                    });
                }
            }
        }

        /// <summary>
        /// 指定のソースファイルに該当するツリーノードを検索して、見つかった場合は強制選択させて、ドキュメントペインに表示します。既に表示されている場合はアクティブ状態にします。
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="offset"></param>
        public void ShowDocumentPane(string sourceFile, int offset)
        {
            var foundModel = SearchModel(sourceFile);
            if (foundModel == null)
                return;

            var model = foundModel.Clone();
            model.StartOffset = offset;

            ShowDocumentPane(model);
        }

        // 指定のソースファイルに対応するモデルを探します。
        private TreeViewItemModel SearchModel(string sourceFile)
        {
            foreach (var model in SolutionTreeModels)
            {
                if (model.TargetFile == sourceFile)
                    return model;

                var child = SearchModel(model, sourceFile);
                if (child != null)
                    return child;
            }

            return null;
        }

        // 指定のソースファイルに対応するモデルを探します。
        private TreeViewItemModel SearchModel(TreeViewItemModel parent, string sourceFile)
        {
            foreach (var model in parent.Children)
            {
                if (model.TargetFile == sourceFile)
                    return model;

                var child = SearchModel(model, sourceFile);
                if (child != null)
                    return child;
            }

            return null;
        }

        /// <summary>
        /// ステータスバーに文字列を表示します。
        /// </summary>
        /// <remarks>
        /// autoClear が true の場合、表示した文字列は 3 秒後に消えます。
        /// </remarks>
        /// <param name="s"></param>
        /// <param name="autoClear"></param>
        public void ShowStatusBarMessage(string s, bool autoClear = false)
        {
            statusBarMessage.Text = s;
            DoEvents();

            if (autoClear)
            {
                Task.Run(async () =>
                {
                    await Task.Delay(3000);
                    await Dispatcher.InvokeAsync(() =>
                    {
                        statusBarMessage.Text = string.Empty;
                    });
                });
            }
        }

        // 現在メッセージ待ち行列の中にある全てのUIメッセージを処理します。
        // https://gist.github.com/pinzolo/2814091

        public static void DoEvents()
        {
            var frame = new DispatcherFrame();
            var callback = new DispatcherOperationCallback(obj =>
            {
                (obj as DispatcherFrame).Continue = false;
                return null;
            });

            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, callback, frame);
            Dispatcher.PushFrame(frame);
        }

    }
}
