using AvalonDock.Layout;
using DotResolution.Data;
using DotResolution.Libraries;
using DotResolution.Libraries.Roslyns;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace DotResolution.Views
{
    /// <summary>
    /// ターゲットファイルの分析画面です。
    /// </summary>
    public partial class AnalyzePanel : UserControl
    {
        private EditorPanel editorPanel1 = null;
        private RelationTreePanel solutionRelationPanel = null;
        private RelationTreePanel projectRelationPanel = null;
        private RelationTreePanel baseTypeRelationPanel = null;
        private RelationTreePanel inheritanceTypeRelationPanel = null;
        private RelationTreePanel bothTypeRelationPanel = null;

        public bool IsLoadedCompleted = false;

        /// <summary>
        /// ソリューションエクスプローラーペインで選択されたモデル を取得、または設定します。
        /// </summary>
        public TreeViewItemModel SelectedSolutionTreeModel { get; set; }

        /// <summary>
        /// 定義一覧ツリーにバインドするコレクションを取得、または設定します。
        /// </summary>
        public ObservableCollection<TreeViewItemModel> DefinitionTreeModels { get; set; } = new ObservableCollection<TreeViewItemModel>();

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        public AnalyzePanel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ロードイベントです。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // 画面が新規作成されたタイミングだけではなく、画面が再アクティブ状態になるたびに Loaded イベントが発生するみたい？
            if (IsLoadedCompleted)
                return;
            IsLoadedCompleted = true;

            // ※DefinitionTreeModels をバインドするため
            DataContext = this;

            switch (SelectedSolutionTreeModel.DefinitionType)
            {
                case DefinitionTypes.SolutionFile:
                case DefinitionTypes.CSharpProjectFile:
                case DefinitionTypes.VisualBasicProjectFile:

                    //
                    definitionTreeAnchorable.Hide();

                    //
                    var layoutDocument1 = default(LayoutDocument);
                    if (SelectedSolutionTreeModel.DefinitionType == DefinitionTypes.SolutionFile)
                    {
                        // プロジェクト間の参照ツリー（全体）ペイン
                        solutionRelationPanel = new RelationTreePanel();
                        layoutDocument1 = new LayoutDocument { Title = "プロジェクト間の参照ツリー（全体）", Content = solutionRelationPanel };
                        categoryDocumentPane.Children.Add(layoutDocument1);

                        var models1 = CreateSolutionReferenceModels();
                        solutionRelationPanel.ShowData(models1);
                    }
                    else
                    {
                        // プロジェクト間の参照ツリー（個別）ペイン
                        projectRelationPanel = new RelationTreePanel();
                        layoutDocument1 = new LayoutDocument { Title = "プロジェクト間の参照ツリー（個別）", Content = projectRelationPanel };
                        categoryDocumentPane.Children.Add(layoutDocument1);

                        var models1 = CreateProjectReferenceModels();
                        projectRelationPanel.ShowData(models1);
                    }

                    // ソースコードペイン
                    editorPanel1 = new EditorPanel();
                    var layoutDocument2 = new LayoutDocument { Title = "ソースコード", Content = editorPanel1, };
                    categoryDocumentPane.Children.Add(layoutDocument2);
                    editorPanel1.AddFormattedSourceCodePane = AddFormattedSourceCodePane;
                    editorPanel1.SetTextFile(SelectedSolutionTreeModel.TargetFile);

                    // 参照ツリーペインをアクティブ状態にする
                    layoutDocument1.IsSelected = true;

                    break;

                default:

                    AppEnv.LanguageType = SelectedSolutionTreeModel.LanguageType;
                    var models2 = RoslynHelper.CreateDefinitionTree(SelectedSolutionTreeModel);
                    foreach (var model in models2)
                        DefinitionTreeModels.Add(model);

                    // ソースコードペイン
                    editorPanel1 = new EditorPanel();
                    var layoutDocument3 = new LayoutDocument { Title = "ソースコード", Content = editorPanel1 };
                    categoryDocumentPane.Children.Add(layoutDocument3);
                    editorPanel1.AddFormattedSourceCodePane = AddFormattedSourceCodePane;
                    editorPanel1.SetTextFile(SelectedSolutionTreeModel.TargetFile);

                    // 継承元ツリーペイン
                    baseTypeRelationPanel = new RelationTreePanel { IsDisplayTreeListView = true };
                    var layoutDocument4 = new LayoutDocument { Title = "継承元ツリー", Content = baseTypeRelationPanel };
                    categoryDocumentPane.Children.Add(layoutDocument4);

                    // 継承先ツリーペイン
                    inheritanceTypeRelationPanel = new RelationTreePanel { IsDisplayTreeListView = true };
                    var layoutDocument5 = new LayoutDocument { Title = "継承先ツリー", Content = inheritanceTypeRelationPanel };
                    categoryDocumentPane.Children.Add(layoutDocument5);

                    // 継承ツリーペイン
                    bothTypeRelationPanel = new RelationTreePanel { IsDisplayTreeListView = true };
                    var layoutDocument6 = new LayoutDocument { Title = "継承ツリー", Content = bothTypeRelationPanel };
                    categoryDocumentPane.Children.Add(layoutDocument6);

                    // ソースコードペインをアクティブ状態にする
                    layoutDocument3.IsSelected = true;

                    break;
            }
        }

        // 継承元ツリーコレクション の作成。バインドしないで使うので、ObservableCollection<T> ではなく List<T>
        private List<DefinitionHeaderModel> CreateBaseTypeModels(TreeViewItemModel containerModel)
        {
            var models = RoslynHelper.CreateBaseTypeModels(containerModel);
            return models;
        }

        // 継承先ツリーコレクション の作成。バインドしないで使うので、ObservableCollection<T> ではなく List<T>
        private List<DefinitionHeaderModel> CreateInheritanceTypeModels(TreeViewItemModel containerModel)
        {
            var models = RoslynHelper.CreateInheritanceTypeModels(containerModel);
            return models;
        }

        // プロジェクト間の参照ツリー（全体）コレクション の作成。バインドしないで使うので、ObservableCollection<T> ではなく List<T>
        private List<DefinitionHeaderModel> CreateSolutionReferenceModels()
        {
            var models = RoslynHelper.CreateSolutionReferenceModels(SelectedSolutionTreeModel);
            return models;
        }

        // プロジェクト間の参照ツリー（個別）コレクション の作成。バインドしないで使うので、ObservableCollection<T> ではなく List<T>
        private List<DefinitionHeaderModel> CreateProjectReferenceModels()
        {
            var models = RoslynHelper.CreateProjectReferenceModels(SelectedSolutionTreeModel);
            return models;
        }

        /// <summary>
        /// 項目の選択イベントです。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void definitionTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var model = e.NewValue as TreeViewItemModel;
            if (model == null)
                return;

            UpdatePanes(model);
        }

        /// <summary>
        /// ソースコードペインのエディターに対して、表示位置を移動します。
        /// </summary>
        /// <remarks>
        /// 同時に、継承元ツリーペイン、継承先ツリーペインの表示更新も実施します。
        /// </remarks>
        /// <param name="offset"></param>
        public void MoveTextEditorOffset(int offset)
        {
            var model = SearchModel(offset);
            if (model == null)
            {
                // 通常ここに来ることは無いはず？何も定義していない位置の場合、ソースコードの表示だけ更新
                // ソースコードペイン、定義位置に移動する
                editorPanel1.MoveTextEditorOffset(offset);
                return;
            }

            UpdatePanes(model);
        }

        // 指定の停止開始位置に対応するモデルを探します。
        private TreeViewItemModel SearchModel(int offset)
        {
            foreach (var model in DefinitionTreeModels)
            {
                if (model.StartOffset == offset)
                    return model;

                var child = SearchModel(model, offset);
                if (child != null)
                    return child;
            }

            return null;
        }

        // 指定の停止開始位置に対応するモデルを探します。
        private TreeViewItemModel SearchModel(TreeViewItemModel parent, int offset)
        {
            foreach (var model in parent.Children)
            {
                if (model.StartOffset == offset)
                    return model;

                var child = SearchModel(model, offset);
                if (child != null)
                    return child;
            }

            return null;
        }


        // ソースコードペイン、継承元ツリーペイン、継承元ツリーペイン、の表示更新
        private void UpdatePanes(TreeViewItemModel model)
        {
            AppEnv.MainView.ShowStatusBarMessage("全てのソースコードをチェック中 ...");

            // ソースコードペイン、定義位置に移動する
            editorPanel1.MoveTextEditorOffset(model.StartOffset);

            switch (model.DefinitionType)
            {
                case DefinitionTypes.Class:
                case DefinitionTypes.Struct:
                case DefinitionTypes.Interface:

                    // 継承元ツリーペイン、表示更新
                    var baseTypeModels = CreateBaseTypeModels(model);
                    baseTypeRelationPanel.ShowData(baseTypeModels);

                    // 継承先ツリーペイン、表示更新
                    var inheritanceTypeModels = CreateInheritanceTypeModels(model);
                    inheritanceTypeRelationPanel.ShowData(inheritanceTypeModels);

                    // 継承ツリーペイン、表示更新
                    bothTypeRelationPanel.ShowDataForLeftSideBaseTypes(baseTypeModels);
                    bothTypeRelationPanel.ShowDataForRightSideInheritanceTypes(inheritanceTypeModels);

                    break;

                default:

                    // 継承元ツリーペイン、表示クリア
                    baseTypeModels = new List<DefinitionHeaderModel>();
                    baseTypeRelationPanel.ShowData(baseTypeModels);

                    // 継承先ツリーペイン、表示クリア
                    inheritanceTypeModels = new List<DefinitionHeaderModel>();
                    inheritanceTypeRelationPanel.ShowData(inheritanceTypeModels);

                    // 継承ツリーペイン、表示クリア
                    bothTypeRelationPanel.ShowDataForLeftSideBaseTypes(baseTypeModels);
                    bothTypeRelationPanel.ShowDataForRightSideInheritanceTypes(inheritanceTypeModels);

                    break;
            }

            AppEnv.MainView.ShowStatusBarMessage("完了", true);
        }

        // 本メソッドは、EditorPanel 内で呼び出されます。
        // フォーマットしたソースコードのペインを追加します。
        private void AddFormattedSourceCodePane()
        {
            var tmpFile = string.Empty;

            try
            {
                tmpFile = RoslynHelper.CreateFormattedFile(SelectedSolutionTreeModel.TargetFile);

                // ソースコードペイン
                var editorPanel2 = new EditorPanel();
                var layoutDocument7 = new LayoutDocument { Title = "整形されたソースコード", Content = editorPanel2, };
                categoryDocumentPane.Children.Insert(1, layoutDocument7);
                editorPanel2.IsFormattedSourceCodePane = true;
                editorPanel2.SetTextFile(tmpFile);
            }
            catch (Exception ex)
            {
                Logger.AppendAllText(ex.ToString());
                Messages.Error(ex.Message);
            }
            finally
            {
                if (File.Exists(tmpFile))
                    File.Delete(tmpFile);
            }
        }
    }
}
