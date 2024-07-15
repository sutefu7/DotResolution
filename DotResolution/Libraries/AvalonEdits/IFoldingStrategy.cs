using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;

namespace DotResolution.Libraries.AvalonEdits
{
    /// <summary>
    /// AvalonEdit 用に使う、展開・折り畳みルールを管理するためのインターフェースです。
    /// </summary>
    /// <remarks>
    /// C# / Visual Basic それぞれの言語用に継承してください。
    /// </remarks>
    public interface IFoldingStrategy
    {
        /// <summary>
        /// AvalonEdit に、展開・折り畳みルールを適用します。
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="document"></param>
        void UpdateFoldings(FoldingManager manager, TextDocument document);
    }
}
