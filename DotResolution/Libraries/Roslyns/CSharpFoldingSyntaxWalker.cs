using ICSharpCode.AvalonEdit.Folding;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace DotResolution.Libraries.Roslyns
{
    /// <summary>
    /// C# のソースコードを分析して、展開・折りたたみ箇所を特定するクラスです。
    /// </summary>
    /// <remarks>
    /// デザインパターンの１つ、Visitor パターンで構文木を巡回していきます。
    /// </remarks>
    public class CSharpFoldingSyntaxWalker : CSharpSyntaxWalker, IFoldingSyntaxWalker
    {
        /// <summary>
        /// 展開・折りたたみ箇所のコレクションを取得、または設定します。
        /// </summary>
        public List<NewFolding> Items { get; set; }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        public CSharpFoldingSyntaxWalker() : base(SyntaxWalkerDepth.Trivia)
        {
            Items = new List<NewFolding>();
        }

        /// <summary>
        /// C# のソースコードを構文木に変換して、構文木の巡回を開始します。
        /// </summary>
        /// <param name="sourceFile"></param>
        public void Parse(string sourceFile)
        {
            var code = RoslynHelper.ReadAllText(sourceFile);
            var tree = CSharpSyntaxTree.ParseText(code);
            var node = tree.GetRoot();
            
            Visit(node);
        }



        /*
         * 展開・折りたたみ箇所を特定するためのスタック
         * Region は、ウォーターフォール開発モデルみたいな形式で、
         * 開始位置の Region が上から下へ、終了位置の Region が下から上へ、と対になっている
         * （一番最初に見つけた開始位置の Region は、一番最後に見つけた終了位置の Region に対応している）
         * 
         * そのため、開始位置を見つけたら、その位置をバックアップしておいて、
         * 終了位置を見つけたら、バックアップしておいた直前の開始位置とを対応させて、返却するようにする
         * 
         * 
         */
        private Stack<RegionDirectiveTriviaSyntax> _RegionStack;

        // 以下、VisitXxx(XxxSyntax) は、
        // Region ディレクティブ、名前空間、クラス、構造体、...、のようなコンテナ系定義に関する、API フックみたいなもの

        // 開始位置の Region ディレクティブ
        public override void VisitRegionDirectiveTrivia(RegionDirectiveTriviaSyntax node)
        {
            if (_RegionStack == null)
                _RegionStack = new Stack<RegionDirectiveTriviaSyntax>();

            _RegionStack.Push(node);
            base.VisitRegionDirectiveTrivia(node);
        }

        // 終了位置の Region ディレクティブ
        public override void VisitEndRegionDirectiveTrivia(EndRegionDirectiveTriviaSyntax node)
        {
            AddRegionData(node);
            base.VisitEndRegionDirectiveTrivia(node);
        }



        // 以降は、開始位置・終了位置両方持っている

        public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            AddDeclarationData(node);
            base.VisitNamespaceDeclaration(node);
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            AddDeclarationData(node);
            base.VisitClassDeclaration(node);
        }

        public override void VisitStructDeclaration(StructDeclarationSyntax node)
        {
            AddDeclarationData(node);
            base.VisitStructDeclaration(node);
        }

        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            AddDeclarationData(node);
            base.VisitInterfaceDeclaration(node);
        }

        public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            AddDeclarationData(node);
            base.VisitEnumDeclaration(node);
        }

        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            AddDeclarationData(node);
            base.VisitConstructorDeclaration(node);
        }

        public override void VisitOperatorDeclaration(OperatorDeclarationSyntax node)
        {
            AddDeclarationData(node);
            base.VisitOperatorDeclaration(node);
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            AddDeclarationData(node);
            base.VisitMethodDeclaration(node);
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            AddDeclarationData(node);
            base.VisitPropertyDeclaration(node);
        }

        public override void VisitAccessorDeclaration(AccessorDeclarationSyntax node)
        {
            AddDeclarationData(node);
            base.VisitAccessorDeclaration(node);
        }

        public override void VisitEventDeclaration(EventDeclarationSyntax node)
        {
            AddDeclarationData(node);
            base.VisitEventDeclaration(node);
        }

        private void AddRegionData(CSharpSyntaxNode node)
        {
            var startSyntax = _RegionStack.Pop();
            var startLength = startSyntax.Span.Start;
            var endLength = node.Span.End;

            // #region aaa のうち、冒頭の「#region」を除去
            var header = startSyntax.ToString();
            header = header.Substring("#region ".Length);

            Items.Add(new NewFolding
            {
                Name = header,
                StartOffset = startLength,
                EndOffset = endLength,
            });
        }

        private void AddDeclarationData(CSharpSyntaxNode node)
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
