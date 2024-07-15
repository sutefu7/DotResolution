using DotResolution.Libraries;
using DotResolution.Libraries.AvalonEdits;
using DotResolution.Libraries.Roslyns;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace DotResolution.Views
{
    /// <summary>
    /// 指定のテキストファイルを表示する画面です。
    /// </summary>
    /// <remarks>
    /// C# / Visual Basic の場合、キーワード定義にしたがって色分け表示します。
    /// </remarks>
    public partial class EditorPanel : UserControl
    {
        private FoldingManager currentManager = null;

        /// <summary>
        /// 整形したソースコードペインを追加します。
        /// </summary>
        public Action AddFormattedSourceCodePane { get; set; } = null;

        /// <summary>
        /// このソースコードペインが、整形されたソースコードペインかどうかを取得、または設定します。
        /// </summary>
        public bool IsFormattedSourceCodePane { get; set; } = false;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        public EditorPanel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// キャレット位置をもとに、定義元を探して移動します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GotoDefinitionItem_Click(object sender, RoutedEventArgs e)
        {
            var extension = Path.GetExtension(textEditor1.Document.FileName).ToLower();
            var sourceFile = textEditor1.Document.FileName;
            var offset = textEditor1.CaretOffset;

            if (extension == ".cs")
            {
                var result = RoslynHelper.FindDefinitionSourceAtPositionForCSharp(sourceFile, offset);
                if (string.IsNullOrEmpty(result.Item1))
                {
                    //Messages.Information("カレットの下のシンボルに移動できません。");
                    Messages.Information("定義位置が見つかりませんでした。");
                }
                else
                {
                    AppEnv.MainView.ShowDocumentPane(result.Item1, result.Item2);
                }
            }

            if (extension == ".vb")
            {
                var result = RoslynHelper.FindDefinitionSourceAtPositionForVisualBasic(sourceFile, offset);
                if (string.IsNullOrEmpty(result.Item1))
                {
                    //Messages.Information("カレットの下のシンボルに移動できません。");
                    Messages.Information("定義位置が見つかりませんでした。");
                }
                else
                {
                    AppEnv.MainView.ShowDocumentPane(result.Item1, result.Item2);
                }
            }
        }

        /// <summary>
        /// 整形したソースコードペインを追加します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormatSourceCodeItem_Click(object sender, RoutedEventArgs e)
        {
            AddFormattedSourceCodePane?.Invoke();
        }

        /// <summary>
        /// 指定のテキストファイルを表示します。
        /// </summary>
        /// <param name="textFile"></param>
        public void SetTextFile(string textFile)
        {
            // TextEditor 初期設定
            textEditor1.IsReadOnly = true;
            textEditor1.ShowLineNumbers = true;

            textEditor1.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            textEditor1.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;

            textEditor1.Document.FileName = textFile;
            textEditor1.Options.HighlightCurrentLine = true;
            
            textEditor1.Load(textFile);

            // 拡張子別の色分け
            var result = default(IHighlightingDefinition);
            var extension = Path.GetExtension(textFile).ToLower();

            switch (extension)
            {
                case ".cs": result = HighlightingManager.Instance.GetDefinition("C#"); break;
                case ".vb": result = HighlightingManager.Instance.GetDefinition("VB"); break;

                case ".csproj": result = HighlightingManager.Instance.GetDefinition("XML"); break;
                case ".vbproj": result = HighlightingManager.Instance.GetDefinition("XML"); break;
                case ".xaml": result = HighlightingManager.Instance.GetDefinition("XML"); break;

                default: result = null; break;
            }

            textEditor1.SyntaxHighlighting = result;

            // コンテキストメニュー
            if (!IsFormattedSourceCodePane)
            {
                if (extension == ".cs" || extension == ".vb")
                {
                    var contextMenu1 = new ContextMenu();
                    var gotoDefinitionItem = new MenuItem { Header = "定義へ移動" };
                    var formatSourceCodeItem = new MenuItem { Header = "ソースコードを整形して表示" };

                    gotoDefinitionItem.Click += GotoDefinitionItem_Click;
                    formatSourceCodeItem.Click += FormatSourceCodeItem_Click;

                    contextMenu1.Items.Add(gotoDefinitionItem);
                    contextMenu1.Items.Add(formatSourceCodeItem);
                    textEditor1.ContextMenu = contextMenu1;
                }
            }

            // C# / Visual Basic の場合、折り畳み機能を追加する
            if (currentManager != null)
                FoldingManager.Uninstall(currentManager);

            if (extension == ".cs")
            {
                var strategy = new CSharpFoldingStrategy();
                currentManager = FoldingManager.Install(textEditor1.TextArea);
                strategy.UpdateFoldings(currentManager, textEditor1.Document);
            }

            if (extension == ".vb")
            {
                var strategy = new VisualBasicFoldingStrategy();
                currentManager = FoldingManager.Install(textEditor1.TextArea);
                strategy.UpdateFoldings(currentManager, textEditor1.Document);
            }
        }

        /// <summary>
        /// ソースコードペインのエディターに対して、表示位置を移動します。
        /// </summary>
        /// <param name="offset"></param>
        public void MoveTextEditorOffset(int offset)
        {
            textEditor1.CaretOffset = offset;

            // キャレット位置（メンバー定義位置）が見えるまでスクロールする
            var jumpLine = textEditor1.Document.GetLineByOffset(offset).LineNumber;
            textEditor1.ScrollToLine(jumpLine);
        }
    }
}
