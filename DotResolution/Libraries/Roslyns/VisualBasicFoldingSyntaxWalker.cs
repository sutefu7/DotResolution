using ICSharpCode.AvalonEdit.Folding;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using System;
using System.Collections.Generic;

namespace DotResolution.Libraries.Roslyns
{
    /// <summary>
    /// Visual Basic のソースコードを分析して、展開・折りたたみ箇所を特定するクラスです。
    /// </summary>
    /// <remarks>
    /// デザインパターンの１つ、Visitor パターンで構文木を巡回していきます。
    /// </remarks>
    public class VisualBasicFoldingSyntaxWalker : VisualBasicSyntaxWalker, IFoldingSyntaxWalker
    {
        /// <summary>
        /// 展開・折りたたみ箇所のコレクションを取得、または設定します。
        /// </summary>
        public List<NewFolding> Items { get; set; }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        public VisualBasicFoldingSyntaxWalker() : base(SyntaxWalkerDepth.Node)
        {
            Items = new List<NewFolding>();
        }

        /// <summary>
        /// Visual Basic のソースコードを構文木に変換して、構文木の巡回を開始します。
        /// </summary>
        /// <param name="sourceFile"></param>
        public void Parse(string sourceFile)
        {
            var code = RoslynHelper.ReadAllText(sourceFile);
            var tree = VisualBasicSyntaxTree.ParseText(code);
            var node = tree.GetRoot();

            Visit(node);
        }

        // 以下、VisitXxx(XxxSyntax) は、
        // Region ディレクティブ、名前空間、クラス、構造体、...、のようなコンテナ系定義に関する、API フックみたいなもの

        public override void VisitCompilationUnit(CompilationUnitSyntax node)
        {
            AddRegionData(node);
            base.VisitCompilationUnit(node);
        }

        public override void VisitNamespaceBlock(NamespaceBlockSyntax node)
        {
            AddBlockData(node);
            base.VisitNamespaceBlock(node);
        }

        public override void VisitClassBlock(ClassBlockSyntax node)
        {
            AddBlockData(node);
            base.VisitClassBlock(node);
        }

        public override void VisitStructureBlock(StructureBlockSyntax node)
        {
            AddBlockData(node);
            base.VisitStructureBlock(node);
        }

        public override void VisitInterfaceBlock(InterfaceBlockSyntax node)
        {
            AddBlockData(node);
            base.VisitInterfaceBlock(node);
        }

        public override void VisitModuleBlock(ModuleBlockSyntax node)
        {
            AddBlockData(node);
            base.VisitModuleBlock(node);
        }

        public override void VisitEnumBlock(EnumBlockSyntax node)
        {
            AddBlockData(node);
            base.VisitEnumBlock(node);
        }

        public override void VisitConstructorBlock(ConstructorBlockSyntax node)
        {
            AddBlockData(node);
            base.VisitConstructorBlock(node);
        }

        public override void VisitOperatorBlock(OperatorBlockSyntax node)
        {
            AddBlockData(node);
            base.VisitOperatorBlock(node);
        }

        // SubBlock, FunctionBlock が含まれているか？
        public override void VisitMethodBlock(MethodBlockSyntax node)
        {
            AddBlockData(node);
            base.VisitMethodBlock(node);
        }

        public override void VisitPropertyBlock(PropertyBlockSyntax node)
        {
            AddBlockData(node);
            base.VisitPropertyBlock(node);
        }



        /*
         * 以下が含まれているか？
         * Property(GetAccessorBlock, SetAccessorBlock)
         * Custom Event(AddHandlerAccessorBlock, RemoveHandlerAccessorBlock, RaiseEventAccessorBlock)
         * 
         * 
         */

        public override void VisitAccessorBlock(AccessorBlockSyntax node)
        {
            AddBlockData(node);
            base.VisitAccessorBlock(node);
        }

        public override void VisitEventBlock(EventBlockSyntax node)
        {
            AddBlockData(node);
            base.VisitEventBlock(node);
        }

        private void AddRegionData(VisualBasicSyntaxNode node)
        {
            /*
             * 
             * どうやら、GetDirectives() が、階層関係なく再帰的に取得してくるみたい、かつ各ブロックで、ところどころ該当する同じ値を取得してくるので、
             * トップレベルのノードだけで、全ての #Region を取得して返却する
             *
             * 宣言順にリスト登録されている。対応する開始 Region と終了 Region を取得するため、スタックを利用する
             * （開始ノードが、終了分までの情報を持っていない）
             * 
             * 
             */

            var regionStack = new Stack<DirectiveTriviaSyntax>();

            foreach (var child in node.GetDirectives())
            {
                var childKind = child.Kind();
                switch (childKind)
                {
                    case SyntaxKind.RegionDirectiveTrivia:
                        regionStack.Push(child);
                        break;

                    case SyntaxKind.EndRegionDirectiveTrivia:

                        var startSyntax = regionStack.Pop();
                        var startLength = startSyntax.Span.Start;
                        var endLength = child.Span.End;

                        // #Region "aaa" のうち、冒頭の「#Region」と文字列を囲うダブルコーテーションを除去
                        var header = startSyntax.ToString();
                        header = header.Substring("#Region ".Length);
                        header = header.Substring(1);
                        header = header.Substring(0, header.Length - 1);

                        Items.Add(new NewFolding
                        {
                            Name = header,
                            StartOffset = startLength,
                            EndOffset = endLength,
                        });

                        break;
                }
            }
        }

        private void AddBlockData(VisualBasicSyntaxNode node)
        {
            // ブロック系は、開始ノードが、開始と終了の両方の文字列位置を持っているので、取得する
            var startLength = node.Span.Start;
            var endLength = node.Span.End;

            var header = node.ToString();
            if (header.Contains(Environment.NewLine))
            {
                header = header.Substring(0, header.IndexOf(Environment.NewLine));
                header = $"{header} ...";
            }

            Items.Add(new NewFolding
            {
                Name = header,
                StartOffset = startLength,
                EndOffset = endLength,
            });
        }
    }
}
