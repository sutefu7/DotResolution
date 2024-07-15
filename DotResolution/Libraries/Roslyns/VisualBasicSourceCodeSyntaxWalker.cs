using DotResolution.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DotResolution.Libraries.Roslyns
{
    /// <summary>
    /// Visual Basic のソースコードに記載された各定義情報を取得するクラスです。
    /// </summary>
    /// <remarks>
    /// ソースコードを構文木に変換して、Visitor パターンで巡回していきます。
    /// </remarks>
    public class VisualBasicSourceCodeSyntaxWalker : VisualBasicSyntaxWalker, ISourceCodeSyntaxWalker
    {
        private string SourceFile = string.Empty;

        /// <summary>
        /// 各定義情報を取得して作成した定義一覧ツリー を取得、または設定します。
        /// </summary>
        public ObservableCollection<TreeViewItemModel> DefinitionItems { get; set; } = new ObservableCollection<TreeViewItemModel>();

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        public VisualBasicSourceCodeSyntaxWalker() : base(SyntaxWalkerDepth.Trivia)
        {

        }

        /// <summary>
        /// Visual Basic のソースコードを構文木に変換して、構文木の巡回を開始します。
        /// </summary>
        public void Parse(string sourceFile)
        {
            SourceFile = sourceFile;

            var code = RoslynHelper.ReadAllText(sourceFile);
            var tree = VisualBasicSyntaxTree.ParseText(code);
            var root = tree.GetRoot();

            Visit(root);
        }

        public override void VisitClassBlock(ClassBlockSyntax node)
        {
            var statementNode = node.ChildNodes().OfType<ClassStatementSyntax>().FirstOrDefault();
            var defineToken = statementNode.ChildTokens().FirstOrDefault(x => x.IsKind(SyntaxKind.IdentifierToken));
            var defineName = defineToken.ToString();

            // ジェネリック型を定義している場合
            if (statementNode.ChildNodes().OfType<TypeParameterListSyntax>().Any())
            {
                var listNode = statementNode.ChildNodes().OfType<TypeParameterListSyntax>().FirstOrDefault();
                var genericTypes = listNode
                    .ChildNodes()
                    .OfType<TypeParameterSyntax>()
                    .Select(x => x.ChildTokens().FirstOrDefault(y => y.IsKind(SyntaxKind.IdentifierToken)).ToString());

                defineName = $"{defineName}(Of {string.Join(", ", genericTypes)})";
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
                LanguageType = LanguageTypes.VisualBasic,
                IsExpanded = true,
                Comment = GetComment(node),
            };

            var parentModel = FindParentModel(model.StartOffset, model.EndOffset);

            if (parentModel != null)
                parentModel.Children.Add(model);
            else
                DefinitionItems.Add(model);

            //
            base.VisitClassBlock(node);
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
                    case DefinitionTypes.Module:
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

        public override void VisitStructureBlock(StructureBlockSyntax node)
        {
            var statementNode = node.ChildNodes().OfType<StructureStatementSyntax>().FirstOrDefault();
            var defineToken = statementNode.ChildTokens().FirstOrDefault(x => x.IsKind(SyntaxKind.IdentifierToken));
            var defineName = defineToken.ToString();

            // ジェネリック型を定義している場合
            if (statementNode.ChildNodes().OfType<TypeParameterListSyntax>().Any())
            {
                var listNode = statementNode.ChildNodes().OfType<TypeParameterListSyntax>().FirstOrDefault();
                var genericTypes = listNode
                    .ChildNodes()
                    .OfType<TypeParameterSyntax>()
                    .Select(x => x.ChildTokens().FirstOrDefault(y => y.IsKind(SyntaxKind.IdentifierToken)).ToString());

                defineName = $"{defineName}(Of {string.Join(", ", genericTypes)})";
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
                LanguageType = LanguageTypes.VisualBasic,
                IsExpanded = true,
                Comment = GetComment(node),
            };

            var parentModel = FindParentModel(model.StartOffset, model.EndOffset);

            if (parentModel != null)
                parentModel.Children.Add(model);
            else
                DefinitionItems.Add(model);

            //
            base.VisitStructureBlock(node);
        }

        public override void VisitInterfaceBlock(InterfaceBlockSyntax node)
        {
            var statementNode = node.ChildNodes().OfType<InterfaceStatementSyntax>().FirstOrDefault();
            var defineToken = statementNode.ChildTokens().FirstOrDefault(x => x.IsKind(SyntaxKind.IdentifierToken));
            var defineName = defineToken.ToString();

            // ジェネリック型を定義している場合
            if (statementNode.ChildNodes().OfType<TypeParameterListSyntax>().Any())
            {
                var listNode = statementNode.ChildNodes().OfType<TypeParameterListSyntax>().FirstOrDefault();
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
                LanguageType = LanguageTypes.VisualBasic,
                IsExpanded = true,
                Comment = GetComment(node),
            };

            var parentModel = FindParentModel(model.StartOffset, model.EndOffset);

            if (parentModel != null)
                parentModel.Children.Add(model);
            else
                DefinitionItems.Add(model);

            //
            base.VisitInterfaceBlock(node);
        }

        public override void VisitModuleBlock(ModuleBlockSyntax node)
        {
            var statementNode = node.ChildNodes().OfType<ModuleStatementSyntax>().FirstOrDefault();
            var defineToken = statementNode.ChildTokens().FirstOrDefault(x => x.IsKind(SyntaxKind.IdentifierToken));
            var defineName = defineToken.ToString();

            // 登録
            var model = new TreeViewItemModel
            {
                Tag = node,
                Text = defineName,
                DefinitionType = DefinitionTypes.Module,

                TargetFile = SourceFile,
                StartOffset = node.Span.Start,
                EndOffset = node.Span.End,
                IdentifierTokenStartOffset = defineToken.Span.Start,
                LanguageType = LanguageTypes.VisualBasic,
                IsExpanded = true,
                Comment = GetComment(node),
            };

            var parentModel = FindParentModel(model.StartOffset, model.EndOffset);

            if (parentModel != null)
                parentModel.Children.Add(model);
            else
                DefinitionItems.Add(model);

            //
            base.VisitModuleBlock(node);
        }

        public override void VisitEnumBlock(EnumBlockSyntax node)
        {
            var statementNode = node.ChildNodes().OfType<EnumStatementSyntax>().FirstOrDefault();
            var defineName = statementNode.ChildTokens().FirstOrDefault(x => x.IsKind(SyntaxKind.IdentifierToken)).ToString();

            // 登録
            var model = new TreeViewItemModel
            {
                Tag = node,
                Text = defineName,
                DefinitionType = DefinitionTypes.Enum,

                TargetFile = SourceFile,
                StartOffset = node.Span.Start,
                EndOffset = node.Span.End,
                LanguageType = LanguageTypes.VisualBasic,
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
                    LanguageType = LanguageTypes.VisualBasic,
                    Comment = GetComment(node),
                };
                model.Children.Add(childModel);
            }

            //
            base.VisitEnumBlock(node);
        }

        public override void VisitConstructorBlock(ConstructorBlockSyntax node)
        {
            var statementNode = node.ChildNodes().OfType<SubNewStatementSyntax>().FirstOrDefault();
            var defineName = statementNode.ChildTokens().FirstOrDefault(x => x.IsKind(SyntaxKind.NewKeyword)).ToString(); // New() 固定文字列でもいいか？
            var methodArguments = string.Empty;

            if (statementNode.ChildNodes().OfType<ParameterListSyntax>().FirstOrDefault().ChildNodes().Any())
            {
                var listNode = statementNode.ChildNodes().OfType<ParameterListSyntax>().FirstOrDefault();
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
                LanguageType = LanguageTypes.VisualBasic,
                Comment = GetComment(node),
            };

            var parentModel = FindParentModel(model.StartOffset, model.EndOffset);

            if (parentModel != null)
                parentModel.Children.Add(model);
            else
                DefinitionItems.Add(model);

            //
            base.VisitConstructorBlock(node);
        }

        public override void VisitOperatorBlock(OperatorBlockSyntax node)
        {
            var statementNode = node.ChildNodes().OfType<OperatorStatementSyntax>().FirstOrDefault();
            var defineName = statementNode.ChildTokens().LastOrDefault().ToString();
            var methodArguments = string.Empty;

            if (statementNode.ChildNodes().OfType<ParameterListSyntax>().FirstOrDefault().ChildNodes().Any())
            {
                var listNode = statementNode.ChildNodes().OfType<ParameterListSyntax>().FirstOrDefault();
                methodArguments = GetMethodArguments(listNode);
            }

            var asNode = statementNode.ChildNodes().FirstOrDefault(x => x is SimpleAsClauseSyntax);
            var returnType = asNode.ChildNodes().FirstOrDefault().ToString();

            methodArguments = $"({methodArguments})";
            defineName = $"{defineName}{methodArguments}";

            if (returnType.ToLower() != "void")
                defineName = $"{defineName} As {returnType}";

            // 登録
            var model = new TreeViewItemModel
            {
                Tag = node,
                Text = defineName,
                DefinitionType = DefinitionTypes.Operator,

                TargetFile = SourceFile,
                StartOffset = node.Span.Start,
                EndOffset = node.Span.End,
                LanguageType = LanguageTypes.VisualBasic,
                Comment = GetComment(node),
            };

            var parentModel = FindParentModel(model.StartOffset, model.EndOffset);

            if (parentModel != null)
                parentModel.Children.Add(model);
            else
                DefinitionItems.Add(model);

            //
            base.VisitOperatorBlock(node);
        }

        // SubBlock, FunctionBlock が含まれているか？
        public override void VisitMethodBlock(MethodBlockSyntax node)
        {
            var statementNode = node.ChildNodes().OfType<MethodStatementSyntax>().FirstOrDefault();
            var isSubMethod = statementNode.ChildTokens().Any(x => x.IsKind(SyntaxKind.SubKeyword));
            var defineName = statementNode.ChildTokens().FirstOrDefault(x => x.IsKind(SyntaxKind.IdentifierToken)).ToString();

            // ジェネリック型を定義している場合
            if (statementNode.ChildNodes().OfType<TypeParameterListSyntax>().Any())
            {
                var listNode = statementNode.ChildNodes().OfType<TypeParameterListSyntax>().FirstOrDefault();
                var genericTypes = listNode
                    .ChildNodes()
                    .OfType<TypeParameterSyntax>()
                    .Select(x => x.ChildTokens().FirstOrDefault(y => y.IsKind(SyntaxKind.IdentifierToken)).ToString());

                defineName = $"{defineName}(Of {string.Join(", ", genericTypes)})";
            }

            // 引数
            var methodArguments = string.Empty;
            var isEventHandler = false;

            if (statementNode.ChildNodes().OfType<ParameterListSyntax>().FirstOrDefault().ChildNodes().Any())
            {
                var listNode = statementNode.ChildNodes().OfType<ParameterListSyntax>().FirstOrDefault();
                methodArguments = GetMethodArguments(listNode);

                isEventHandler = IsEventHandlerPatternDefinition(listNode);
            }

            // 戻り値
            var returnType = "Void";
            if (!isSubMethod)
            {
                var asNode = statementNode.ChildNodes().FirstOrDefault(x => x is SimpleAsClauseSyntax);
                returnType = asNode.ChildNodes().FirstOrDefault().ToString();
            }

            methodArguments = $"({methodArguments})";
            defineName = $"{defineName}{methodArguments}";

            if (returnType.ToLower() != "void")
                defineName = $"{defineName} As {returnType}";

            // メソッドの種類
            var definitionType = DefinitionTypes.Method;

            isEventHandler = (isSubMethod && isEventHandler);
            if (isEventHandler)
                definitionType = DefinitionTypes.EventHandler;

            // 登録
            var model = new TreeViewItemModel
            {
                Tag = node,
                Text = defineName,
                DefinitionType = definitionType,

                TargetFile = SourceFile,
                StartOffset = node.Span.Start,
                EndOffset = node.Span.End,
                LanguageType = LanguageTypes.VisualBasic,
                Comment = GetComment(node),
            };

            var parentModel = FindParentModel(model.StartOffset, model.EndOffset);

            if (parentModel != null)
                parentModel.Children.Add(model);
            else
                DefinitionItems.Add(model);

            //
            base.VisitMethodBlock(node);
        }

        // Interface のメソッドの時とか
        // Windows API 系(DllImport)
        public override void VisitMethodStatement(MethodStatementSyntax statementNode)
        {
            // VisitMethodBlock() から来た場合は、二重登録になってしまうので飛ばす
            if (statementNode.Parent is MethodBlockSyntax)
            {
                base.VisitMethodStatement(statementNode);
                return;
            }

            var isWinAPI = statementNode.AttributeLists.Any(x => x.ToString().Contains("DllImport"));
            var isSubMethod = statementNode.ChildTokens().Any(x => x.IsKind(SyntaxKind.SubKeyword));
            var defineName = statementNode.ChildTokens().FirstOrDefault(x => x.IsKind(SyntaxKind.IdentifierToken)).ToString();

            // ジェネリック型を定義している場合
            if (statementNode.ChildNodes().OfType<TypeParameterListSyntax>().Any())
            {
                var listNode = statementNode.ChildNodes().OfType<TypeParameterListSyntax>().FirstOrDefault();
                var genericTypes = listNode
                    .ChildNodes()
                    .OfType<TypeParameterSyntax>()
                    .Select(x => x.ChildTokens().FirstOrDefault(y => y.IsKind(SyntaxKind.IdentifierToken)).ToString());

                defineName = $"{defineName}(Of {string.Join(", ", genericTypes)})";
            }

            // 引数
            var methodArguments = string.Empty;
            if (statementNode.ChildNodes().OfType<ParameterListSyntax>().FirstOrDefault().ChildNodes().Any())
            {
                var listNode = statementNode.ChildNodes().OfType<ParameterListSyntax>().FirstOrDefault();
                methodArguments = GetMethodArguments(listNode);
            }

            // 戻り値
            var returnType = "Void";
            if (!isSubMethod)
            {
                var asNode = statementNode.ChildNodes().FirstOrDefault(x => x is SimpleAsClauseSyntax);
                returnType = asNode.ChildNodes().FirstOrDefault().ToString();
            }

            methodArguments = $"({methodArguments})";
            defineName = $"{defineName}{methodArguments}";

            if (returnType.ToLower() != "void")
                defineName = $"{defineName} As {returnType}";

            // メソッドの種類
            var definitionType = DefinitionTypes.Method;

            if (isWinAPI)
                definitionType = DefinitionTypes.WindowsAPI;

            // 登録
            var model = new TreeViewItemModel
            {
                Tag = statementNode,
                Text = defineName,
                DefinitionType = definitionType,

                TargetFile = SourceFile,
                StartOffset = statementNode.Span.Start,
                EndOffset = statementNode.Span.End,
                LanguageType = LanguageTypes.VisualBasic,
                Comment = GetComment(statementNode),
            };

            var parentModel = FindParentModel(model.StartOffset, model.EndOffset);

            if (parentModel != null)
                parentModel.Children.Add(model);
            else
                DefinitionItems.Add(model);

            //
            base.VisitMethodStatement(statementNode);
        }

        // Windows API 系(Declare)
        public override void VisitDeclareStatement(DeclareStatementSyntax statementNode)
        {
            var isSubMethod = statementNode.ChildTokens().Any(x => x.IsKind(SyntaxKind.SubKeyword));
            var defineName = statementNode.ChildTokens().FirstOrDefault(x => x.IsKind(SyntaxKind.IdentifierToken)).ToString();

            // 引数
            var methodArguments = string.Empty;
            if (statementNode.ChildNodes().OfType<ParameterListSyntax>().FirstOrDefault().ChildNodes().Any())
            {
                var listNode = statementNode.ChildNodes().OfType<ParameterListSyntax>().FirstOrDefault();
                methodArguments = GetMethodArguments(listNode);
            }

            // 戻り値
            var returnType = "Void";
            if (!isSubMethod)
            {
                var asNode = statementNode.ChildNodes().FirstOrDefault(x => x is SimpleAsClauseSyntax);
                returnType = asNode.ChildNodes().FirstOrDefault().ToString();
            }

            methodArguments = $"({methodArguments})";
            defineName = $"{defineName}{methodArguments}";

            if (returnType.ToLower() != "void")
                defineName = $"{defineName} As {returnType}";

            // 登録
            var model = new TreeViewItemModel
            {
                Tag = statementNode,
                Text = defineName,
                DefinitionType = DefinitionTypes.WindowsAPI,

                TargetFile = SourceFile,
                StartOffset = statementNode.Span.Start,
                EndOffset = statementNode.Span.End,
                LanguageType = LanguageTypes.VisualBasic,
                Comment = GetComment(statementNode),
            };

            var parentModel = FindParentModel(model.StartOffset, model.EndOffset);

            if (parentModel != null)
                parentModel.Children.Add(model);
            else
                DefinitionItems.Add(model);

            //
            base.VisitDeclareStatement(statementNode);
        }

        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            var variables = node.ChildNodes().OfType<VariableDeclaratorSyntax>();

            foreach (var variable in variables)
            {
                // Public i1, i2 As Integer
                // Public i3() As Integer, i4 As Integer

                // 先に型を取得
                var asNode = variable.ChildNodes().FirstOrDefault(x => x is SimpleAsClauseSyntax);
                var variableType = asNode.ChildNodes().FirstOrDefault().ToString();

                // フィールド名を取得
                var fields = variable.ChildNodes().OfType<ModifiedIdentifierSyntax>();

                foreach (var field in fields)
                {
                    var startOffset = field.Span.Start;
                    var endOffset = field.Span.End;

                    var fieldName = field.ChildTokens().FirstOrDefault(x => x.IsKind(SyntaxKind.IdentifierToken)).ToString();
                    var fieldType = variableType;

                    var hasArrayRank = field.ChildNodes().Any();
                    if (hasArrayRank)
                    {
                        var arrayRank = field.ChildNodes().FirstOrDefault().ToString();
                        fieldType = $"{fieldType}{arrayRank}";
                    }

                    fieldName = $"{fieldName} As {fieldType}";

                    // 登録
                    var model = new TreeViewItemModel
                    {
                        Tag = field,
                        Text = fieldName,
                        DefinitionType = DefinitionTypes.Field,

                        TargetFile = SourceFile,
                        StartOffset = startOffset,
                        EndOffset = endOffset,
                        LanguageType = LanguageTypes.VisualBasic,
                        Comment = GetComment(node),
                    };

                    var parentModel = FindParentModel(model.StartOffset, model.EndOffset);

                    if (parentModel != null)
                        parentModel.Children.Add(model);
                    else
                        DefinitionItems.Add(model);
                }
            }

            //
            base.VisitFieldDeclaration(node);
        }

        public override void VisitPropertyBlock(PropertyBlockSyntax node)
        {
            var startLength = node.Span.Start;
            var endLength = node.Span.End;
            var statementNode = node.ChildNodes().OfType<PropertyStatementSyntax>().FirstOrDefault();
            WalkPropertyBlockOrPropertyStatement(statementNode, startLength, endLength);

            base.VisitPropertyBlock(node);
        }

        // 自動実装プロパティとか
        public override void VisitPropertyStatement(PropertyStatementSyntax node)
        {
            // VisitPropertyBlock() から来た場合は、二重登録になってしまうので飛ばす
            if (node.Parent is PropertyBlockSyntax)
            {
                base.VisitPropertyStatement(node);
                return;
            }

            var startLength = node.Span.Start;
            var endLength = node.Span.End;
            WalkPropertyBlockOrPropertyStatement(node, startLength, endLength);

            base.VisitPropertyStatement(node);
        }

        private void WalkPropertyBlockOrPropertyStatement(PropertyStatementSyntax node, int startLength, int endLength)
        {
            var isDefault = node.ChildTokens().Any(x => x.IsKind(SyntaxKind.DefaultKeyword));
            var defineName = node.ChildTokens().FirstOrDefault(x => x.IsKind(SyntaxKind.IdentifierToken)).ToString();
            var asNode = node.ChildNodes().OfType<SimpleAsClauseSyntax>().FirstOrDefault();
            var defineType = asNode.ChildNodes().FirstOrDefault().ToString();

            // Property Name As String の場合、カッコが無い → ParameterListSyntax タグがない
            // Property Name() As String の場合、カッコがある → ParameterListSyntax タグがある（個数0）
            // Default Property Item(i As Index) As String の場合、カッコがある → ParameterListSyntax タグがある（個数1以上）
            var methodArguments = string.Empty;
            if (node.ChildNodes().OfType<ParameterListSyntax>().Any())
            {
                if (node.ChildNodes().OfType<ParameterListSyntax>().FirstOrDefault().ChildNodes().Any())
                {
                    var listNode = node.ChildNodes().OfType<ParameterListSyntax>().FirstOrDefault();
                    methodArguments = GetMethodArguments(listNode);
                }
            }

            var definitionType = DefinitionTypes.Property;
            if (string.IsNullOrEmpty(methodArguments))
            {
                defineName = $"{defineName} As {defineType}";
            }
            else
            {
                definitionType = DefinitionTypes.Indexer;

                methodArguments = $"({methodArguments})";
                defineName = $"Default {defineName}{methodArguments} As {defineType}";
            }

            // 登録
            var model = new TreeViewItemModel
            {
                Tag = node,
                Text = defineName,
                DefinitionType = definitionType,

                TargetFile = SourceFile,
                StartOffset = startLength,
                EndOffset = endLength,
                LanguageType = LanguageTypes.VisualBasic,
                Comment = GetComment(node),
            };

            var parentModel = FindParentModel(model.StartOffset, model.EndOffset);

            if (parentModel != null)
                parentModel.Children.Add(model);
            else
                DefinitionItems.Add(model);
        }

        // カスタムイベント定義
        public override void VisitEventBlock(EventBlockSyntax node)
        {
            var startLength = node.Span.Start;
            var endLength = node.Span.End;
            var statementNode = node.ChildNodes().OfType<EventStatementSyntax>().FirstOrDefault();
            WalkEventBlockOrEventStatement(statementNode, startLength, endLength);

            base.VisitEventBlock(node);
        }

        public override void VisitEventStatement(EventStatementSyntax node)
        {
            // VisitEventBlock() から来た場合は、二重登録になってしまうので飛ばす
            if (node.Parent is EventBlockSyntax)
            {
                base.VisitEventStatement(node);
                return;
            }

            var startLength = node.Span.Start;
            var endLength = node.Span.End;
            WalkEventBlockOrEventStatement(node, startLength, endLength);

            base.VisitEventStatement(node);
        }

        private void WalkEventBlockOrEventStatement(EventStatementSyntax node, int startLength, int endLength)
        {
            var defineName = node.ChildTokens().FirstOrDefault(x => x.IsKind(SyntaxKind.IdentifierToken)).ToString();
            var defineType = "Void";

            // Public Event Clicked As EventHandler 系の宣言の場合
            if (node.ChildNodes().OfType<SimpleAsClauseSyntax>().Any())
            {
                var asNode = node.ChildNodes().OfType<SimpleAsClauseSyntax>().FirstOrDefault();
                defineType = asNode.ChildNodes().FirstOrDefault().ToString();
            }

            // Public Event Moved(sender As Object, e As EventArgs) 系の宣言の場合
            var methodArguments = string.Empty;
            if (node.ChildNodes().OfType<ParameterListSyntax>().Any())
            {
                if (node.ChildNodes().OfType<ParameterListSyntax>().FirstOrDefault().ChildNodes().Any())
                {
                    var listNode = node.ChildNodes().OfType<ParameterListSyntax>().FirstOrDefault();
                    methodArguments = GetMethodArguments(listNode);
                }
            }

            if (string.IsNullOrEmpty(methodArguments))
            {
                methodArguments = $"({methodArguments})";
                defineName = $"{defineName}{methodArguments}";
            }

            if (defineType.ToLower() != "void")
                defineName = $"{defineName} As {defineType}";

            // 登録
            var model = new TreeViewItemModel
            {
                Tag = node,
                Text = defineName,
                DefinitionType = DefinitionTypes.Event,

                TargetFile = SourceFile,
                StartOffset = startLength,
                EndOffset = endLength,
                LanguageType = LanguageTypes.VisualBasic,
                Comment = GetComment(node),
            };

            var parentModel = FindParentModel(model.StartOffset, model.EndOffset);

            if (parentModel != null)
                parentModel.Children.Add(model);
            else
                DefinitionItems.Add(model);
        }

        public override void VisitDelegateStatement(DelegateStatementSyntax statementNode)
        {
            var isSubMethod = statementNode.ChildTokens().Any(x => x.IsKind(SyntaxKind.SubKeyword));
            var defineName = statementNode.ChildTokens().FirstOrDefault(x => x.IsKind(SyntaxKind.IdentifierToken)).ToString();

            // ジェネリック型を定義している場合
            if (statementNode.ChildNodes().OfType<TypeParameterListSyntax>().Any())
            {
                var listNode = statementNode.ChildNodes().OfType<TypeParameterListSyntax>().FirstOrDefault();
                var genericTypes = listNode
                    .ChildNodes()
                    .OfType<TypeParameterSyntax>()
                    .Select(x => x.ChildTokens().FirstOrDefault(y => y.IsKind(SyntaxKind.IdentifierToken)).ToString());

                defineName = $"{defineName}(Of {string.Join(", ", genericTypes)})";
            }

            // 引数
            var methodArguments = string.Empty;
            if (statementNode.ChildNodes().OfType<ParameterListSyntax>().FirstOrDefault().ChildNodes().Any())
            {
                var listNode = statementNode.ChildNodes().OfType<ParameterListSyntax>().FirstOrDefault();
                methodArguments = GetMethodArguments(listNode);
            }

            // 戻り値
            var returnType = "Void";
            if (!isSubMethod)
            {
                var asNode = statementNode.ChildNodes().FirstOrDefault(x => x is SimpleAsClauseSyntax);
                returnType = asNode.ChildNodes().FirstOrDefault().ToString();
            }

            methodArguments = $"({methodArguments})";
            defineName = $"{defineName}{methodArguments}";

            if (returnType.ToLower() != "void")
                defineName = $"{defineName} As {returnType}";

            // 登録
            var model = new TreeViewItemModel
            {
                Tag = statementNode,
                Text = defineName,
                DefinitionType = DefinitionTypes.Delegate,

                TargetFile = SourceFile,
                StartOffset = statementNode.Span.Start,
                EndOffset = statementNode.Span.End,
                LanguageType = LanguageTypes.VisualBasic,
                Comment = GetComment(statementNode),
            };

            var parentModel = FindParentModel(model.StartOffset, model.EndOffset);

            if (parentModel != null)
                parentModel.Children.Add(model);
            else
                DefinitionItems.Add(model);

            //
            base.VisitDelegateStatement(statementNode);
        }

        private string GetMethodArguments(ParameterListSyntax node)
        {
            var sb = new StringBuilder();
            var parameters = node.ChildNodes().OfType<ParameterSyntax>();

            foreach (var parameter in parameters)
            {
                //
                var isByRef = parameter.ChildTokens().Any(x => x.IsKind(SyntaxKind.ByRefKeyword));
                var isByVal = !isByRef;
                var isOptional = parameter.ChildTokens().Any(x => x.IsKind(SyntaxKind.OptionalKeyword));
                var isParamArray = parameter.ChildTokens().Any(x => x.IsKind(SyntaxKind.ParamArrayKeyword));

                var modified = parameter.ChildNodes().FirstOrDefault(x => x is ModifiedIdentifierSyntax);
                var parameterName = modified.ChildTokens().FirstOrDefault(x => x.IsKind(SyntaxKind.IdentifierToken)).ToString();

                var asNode = parameter.ChildNodes().FirstOrDefault(x => x is SimpleAsClauseSyntax);
                var parameterType = asNode.ChildNodes().FirstOrDefault().ToString();
                parameterType = RemoveNamespace(parameterType);

                var hasArrayRank = modified.ChildNodes().Any();
                if (hasArrayRank)
                {
                    var arrayRank = modified.ChildNodes().FirstOrDefault().ToString();
                    parameterType = $"{parameterType}{arrayRank}";
                }

                //
                if (sb.Length != 0) sb.Append(", ");
                if (isByRef) sb.Append("ByRef ");
                if (isParamArray) sb.Append("ParamArray ");
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
                    var asNode = parameter.ChildNodes().FirstOrDefault(x => x is SimpleAsClauseSyntax);
                    var parameterType = asNode.ChildNodes().FirstOrDefault().ToString();

                    var modified = parameter.ChildNodes().FirstOrDefault(x => x is ModifiedIdentifierSyntax);
                    var hasArrayRank = modified.ChildNodes().Any();
                    if (hasArrayRank)
                    {
                        var arrayRank = modified.ChildNodes().FirstOrDefault().ToString();
                        parameterType = $"{parameterType}{arrayRank}";
                    }
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

                if (firstType == "Object" && secondType.EndsWith("EventArgs"))
                    return true;
            }

            return false;
        }

        private static string GetComment(SyntaxNode node)
        {
            var leadingTrivia = node.GetLeadingTrivia();
            var trailingTrivia = node.GetTrailingTrivia();

            // XML ドキュメントコメント
            var xmlComment = string.Empty;
            var triviaList = leadingTrivia
                .Where(x => x.IsKind(SyntaxKind.DocumentationCommentTrivia))
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
            var commentList = leadingTrivia.Where(x => x.IsKind(SyntaxKind.CommentTrivia));

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
            var rearComment = trailingTrivia.FirstOrDefault(x => x.IsKind(SyntaxKind.CommentTrivia) || x.IsKind(SyntaxKind.DocumentationCommentTrivia)).ToString();

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
