using DotResolution.Libraries.Roslyns;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using System.Collections.Generic;
using System.Linq;

namespace DotResolution.Libraries.AvalonEdits
{
    /// <summary>
    /// AvalonEdit / Visual Basic 用の展開・折り畳みルールを管理するためのクラスです。
    /// </summary>
    public class VisualBasicFoldingStrategy : IFoldingStrategy
    {
        /// <summary>
        /// AvalonEdit に、C# 用の展開・折り畳みルールを適用します。
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="document"></param>
        public void UpdateFoldings(FoldingManager manager, TextDocument document)
        {
            var firstErrorOffset = -1;
            var foldings = CreateNewFoldings(document, firstErrorOffset);
            var sortedItems = foldings.OrderBy(x => x.StartOffset);

            manager.UpdateFoldings(sortedItems, firstErrorOffset);
        }

        // Class, Method などコンテナ単位で折り畳む開始位置、終了位置、折り畳んだ際の表示名を返却
        private IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, int firstErrorOffset)
        {
            var walker = new VisualBasicFoldingSyntaxWalker();
            walker.Parse(document.FileName);

            foreach (var item in walker.Items)
                yield return item;
        }
    }
}
