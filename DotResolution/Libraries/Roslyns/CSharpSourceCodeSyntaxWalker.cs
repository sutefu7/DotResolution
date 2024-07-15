using DotResolution.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DotResolution.Libraries.Roslyns
{
    /// <summary>
    /// C# のソースコードに記載された各定義情報を取得するクラスです。
    /// </summary>
    /// <remarks>
    /// ソースコードを構文木に変換して、Visitor パターンで巡回していきます。
    /// </remarks>
    public class CSharpSourceCodeSyntaxWalker : CSharpSyntaxWalker, ISourceCodeSyntaxWalker
    {
        private string SourceFile = string.Empty;

        /// <summary>
        /// 各定義情報を取得して作成した定義一覧ツリー を取得、または設定します。
        /// </summary>
        public ObservableCollection<TreeViewItemModel> DefinitionItems { get; set; } = new ObservableCollection<TreeViewItemModel>();

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        public CSharpSourceCodeSyntaxWalker() : base(SyntaxWalkerDepth.Trivia)
        {

        }

        /// <summary>
        /// C# のソースコードを構文木に変換して、構文木の巡回を開始します。
        /// </summary>
        public void Parse(string sourceFile)
        {
            SourceFile = sourceFile;

            var code = RoslynHelper.ReadAllText(sourceFile);
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();

            Visit(root);
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var defineToken = node.ChildTokens().FirstOrDefault(x => x.IsKind(SyntaxKind.IdentifierToken));
            var defineName = defineToken.ToString();

            // ジェネリック型を定義している場合
            if (node.ChildNodes().OfType<TypeParameterListSyntax>().Any())
            {
                var listNode = node.ChildNodes().OfType<TypeParameterListSyntax>().FirstOrDefault();
                var genericTypes = listNode
                    .ChildNodes()
                    .OfType<TypeParameterSyntax>()
                    .Select(x => x.ChildTokens().FirstOrDefault(y => y.IsKind(SyntaxKind.IdentifierToken)).ToString());

                defineName = $"{defineName}<{string.Join(", ", genericTypes)}>";
            }

            // 登録
            var model = new TreeViewItemModel
            {
                Tag = node,
                Text = defineName,
                DefinitionType = DefinitionTypes.Class,

                TargetFile = SourceFile,
                StartOffset = node.Span.Start,
                EndOffset = node.Span.End,
                IdentifierTokenStartOffset = defineToken.Span.Start,
                LanguageType = LanguageTypes.CSharp,
                IsExpanded = true,
                Comment = GetComment(node),
            };

            var parentModel = FindParentModel(model.StartOffset, model.EndOffset);

            if (parentModel != null)
                parentModel.Children.Add(model);
            else
                DefinitionItems.Add(model);

            //
            base.VisitClassDeclaration(node);
        }

        private TreeViewItemModel FindParentModel(int startOffset, int endOffset)
        {
            /*
             * クラスなどのコンテナ系定義の場合、
             * この定義の外側が、クラスだったらインナークラス定義、名前空間なら通常クラス定義、なのでチェック
             * インナー定義の場合、一番近い外側の定義を得る
             * 
             * namespace ns1
             *   class Class1       : 2. この親は違う（Class1aa にとっては）
             *     class Class1a    : 2. この親を見つけたい
             *       class Class1aa : 1. ここが現在位置だとしたら、
             * 
             * 
             * また、フィールドやプロパティなど、メンバーの親コンテナを探す際も、
             * 上記の考え方で直近のコンテナ定義を得る
             * 
             * 
             */

            // どの階層にいるモデルが該当するか分からないので、いったんあちこちから１次元配列に取ってきて、
            // その中で開始位置が一番大きいモデルが、親コンテナとなるはず
            var results = FindParentModelInternal(DefinitionItems, startOffset, endOffset);
            var candidates = results.OrderBy(x => x.StartOffset);
            var candidate = candidates.LastOrDefault();

            return candidate;
        }

        private IEnumerable<TreeViewItemModel> FindParentModelInternal(ObservableCollection<TreeViewItemModel> models, int startOffset, int endOffset)
        {
            foreach (var model in models)
            {
                switch (model.DefinitionType)
                {
                    case DefinitionTypes.Namespace:
                    case DefinitionTypes.Class:
                    case DefinitionTypes.Struct:
                    case DefinitionTypes.Interface:

                        if (model.StartOffset < startOffset && endOffset < model.EndOffset)
                            yield return model;

                        break;
                }

                if (model.Children.Count > 0)
                {
                    var childModels = FindParentModelInternal(model.Children, startOffset, endOffset);
                    foreach (var childModel in childModels)
                        yield return childModel;
                }
            }
        }

        public override void VisitStructDeclaration(StructDeclarationSyntax node)
        {
            var defineToken = node.ChildTokens().FirstOrDefault(x => x.IsKind(SyntaxKind.IdentifierToken));
            var defineName = defineToken.ToString();

            // ジェネリック型を定義している場合
            if (node.ChildNodes().OfType<TypeParameterListSyntax>().Any())
            {
                var listNode = node.ChildNodes().OfType<TypeParameterListSyntax>().FirstOrDefault();
                var genericTypes = listNode
                    .ChildNodes()
                    .OfType<TypeParameterSyntax>()
                    .Select(x => x.ChildTokens().FirstOrDefault(y => y.IsKind(SyntaxKind.IdentifierToken)).ToString());

                defineName = $"{defineName}<{string.Join(", ", genericTypes)}>";
            }

            // 登録
            var model = new TreeViewItemModel
            {
                Tag = node,
                Text = defineName,
                DefinitionType = DefinitionTypes.Struct,

                TargetFile = SourceFile,
                StartOffset = node.Span.Start,
                EndOffset = node.Span.End,
                IdentifierTokenStartOffset = defineToken.Span.Start,
                LanguageType = LanguageTypes.CSharp,
                IsExpanded = true,
                Comment = GetComment(node),
            };

            var parentModel = FindParentModel(model.StartOffset, model.EndOffset);

            if (parentModel != null)
                parentModel.Children.Add(model);
            else
                DefinitionItems.Add(model);

            //
            base.VisitStructDeclaration(node);
        }

        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            var defineToken = node.ChildTokens().FirstOrDefault(x => x.IsKind(SyntaxKind.IdentifierToken));
            var defineName = defineToken.ToString();

            // ジェネリック型を定義している場合
            if (node.ChildNodes().OfType<TypeParameterListSyntax>().Any())
            {
                var listNode = node.ChildNodes().OfType<TypeParameterListSyntax>().FirstOrDefault();
                var genericTypes = listNode
                    .ChildNodes()
                    .OfType<TypeParameterSyntax>()
                    .Select(x => x.ChildTokens().FirstOrDefault(y => y.IsKind(SyntaxKind.IdentifierToken)).ToString());

                defineName = $"{defineName}<{string.Join(", ", genericTypes)}>";
            }

            // 登録
            var model = new TreeViewItemModel
            {
                Tag = node,
                Text = defineName,
                DefinitionType = DefinitionTypes.Interface,

                TargetFile = SourceFile,
                StartOffset = node.Span.Start,
                EndOffset = node.Span.End,
                IdentifierTokenStartOffset = defineToken.Span.Start,
                LanguageType = LanguageTypes.CSharp,
                IsExpanded = true,
                Comment = GetComment(node),
            };

            var parentModel = FindParentModel(model.StartOffset, model.EndOffset);

            if (parentModel != null)
                parentModel.Children.Add(model);
            else
                DefinitionItems.Add(model);

            //
            base.VisitInterfaceDeclaration(node);
        }

        public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            var defineName = node.ChildTokens().FirstOrDefault(x => x.IsKind(SyntaxKind.IdentifierToken)).ToString();

            // 登録
            var model = new TreeViewItemModel
            {
                Tag = node,
                Text = defineName,
                DefinitionType = DefinitionTypes.Enum,

                TargetFile = SourceFile,
                StartOffset = node.Span.Start,
                EndOffset = node.Span.End,
                LanguageType = LanguageTypes.CSharp,
                IsExpanded = true,
                Comment = GetComment(node),
            };

            var parentModel = FindParentModel(model.StartOffset, model.EndOffset);

            if (parentModel != null)
                parentModel.Children.Add(model);
            else
                DefinitionItems.Add(model);

            // Enum Members
            var enumMembers = node.ChildNodes().OfType<EnumMemberDeclarationSyntax>().Select(x => x.Identifier.Text);

            foreach (var enumMember in enumMembers)
            {
                var childModel = new TreeViewItemModel
                {
                    Text = enumMember,
                    DefinitionType = DefinitionTypes.EnumItem,

                    TargetFile = SourceFile,
                    LanguageType = LanguageTypes.CSharp,
                    Comment = GetComment(node),
                };
                model.Children.Add(childModel);
            }

            //
            base.VisitEnumDeclaration(node);
        }

        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            var defineName = node.ChildTokens().FirstOrDefault(x => x.IsKind(SyntaxKind.IdentifierToken)).ToString();
            var methodArguments = string.Empty;

            if (node.ChildNodes().OfType<ParameterListSyntax>().FirstOrDefault().ChildNodes().Any())
            {
                var listNode = node.ChildNodes().OfType<ParameterListSyntax>().FirstOrDefault();
                methodArguments = GetMethodArguments(listNode);
            }
            
            methodArguments = $"({methodArguments})";
            defineName = $"{defineName}{methodArguments}";

            // 登録
            var model = new TreeViewItemModel
            {
                Tag = node,
                Text = defineName,
                DefinitionType = DefinitionTypes.Constructor,

                TargetFile = SourceFile,
                StartOffset = node.Span.Start,
                EndOffset = node.Span.End,
                LanguageType = LanguageTypes.CSharp,
                Comment = GetComment(node),
            };

            var parentModel = FindParentModel(model.StartOffset, model.EndOffset);

            if (parentModel != null)
                parentModel.Children.Add(model);
            else
                DefinitionItems.Add(model);

            //
            base.VisitConstructorDeclaration(node);
        }

        public override void VisitOperatorDeclaration(OperatorDeclarationSyntax node)
        {
            var returnType = node.ChildNodes().FirstOrDefault().ToString();
            var defineName = node.ChildTokens().LastOrDefault().ToString();
            var methodArguments = string.Empty;

            if (node.ChildNodes().OfType<ParameterListSyntax>().FirstOrDefault().ChildNodes().Any())
            {
                var listNode = node.ChildNodes().OfType<ParameterListSyntax>().FirstOrDefault();
                methodArguments = GetMethodArguments(listNode);
            }

            methodArguments = $"({methodArguments})";
            defineName = $"operator {defineName}{methodArguments} : {returnType}";

            // 登録
            var model = new TreeViewItemModel
            {
                Tag = node,
                Text = defineName,
                DefinitionType = DefinitionTypes.Operator,

                TargetFile = SourceFile,
                StartOffset = node.Span.Start,
                EndOffset = node.Span.End,
                LanguageType = LanguageTypes.CSharp,
                Comment = GetComment(node),
            };

            var parentModel = FindParentModel(model.StartOffset, model.EndOffset);

            if (parentModel != null)
                parentModel.Children.Add(model);
            else
                DefinitionItems.Add(model);

            //
            base.VisitOperatorDeclaration(node);
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var returnType = node.ChildNodes().FirstOrDefault(x => !(x is AttributeListSyntax)).ToString();
            var isSubMethod = node.ChildNodes().FirstOrDefault(x => !(x is AttributeListSyntax)).ChildTokens().Any(x => x.IsKind(SyntaxKind.VoidKeyword));
            var defineName = node.ChildTokens().FirstOrDefault(x => x.IsKind(SyntaxKind.IdentifierToken)).ToString();

            // ジェネリック型を定義している場合
            if (node.ChildNodes().OfType<TypeParameterListSyntax>().Any())
            {
                var listNode = node.ChildNodes().OfType<TypeParameterListSyntax>().FirstOrDefault();
                var genericTypes = listNode
                    .ChildNodes()
                    .OfType<TypeParameterSyntax>()
                    .Select(x => x.ChildTokens().FirstOrDefault(y => y.IsKind(SyntaxKind.IdentifierToken)).ToString());

                defineName = $"{defineName}<{string.Join(", ", genericTypes)}>";
            }

            // 引数
            var methodArguments = string.Empty;
            var isEventHandler = false;

            if (node.ChildNodes().OfType<ParameterListSyntax>().FirstOrDefault().ChildNodes().Any())
            {
                var listNode = node.ChildNodes().OfType<ParameterListSyntax>().FirstOrDefault();
                methodArguments = GetMethodArguments(listNode);

                isEventHandler = IsEventHandlerPatternDefinition(listNode);
            }

            methodArguments = $"({methodArguments})";
            defineName = $"{defineName}{methodArguments} : {returnType}";

            // メソッドの種類
            var definitionType = DefinitionTypes.Method;

            isEventHandler = (isSubMethod && isEventHandler);
            if (isEventHandler)
                definitionType = DefinitionTypes.EventHandler;

            // Method, EventHandler っぽくても、Windows API の場合は、こちらを優先する
            var isWinAPI = node.AttributeLists.Any(x => x.ToString().Contains("DllImport"));
            if (isWinAPI)
                definitionType = DefinitionTypes.WindowsAPI;

            // 登録
            var model = new TreeViewItemModel
            {
                Tag = node,
                Text = defineName,
                DefinitionType = definitionType,

                TargetFile = SourceFile,
                StartOffset = node.Span.Start,
                EndOffset = node.Span.End,
                LanguageType = LanguageTypes.CSharp,
                Comment = GetComment(node),
            };

            var parentModel = FindParentModel(model.StartOffset, model.EndOffset);

            if (parentModel != null)
                parentModel.Children.Add(model);
            else
                DefinitionItems.Add(model);

            //
            base.VisitMethodDeclaration(node);
        }

        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            var variable = node.ChildNodes().FirstOrDefault(x => x is VariableDeclarationSyntax);
            var variableType = variable.ChildNodes().FirstOrDefault().ToString();
            var fields = variable.ChildNodes().Where(x => x is VariableDeclaratorSyntax);

            foreach (VariableDeclaratorSyntax field in fields)
            {
                var startOffset = field.Span.Start;
                var endOffset = field.Span.End;

                var fieldName = field.ChildTokens().FirstOrDefault(x => x.IsKind(SyntaxKind.IdentifierToken)).ToString();
                var fieldType = variableType;

                fieldName = $"{fieldName} : {fieldType}";

                // 登録
                var model = new TreeViewItemModel
                {
                    Tag = field,
                    Text = fieldName,
                    DefinitionType = DefinitionTypes.Field,

                    TargetFile = SourceFile,
                    StartOffset = startOffset,
                    EndOffset = endOffset,
                    LanguageType = LanguageTypes.CSharp,
                    Comment = GetComment(node),
                };

                var parentModel = FindParentModel(model.StartOffset, model.EndOffset);

                if (parentModel != null)
                    parentModel.Children.Add(model);
                else
                    DefinitionItems.Add(model);
            }

            //
            base.VisitFieldDeclaration(node);
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            var defineType = node.ChildNodes().FirstOrDefault().ToString();
            var defineName = node.ChildTokens().FirstOrDefault(x => x.IsKind(SyntaxKind.IdentifierToken)).ToString();

            defineName = $"{defineName} : {defineType}";

            // 登録
            var model = new TreeViewItemModel
            {
                Tag = node,
                Text = defineName,
                DefinitionType = DefinitionTypes.Property,

                TargetFile = SourceFile,
                StartOffset = node.Span.Start,
                EndOffset = node.Span.End,
                LanguageType = LanguageTypes.CSharp,
                Comment = GetComment(node),
            };

            var parentModel = FindParentModel(model.StartOffset, model.EndOffset);

            if (parentModel != null)
                parentModel.Children.Add(model);
            else
                DefinitionItems.Add(model);

            //
            base.VisitPropertyDeclaration(node);
        }

        // 以下はプロパティではなく別扱いみたいです。本ツール内では、今のところプロパティ扱いします。
        // public object this[int index] { get; set; }
        public override void VisitIndexerDeclaration(IndexerDeclarationSyntax node)
        {
            var defineType = node.ChildNodes().FirstOrDefault().ToString();
            var defineName = node.ChildTokens().FirstOrDefault(x => x.IsKind(SyntaxKind.ThisKeyword)).ToString();
            var methodArguments = string.Empty;

            if (node.ChildNodes().OfType<BracketedParameterListSyntax>().Any())
            {
                if (node.ChildNodes().OfType<BracketedParameterListSyntax>().FirstOrDefault().ChildNodes().Any())
                {
                    var listNode = node.ChildNodes().OfType<BracketedParameterListSyntax>().FirstOrDefault();
                    methodArguments = GetIndexerArguments(listNode);
                }
            }

            methodArguments = $"[{methodArguments}]";
            defineName = $"{defineName}{methodArguments} : {defineType}";

            // 登録
            var model = new TreeViewItemModel
            {
                Tag = node,
                Text = defineName,
                DefinitionType = DefinitionTypes.Indexer,

                TargetFile = SourceFile,
                StartOffset = node.Span.Start,
                EndOffset = node.Span.End,
                LanguageType = LanguageTypes.CSharp,
                Comment = GetComment(node),
            };

            var parentModel = FindParentModel(model.StartOffset, model.EndOffset);

            if (parentModel != null)
                parentModel.Children.Add(model);
            else
                DefinitionItems.Add(model);

            //
            base.VisitIndexerDeclaration(node);
        }

        // 通常のイベント定義
        // public event EventHandler Clicked;
        public override void VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
        {
            var variable = node.ChildNodes().FirstOrDefault(x => x is VariableDeclarationSyntax);
            var defineType = variable.ChildNodes().FirstOrDefault().ToString();
            var defineName = variable.ChildNodes().FirstOrDefault(x => x is VariableDeclaratorSyntax).ToString();

            defineName = $"{defineName} : {defineType}";

            // 登録
            var model = new TreeViewItemModel
            {
                Tag = node,
                Text = defineName,
                DefinitionType = DefinitionTypes.Event,

                TargetFile = SourceFile,
                StartOffset = node.Span.Start,
                EndOffset = node.Span.End,
                LanguageType = LanguageTypes.CSharp,
                Comment = GetComment(node),
            };

            var parentModel = FindParentModel(model.StartOffset, model.EndOffset);

            if (parentModel != null)
                parentModel.Children.Add(model);
            else
                DefinitionItems.Add(model);

            //
            base.VisitEventFieldDeclaration(node);
        }

        // add/remove アクセサーを明示的に書く版, WPF で見る形式かも
        // public event EventHandler Moved
        // {
        //     add {}
        //     remove {}
        // }
        public override void VisitEventDeclaration(EventDeclarationSyntax node)
        {
            var defineType = node.ChildNodes().FirstOrDefault(x => x is IdentifierNameSyntax).ToString();
            var defineName = node.ChildTokens().FirstOrDefault(x => x.IsKind(SyntaxKind.IdentifierToken)).ToString();

            defineName = $"{defineName} : {defineType}";

            // 登録
            var model = new TreeViewItemModel
            {
                Tag = node,
                Text = defineName,
                DefinitionType = DefinitionTypes.Event,

                TargetFile = SourceFile,
                StartOffset = node.Span.Start,
                EndOffset = node.Span.End,
                LanguageType = LanguageTypes.CSharp,
                Comment = GetComment(node),
            };

            var parentModel = FindParentModel(model.StartOffset, model.EndOffset);

            if (parentModel != null)
                parentModel.Children.Add(model);
            else
                DefinitionItems.Add(model);

            //
            base.VisitEventDeclaration(node);
        }

        public override void VisitDelegateDeclaration(DelegateDeclarationSyntax node)
        {
            var returnType = node.ChildNodes().FirstOrDefault().ToString();
            var isSubMethod = node.ChildNodes().FirstOrDefault().ChildTokens().Any(x => x.IsKind(SyntaxKind.VoidKeyword));
            var defineName = node.ChildTokens().FirstOrDefault(x => x.IsKind(SyntaxKind.IdentifierToken)).ToString();

            // ジェネリック型を定義している場合
            if (node.ChildNodes().OfType<TypeParameterListSyntax>().Any())
            {
                var listNode = node.ChildNodes().OfType<TypeParameterListSyntax>().FirstOrDefault();
                var genericTypes = listNode
                    .ChildNodes()
                    .OfType<TypeParameterSyntax>()
                    .Select(x => x.ChildTokens().FirstOrDefault(y => y.IsKind(SyntaxKind.IdentifierToken)).ToString());

                defineName = $"{defineName}<{string.Join(", ", genericTypes)}>";
            }

            // 引数
            var methodArguments = string.Empty;
            var isEventHandler = false;

            if (node.ChildNodes().OfType<ParameterListSyntax>().FirstOrDefault().ChildNodes().Any())
            {
                var listNode = node.ChildNodes().OfType<ParameterListSyntax>().FirstOrDefault();
                methodArguments = GetMethodArguments(listNode);

                isEventHandler = IsEventHandlerPatternDefinition(listNode);
            }

            methodArguments = $"({methodArguments})";
            defineName = $"{defineName}{methodArguments} : {returnType}";

            // 登録
            var model = new TreeViewItemModel
            {
                Tag = node,
                Text = defineName,
                DefinitionType = DefinitionTypes.Delegate,

                TargetFile = SourceFile,
                StartOffset = node.Span.Start,
                EndOffset = node.Span.End,
                LanguageType = LanguageTypes.CSharp,
                Comment = GetComment(node),
            };

            var parentModel = FindParentModel(model.StartOffset, model.EndOffset);

            if (parentModel != null)
                parentModel.Children.Add(model);
            else
                DefinitionItems.Add(model);

            //
            base.VisitDelegateDeclaration(node);
        }

        private string GetMethodArguments(ParameterListSyntax node)
        {
            var sb = new StringBuilder();
            var parameters = node.ChildNodes().OfType<ParameterSyntax>();

            foreach (var parameter in parameters)
            {
                //
                var isIn = parameter.ChildTokens().Any(x => x.IsKind(SyntaxKind.InKeyword));
                var isOut = parameter.ChildTokens().Any(x => x.IsKind(SyntaxKind.OutKeyword));
                var isByRef = parameter.ChildTokens().Any(x => x.IsKind(SyntaxKind.RefKeyword));
                var isByVal = !isByRef;
                var isOptional = parameter.ChildNodes().Any(x => x is EqualsValueClauseSyntax);
                var isParamArray = parameter.ChildTokens().Any(x => x.IsKind(SyntaxKind.ParamsKeyword));

                var parameterName = parameter.ChildTokens().FirstOrDefault(x => x.IsKind(SyntaxKind.IdentifierToken)).ToString();
                var parameterType = parameter.ChildNodes().FirstOrDefault().ToString();
                parameterType = RemoveNamespace(parameterType);

                //
                if (sb.Length != 0) sb.Append(", ");
                if (isIn) sb.Append("in ");
                if (isOut) sb.Append("out ");
                if (isByRef) sb.Append("ref ");
                if (isParamArray) sb.Append("params ");
                if (isOptional) sb.Append("[");

                sb.Append(parameterType);

                if (isOptional) sb.Append("]");
            }

            return sb.ToString();
        }

        private Regex RemoveNamespaceRegex = null;

        private string RemoveNamespace(string parameterType)
        {
            // IEnumerable<Int32>
            // System.Collections.Generic.IEnumerable<System.Int32>
            // ↓
            // IEnumerable<Int32>
            if (parameterType.Contains("."))
            {
                if (RemoveNamespaceRegex == null)
                    RemoveNamespaceRegex = new Regex(@"(\w+\.)*");

                parameterType = RemoveNamespaceRegex.Replace(parameterType, string.Empty);
            }

            return parameterType;
        }

        private bool IsEventHandlerPatternDefinition(ParameterListSyntax node)
        {
            var parameters = node.ChildNodes().OfType<ParameterSyntax>();
            var firstType = string.Empty;
            var secondType = string.Empty;

            if (parameters.Count() == 2)
            {
                foreach (var parameter in parameters)
                {
                    var parameterType = parameter.ChildNodes().FirstOrDefault().ToString();
                    parameterType = RemoveNamespace(parameterType);

                    if (string.IsNullOrEmpty(firstType))
                    {
                        firstType = parameterType;
                        continue;
                    }

                    if (string.IsNullOrEmpty(secondType))
                    {
                        secondType = parameterType;
                        continue;
                    }
                }

                if (firstType == "object" && secondType.EndsWith("EventArgs"))
                    return true;
            }

            return false;
        }

        private string GetIndexerArguments(BracketedParameterListSyntax node)
        {
            var sb = new StringBuilder();
            var parameters = node.ChildNodes().OfType<ParameterSyntax>();

            foreach (var parameter in parameters)
            {
                //
                var isIn = parameter.ChildTokens().Any(x => x.IsKind(SyntaxKind.InKeyword));
                var isOut = parameter.ChildTokens().Any(x => x.IsKind(SyntaxKind.OutKeyword));
                var isByRef = parameter.ChildTokens().Any(x => x.IsKind(SyntaxKind.RefKeyword));
                var isByVal = !isByRef;
                var isOptional = parameter.ChildNodes().Any(x => x is EqualsValueClauseSyntax);
                var isParamArray = parameter.ChildTokens().Any(x => x.IsKind(SyntaxKind.ParamsKeyword));

                var parameterName = parameter.ChildTokens().FirstOrDefault(x => x.IsKind(SyntaxKind.IdentifierToken)).ToString();
                var parameterType = parameter.ChildNodes().FirstOrDefault().ToString();
                parameterType = RemoveNamespace(parameterType);

                //
                if (sb.Length != 0) sb.Append(", ");
                if (isIn) sb.Append("in ");
                if (isOut) sb.Append("out ");
                if (isByRef) sb.Append("ref ");
                if (isOptional) sb.Append("[");
                if (isParamArray) sb.Append("params ");

                sb.Append(parameterType);

                if (isOptional) sb.Append("]");
            }

            return sb.ToString();
        }

        private static string GetComment(SyntaxNode node)
        {
            var leadingTrivia = node.GetLeadingTrivia();
            var trailingTrivia = node.GetTrailingTrivia();

            // XML ドキュメントコメント
            var xmlComment = string.Empty;
            var triviaList = leadingTrivia
                .Where(x => x.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
                .Select(x => x.GetStructure() as DocumentationCommentTriviaSyntax)
                .FirstOrDefault();

            if (triviaList != null)
            {
                var summary = string.Empty;
                var remarks = string.Empty;
                var docCommentNodes = triviaList.DescendantNodes();

                for (var i = 0; i < docCommentNodes.Count(); i++)
                {
                    var docCommentNode = docCommentNodes.ElementAt(i);
                    if (docCommentNode is XmlElementStartTagSyntax)
                    {
                        var tagNode = docCommentNodes.ElementAt(i + 1) as XmlNameSyntax;

                        // summary
                        if (tagNode != null && tagNode.LocalName.ValueText == "summary")
                        {
                            var textNode = docCommentNodes.ElementAt(i + 2) as XmlTextSyntax;
                            var sb = new StringBuilder();
                            foreach (var token in textNode.TextTokens)
                                sb.Append(token.ValueText);

                            sb.Replace(Environment.NewLine, string.Empty);
                            summary = sb.ToString();
                        }

                        // remarks
                        if (tagNode != null && tagNode.LocalName.ValueText == "remarks")
                        {
                            var textNode = docCommentNodes.ElementAt(i + 2) as XmlTextSyntax;
                            var sb = new StringBuilder();
                            foreach (var token in textNode.TextTokens)
                                sb.Append(token.ValueText);

                            sb.Replace(Environment.NewLine, string.Empty);
                            remarks = sb.ToString();
                        }
                    }
                }

                xmlComment = summary;
            }

            // 単一行コメント
            var singleComment = string.Empty;
            var commentList = leadingTrivia.Where(x => x.IsKind(SyntaxKind.SingleLineCommentTrivia));

            if (commentList != null && commentList.Any())
            {
                var sb = new StringBuilder();
                foreach (var comment in commentList)
                {
                    var tmp = comment.ToString().Trim().Substring(2);
                    sb.Append(tmp);
                }
                singleComment = sb.ToString();
            }

            // 行末コメント
            var rearComment = trailingTrivia.FirstOrDefault(x => x.IsKind(SyntaxKind.SingleLineCommentTrivia) || x.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)).ToString();

            if (!string.IsNullOrWhiteSpace(rearComment))
            {
                rearComment = rearComment.Trim().Substring(2);
            }

            // XML ドキュメントコメント、単一行コメント、行末コメント、の優先度で返却します。
            if (!string.IsNullOrWhiteSpace(xmlComment)) return xmlComment;
            if (!string.IsNullOrWhiteSpace(singleComment)) return singleComment;
            if (!string.IsNullOrWhiteSpace(rearComment)) return rearComment;

            return string.Empty;
        }
    }
}
