using DotResolution.Data;
using Hnx8.ReadJEnc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DotResolution.Libraries.Roslyns
{
    /// <summary>
    /// Roslyn に関するヘルパークラスです。
    /// </summary>
    public class RoslynHelper
    {
        #region ソリューションファイルから TreeNode 作成

        /// <summary>
        /// 指定のソリューションファイルをもとに、ツリー用のバインドデータコレクションを作成・返却します。
        /// </summary>
        /// <param name="solutionFile"></param>
        /// <returns></returns>
        public static async Task<TreeViewItemModel> CreateSolutionExplorerTreeAsync(string solutionFile)
        {
            // 全てのソースコードをもとに、構文木とコンパイラを作成
            var workspace = MSBuildWorkspace.Create();
            workspace.LoadMetadataForReferencedProjects = true;

            var solution = await workspace.OpenSolutionAsync(solutionFile);
            await CorrectAnalysisData(solution);
            return CreateSolutionTree(solution);
        }

        // ソースコードをもとに、構文木・コンパイラの作成
        private static async Task CorrectAnalysisData(Solution solution)
        {
            foreach (var project in solution.Projects)
            {
                if (GetLanguageTypes(project) == LanguageTypes.CSharp)
                {
                    foreach (var document in project.Documents)
                    {
                        var tree = await document.GetSyntaxTreeAsync();
                        AppEnv.CSharpSyntaxTrees.Add(tree);
                    }

                    var references = new List<PortableExecutableReference>
                    {
                        // microlib.dll
                        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                        // System.dll
                        MetadataReference.CreateFromFile(typeof(ObservableCollection<>).Assembly.Location),
                        // System.Core.dll
                        MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                        // External library
                        // MetadataReference.CreateFromFile("library path"),
                    };

                    var compilation = CSharpCompilation.Create(project.AssemblyName, AppEnv.CSharpSyntaxTrees, references);
                    AppEnv.CSharpCompilations.Add(compilation);
                }

                if (GetLanguageTypes(project) == LanguageTypes.VisualBasic)
                {
                    foreach (var document in project.Documents)
                    {
                        var tree = await document.GetSyntaxTreeAsync();
                        AppEnv.VisualBasicSyntaxTrees.Add(tree);
                    }

                    var references = new List<PortableExecutableReference>
                    {
                        // microlib.dll
                        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                        // System.dll
                        MetadataReference.CreateFromFile(typeof(ObservableCollection<>).Assembly.Location),
                        // System.Core.dll
                        MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                        // External library
                        // MetadataReference.CreateFromFile("library path"),
                    };

                    var kinds = Path.GetExtension(project.OutputFilePath).ToLower() == ".exe" ? OutputKind.ConsoleApplication : OutputKind.DynamicallyLinkedLibrary;
                    var options = new VisualBasicCompilationOptions(outputKind: kinds, rootNamespace: project.DefaultNamespace);
                    var compilation = VisualBasicCompilation.Create(project.AssemblyName, AppEnv.VisualBasicSyntaxTrees, references, options);
                    AppEnv.VisualBasicCompilations.Add(compilation);
                }
            }
        }

        private static LanguageTypes GetLanguageTypes(Project project)
        {
            if (project.Language == "C#") return LanguageTypes.CSharp;
            if (project.Language == "Visual Basic") return LanguageTypes.VisualBasic;

            return LanguageTypes.CSharp;
        }

        // ソリューションファイルツリーを作成
        private static TreeViewItemModel CreateSolutionTree(Solution solution)
        {
            // ソリューションファイル
            var solutionModel = new TreeViewItemModel
            {
                Tag = solution,
                Text = Path.GetFileNameWithoutExtension(solution.FilePath),
                TargetFile = solution.FilePath,
                DefinitionType = DefinitionTypes.SolutionFile,
                IsExpanded = true,
            };

            foreach (var project in solution.Projects)
            {
                var projectModel = CreateProjectTree(solution, project);
                solutionModel.Children.Add(projectModel);
            }

            return solutionModel;
        }

        // プロジェクトファイルツリーを作成
        private static TreeViewItemModel CreateProjectTree(Solution solution, Project project)
        {
            // プロジェクトファイル
            var langType =
                (GetLanguageTypes(project) == LanguageTypes.CSharp) ? DefinitionTypes.CSharpProjectFile :
                (GetLanguageTypes(project) == LanguageTypes.VisualBasic) ? DefinitionTypes.VisualBasicProjectFile :
                DefinitionTypes.Unknown;

            var projectModel = new TreeViewItemModel
            {
                Tag = project,
                Text = project.Name,
                TargetFile = project.FilePath,
                DefinitionType = langType,
                LanguageType = GetLanguageTypes(project),
                IsExpanded = true,
            };

            // 参照 dll
            var allRefDlls = project.MetadataReferences;
            var refDlls = allRefDlls.Where(x => x.Display != null && !x.Display.Contains(@"\packages\"));

            if (refDlls.Any())
            {
                var headerModel = new TreeViewItemModel
                {
                    Text = "参照",
                    DefinitionType = DefinitionTypes.Dependency,
                };
                projectModel.Children.Add(headerModel);

                foreach (var refDll in refDlls)
                {
                    var memberModel = new TreeViewItemModel
                    {
                        Text = Path.GetFileNameWithoutExtension(refDll.Display),
                        DefinitionType = DefinitionTypes.Dependency,
                    };
                    headerModel.Children.Add(memberModel);
                }
            }

            // NuGet 参照 dll
            var nugetDlls = allRefDlls.Where(x => x.Display != null && x.Display.Contains(@"\packages\"));

            if (nugetDlls.Any())
            {
                var headerModel = new TreeViewItemModel
                {
                    Text = "NuGet 参照",
                    DefinitionType = DefinitionTypes.Dependency,
                };
                projectModel.Children.Add(headerModel);

                foreach (var nugetDll in nugetDlls)
                {
                    var memberModel = new TreeViewItemModel
                    {
                        Text = Path.GetFileNameWithoutExtension(nugetDll.Display),
                        DefinitionType = DefinitionTypes.Dependency,
                    };
                    headerModel.Children.Add(memberModel);
                }
            }

            // プロジェクト参照 dll
            var projectDlls = project.ProjectReferences;

            if (projectDlls.Any())
            {
                var headerModel = new TreeViewItemModel
                {
                    Text = "プロジェクト参照",
                    DefinitionType = DefinitionTypes.Dependency,
                };
                projectModel.Children.Add(headerModel);

                foreach (var projectDll in projectDlls)
                {
                    var found = solution.Projects.First(x => x.Id == projectDll.ProjectId);
                    var memberModel = new TreeViewItemModel
                    {
                        Text = Path.GetFileNameWithoutExtension(found.Name),
                        DefinitionType = DefinitionTypes.Dependency,
                    };
                    headerModel.Children.Add(memberModel);
                }
            }

            // ソースファイル
            foreach (var document in project.Documents)
            {
                // 自動生成系は表示しない
                // WPF/WinForms
                // ～\WindowsFormsApp1\obj\Debug\.NETFramework,Version=v4.8.AssemblyAttributes.cs
                if (document.FilePath.Contains($@"\{project.Name}\obj\"))
                    continue;

                // WinForms, デザイナーファイルは動的に判断して追加するので、ここでは飛ばす
                if (document.FilePath.Contains(".Designer."))
                    continue;

                //
                var sourceModel = CreateSourceTree(solution, project, document);

                if (IsSameDirectory(project, document))
                {
                    projectModel.Children.Add(sourceModel);
                }
                else
                {
                    var parentModel = CreateSubFolders(project, document, projectModel);
                    parentModel.Children.Add(sourceModel);
                }
            }

            return projectModel;
        }

        // ソースファイルツリーを作成
        private static TreeViewItemModel CreateSourceTree(Solution solution, Project project, Document document)
        {
            var headerType =
                (GetLanguageTypes(project) == LanguageTypes.CSharp) ? DefinitionTypes.CSharpSourceFileForHeader :
                (GetLanguageTypes(project) == LanguageTypes.VisualBasic) ? DefinitionTypes.VisualBasicSourceFileForHeader :
                DefinitionTypes.Unknown;

            var langType =
                (GetLanguageTypes(project) == LanguageTypes.CSharp) ? DefinitionTypes.CSharpSourceFile :
                (GetLanguageTypes(project) == LanguageTypes.VisualBasic) ? DefinitionTypes.VisualBasicSourceFile :
                DefinitionTypes.Unknown;

            var headerModel = new TreeViewItemModel
            {
                Tag = document,
                Text = Path.GetFileName(document.FilePath),
                TargetFile = document.FilePath,
                DefinitionType = langType,
                LanguageType = GetLanguageTypes(project),
            };

            // WinForms, このソースファイルが Control 系クラスの場合、同じフォルダ内に xxx.Designer.cs/vb ファイルがあるはず
            var designerFile = Path.GetDirectoryName(document.FilePath);
            var designerName = Path.GetFileNameWithoutExtension(document.FilePath);
            var extension = Path.GetExtension(document.FilePath);
            designerFile = Path.Combine(designerFile, $"{designerName}.Designer{extension}");

            if (project.Documents.Any(x => x.FilePath == designerFile))
            {
                headerModel.DefinitionType = headerType;

                // ソースファイル
                var srcModel = new TreeViewItemModel
                {
                    Tag = document,
                    Text = Path.GetFileName(document.FilePath),
                    TargetFile = document.FilePath,
                    DefinitionType = langType,
                    LanguageType = GetLanguageTypes(project),
                };
                headerModel.Children.Add(srcModel);

                // デザイナーファイル（xxx.Designer.cs/vb）
                var designer = project.Documents.First(x => x.FilePath == designerFile);
                var designerModel = new TreeViewItemModel
                {
                    Tag = designer,
                    Text = Path.GetFileName(designer.FilePath),
                    TargetFile = designer.FilePath,
                    DefinitionType = DefinitionTypes.GeneratedFile,
                    LanguageType = GetLanguageTypes(project),
                };
                headerModel.Children.Add(designerModel);
            }

            // WPF, このソースファイルが xxx.xaml.cs/vb という名称の場合、xxx.xaml と対なのでこちらも含める
            // Roslyn では cs/vb ソースしか認識しないため？、自前で用意する
            if (document.FilePath.Contains(".xaml."))
            {
                designerFile = Path.GetDirectoryName(document.FilePath);
                designerName = Path.GetFileNameWithoutExtension(document.FilePath);
                designerFile = Path.Combine(designerFile, designerName);

                // デザイナーファイル（xaml）
                headerModel.Text = Path.GetFileName(designerFile);
                headerModel.TargetFile = designerFile;
                headerModel.DefinitionType = headerType;

                var designerModel = new TreeViewItemModel
                {
                    Text = Path.GetFileName(designerFile),
                    TargetFile = designerFile,
                    DefinitionType = DefinitionTypes.Unknown,
                };
                headerModel.Children.Add(designerModel);

                // ソースファイル
                var srcModel = new TreeViewItemModel
                {
                    Tag = document,
                    Text = Path.GetFileName(document.FilePath),
                    TargetFile = document.FilePath,
                    DefinitionType = langType,
                    LanguageType = GetLanguageTypes(project),
                };
                headerModel.Children.Add(srcModel);
            }

            return headerModel;
        }

        // ソースファイルがプロジェクトファイルと同じフォルダにあるかどうか（＝サブフォルダ作成有無チェック）
        private static bool IsSameDirectory(Project project, Document document)
        {
            var projectDirectory = Path.GetDirectoryName(project.FilePath);
            var sourceDirectory = Path.GetDirectoryName(document.FilePath);

            // Views
            // Views\Controls\
            // \Controls\
            var difference = sourceDirectory.Replace(projectDirectory, string.Empty);
            return string.IsNullOrWhiteSpace(difference);
        }

        // サブフォルダの再帰的作成と、親となるツリーの返却
        private static TreeViewItemModel CreateSubFolders(Project project, Document document, TreeViewItemModel projectModel)
        {
            var projectDirectory = Path.GetDirectoryName(project.FilePath);
            var sourceDirectory = Path.GetDirectoryName(document.FilePath);

            // Views
            // Views\aaa\bbb\
            // aaa, bbb
            var difference = sourceDirectory.Replace(projectDirectory, string.Empty);
            var subDirectories = difference.Split(new string[] { @"\" }, System.StringSplitOptions.RemoveEmptyEntries);
            var parentModel = default(TreeViewItemModel);
            var currentModel = default(TreeViewItemModel);

            var i = 0;
            var subDirectory = subDirectories[i++];

            // これから作成しようとしているフォルダが、
            // すでに登録されている場合、そのノードインスタンスを取得
            // 無ければ作成して登録
            if (projectModel.Children.Any(x => x.Text == subDirectory))
            {
                parentModel = projectModel.Children.First(x => x.Text == subDirectory);
            }
            else
            {
                parentModel = new TreeViewItemModel { Text = subDirectory, DefinitionType = DefinitionTypes.Folder };
                projectModel.Children.Add(parentModel);
            }

            // ２個目以降、サブフォルダ数分繰り返す
            while (i < subDirectories.Length)
            {
                subDirectory = subDirectories[i++];

                if (parentModel.Children.Any(x => x.Text == subDirectory))
                {
                    currentModel = parentModel.Children.First(x => x.Text == subDirectory);
                }
                else
                {
                    currentModel = new TreeViewItemModel { Text = subDirectory, DefinitionType = DefinitionTypes.Folder };
                    parentModel.Children.Add(currentModel);
                }

                // 現在のフォルダを親フォルダに変えて、再帰
                parentModel = currentModel;
            }

            return parentModel;
        }

        #endregion

        #region ソースコード（TreeViewItemModel） から定義一覧ツリー作成

        public static ObservableCollection<TreeViewItemModel> CreateDefinitionTree(TreeViewItemModel sourceModel)
        {
            if (sourceModel.LanguageType == LanguageTypes.CSharp)
            {
                var walker = new CSharpSourceCodeSyntaxWalker();
                walker.Parse(sourceModel.TargetFile);
                return walker.DefinitionItems;
            }

            if (sourceModel.LanguageType == LanguageTypes.VisualBasic)
            {
                var walker = new VisualBasicSourceCodeSyntaxWalker();
                walker.Parse(sourceModel.TargetFile);
                return walker.DefinitionItems;
            }

            return null;
        }

        #endregion

        #region コンテナ系（class, struct, interface / TreeViewItemModel） から継承元ツリー作成

        public static List<DefinitionHeaderModel> CreateBaseTypeModels(TreeViewItemModel selectedModel)
        {
            // １つ分の表示データを作成
            var models = new List<DefinitionHeaderModel>();
            var model = CreateDefinitionHeaderModel(selectedModel, true);
            models.Add(model);

            // 継承元コレクション探しとセット
            switch (selectedModel.LanguageType)
            {
                case LanguageTypes.CSharp:
                    AddBaseTypesForCSharp(selectedModel, models, model);
                    break;

                case LanguageTypes.VisualBasic:
                    AddBaseTypesForVisualBasic(selectedModel, models, model);
                    break;
            }

            return models;
        }

        private static DefinitionHeaderModel CreateDefinitionHeaderModel(TreeViewItemModel referenceModel, bool isArrowDirectionEnd = false)
        {
            var model = new DefinitionHeaderModel
            {
                ID = Guid.NewGuid().ToString(),
                RelationID = string.Empty,
                IsArrowDirectionEnd = isArrowDirectionEnd,
                DefinitionName = referenceModel.Text,
                DefinitionType = referenceModel.DefinitionType,
                ReferenceModel = referenceModel,
                IsDifferenceFile = false,
                DifferenceName = string.Empty,
                DifferenceFile = string.Empty,
                StartOffset = referenceModel.StartOffset,
                IsExpanded = referenceModel.Children.Any(),
            };

            if (referenceModel.Children.Any())
            {
                var models = new ObservableCollection<TreeViewItemModel>();

                // Enum
                var definitionType = DefinitionTypes.Enum;
                var headerText = "列挙体";
                AddChildrenMembers(models, referenceModel, definitionType, headerText);

                // Delegate
                definitionType = DefinitionTypes.Delegate;
                headerText = "デリゲート";
                AddChildrenMembers(models, referenceModel, definitionType, headerText);

                // Event
                definitionType = DefinitionTypes.Event;
                headerText = "イベント";
                AddChildrenMembers(models, referenceModel, definitionType, headerText);

                // Field
                definitionType = DefinitionTypes.Field;
                headerText = "フィールド";
                AddChildrenMembers(models, referenceModel, definitionType, headerText);

                // Indexer
                definitionType = DefinitionTypes.Indexer;
                headerText = "インデクサー";
                AddChildrenMembers(models, referenceModel, definitionType, headerText);

                // Property
                definitionType = DefinitionTypes.Property;
                headerText = "プロパティ";
                AddChildrenMembers(models, referenceModel, definitionType, headerText);

                // Constructor
                definitionType = DefinitionTypes.Constructor;
                headerText = "コンストラクター";
                AddChildrenMembers(models, referenceModel, definitionType, headerText);

                // Operator
                definitionType = DefinitionTypes.Operator;
                headerText = "オペレーター";
                AddChildrenMembers(models, referenceModel, definitionType, headerText);

                // WindowsAPI
                definitionType = DefinitionTypes.WindowsAPI;
                headerText = "Windows API";
                AddChildrenMembers(models, referenceModel, definitionType, headerText);

                // EventHandler
                definitionType = DefinitionTypes.EventHandler;
                headerText = "イベントハンドラー";
                AddChildrenMembers(models, referenceModel, definitionType, headerText);

                // Method
                definitionType = DefinitionTypes.Method;
                headerText = "メソッド";
                AddChildrenMembers(models, referenceModel, definitionType, headerText);

                model.MemberTreeItems = models;

                if (!models.Any())
                    model.IsExpanded = false;
            }

            return model;
        }

        private static void AddChildrenMembers(ObservableCollection<TreeViewItemModel> models, TreeViewItemModel referenceModel, DefinitionTypes definitionType, string headerText)
        {
            if (referenceModel.Children.Any(x => x.DefinitionType == definitionType))
            {
                var children = referenceModel.Children
                    .Where(x => x.DefinitionType == definitionType)
                    .OrderBy(x => x.Text);          // 名前順
                    //.OrderBy(x => x.StartOffset); // 定義順

                var childHeaderModel = new TreeViewItemModel
                {
                    Text = headerText,
                    DefinitionType = definitionType,
                    IsExpanded = true,
                };

                models.Add(childHeaderModel);

                foreach (var child in children)
                {
                    var childModel = child.Clone();
                    childHeaderModel.Children.Add(childModel);
                }
            }
        }

        private static void AddBaseTypesForCSharp(TreeViewItemModel referenceModel, List<DefinitionHeaderModel> models, DefinitionHeaderModel model)
        {
            // Class
            if (referenceModel.DefinitionType == DefinitionTypes.Class)
            {
                // 継承元クラス、またはインターフェースがある場合
                var node = referenceModel.Tag as ClassDeclarationSyntax;
                if (node.ChildNodes().OfType<BaseListSyntax>().Any())
                {
                    var listNode = node.ChildNodes().OfType<BaseListSyntax>().FirstOrDefault();
                    var baseTypes = listNode.ChildNodes().OfType<SimpleBaseTypeSyntax>();

                    foreach (var baseType in baseTypes)
                        AddBaseTypesForCSharp(referenceModel, models, model, baseType);
                }
            }

            // Struct
            if (referenceModel.DefinitionType == DefinitionTypes.Struct)
            {
                // 継承元クラス、またはインターフェースがある場合
                var node = referenceModel.Tag as StructDeclarationSyntax;
                if (node.ChildNodes().OfType<BaseListSyntax>().Any())
                {
                    var listNode = node.ChildNodes().OfType<BaseListSyntax>().FirstOrDefault();
                    var baseTypes = listNode.ChildNodes().OfType<SimpleBaseTypeSyntax>();

                    foreach (var baseType in baseTypes)
                        AddBaseTypesForCSharp(referenceModel, models, model, baseType);
                }
            }

            // Interface
            if (referenceModel.DefinitionType == DefinitionTypes.Interface)
            {
                // 継承元クラス、またはインターフェースがある場合
                var node = referenceModel.Tag as InterfaceDeclarationSyntax;
                if (node.ChildNodes().OfType<BaseListSyntax>().Any())
                {
                    var listNode = node.ChildNodes().OfType<BaseListSyntax>().FirstOrDefault();
                    var baseTypes = listNode.ChildNodes().OfType<SimpleBaseTypeSyntax>();

                    foreach (var baseType in baseTypes)
                        AddBaseTypesForCSharp(referenceModel, models, model, baseType);
                }
            }
        }

        private static void AddBaseTypesForCSharp(TreeViewItemModel referenceModel, List<DefinitionHeaderModel> models, DefinitionHeaderModel model, SimpleBaseTypeSyntax baseType)
        {
            // 継承元の型名
            var typeName = baseType.ToString();

            // クローズドジェネリック型の場合
            var node = baseType.ChildNodes().FirstOrDefault();
            if (node is Microsoft.CodeAnalysis.CSharp.Syntax.GenericNameSyntax)
            {
                var listNode = node.ChildNodes().FirstOrDefault();           // TypeArgumentListSyntax
                var genericTypes = listNode.ChildNodes();                    // PredefinedTypeSyntax, GenericNameSyntax, ...
                typeName = $"{typeName}<{string.Join(", ", genericTypes)}>";
            }

            model.BaseTypes.Add(typeName);

            // 定義元を探す
            var targetFile = referenceModel.TargetFile;
            var baseTypeStartOffset = baseType.Span.Start;

            if (typeName.Contains("::"))
            {
                // Xxx::Class1, Xxx::NS1.Class1 などのような記述の場合、オフセット位置を Using Alias 名前空間ではなく、最後のコンテナ系定義位置に移動させる
                if (typeName.Contains("."))
                {
                    var index = typeName.LastIndexOf(".") + 1;
                    baseTypeStartOffset += index;
                }
                else
                {
                    var index = typeName.LastIndexOf("::") + 2;
                    baseTypeStartOffset += index;
                }
            }
            else if (typeName.Contains("."))
            {
                // ClassLibrary1.Class1 などのような記述の場合、オフセット位置を名前空間ではなく、最後のコンテナ系定義位置に移動させる
                var index = typeName.LastIndexOf(".") + 1;
                baseTypeStartOffset += index;
            }

            var result = FindDefinitionSourceAtPositionForCSharp(targetFile, baseTypeStartOffset);

            // 見つからない場合
            if (string.IsNullOrEmpty(result.Item1))
            {
                var definitionType = typeName.ToLower().StartsWith("i") ? DefinitionTypes.Interface : DefinitionTypes.Class;
                var argument = new TreeViewItemModel { Text = typeName, DefinitionType = definitionType };
                var notFoundModel = CreateDefinitionHeaderModel(argument, true);
                notFoundModel.RelationID = model.ID;
                models.Add(notFoundModel);

                return;
            }

            // 見つかった場合

            // コンテナ系定義とベース定義の、ソースファイルが違う場合
            var isDifferenceFile = false;
            if (targetFile != result.Item1)
                isDifferenceFile = true;

            //
            targetFile = result.Item1;
            baseTypeStartOffset = result.Item2;

            var nextReferenceModel = new TreeViewItemModel { LanguageType = referenceModel.LanguageType, TargetFile = targetFile };
            var candidates = CreateDefinitionTree(nextReferenceModel);
            nextReferenceModel = SearchModelForCSharp(candidates, baseTypeStartOffset);

            if (nextReferenceModel == null)
            {
                // 見つかった先が、using alias の場合（継承元が using alias 名だった場合）
                result = FoundIfUsingAlias(result);

                // 見つからない場合
                if (string.IsNullOrEmpty(result.Item1))
                {
                    var definitionType = typeName.ToLower().StartsWith("i") ? DefinitionTypes.Interface : DefinitionTypes.Class;
                    var argument = new TreeViewItemModel { Text = typeName, DefinitionType = definitionType };
                    var notFoundModel = CreateDefinitionHeaderModel(argument, true);
                    notFoundModel.RelationID = model.ID;
                    models.Add(notFoundModel);

                    return;
                }

                if (referenceModel.TargetFile != result.Item1)
                    isDifferenceFile = true;

                targetFile = result.Item1;
                baseTypeStartOffset = result.Item2;

                nextReferenceModel = new TreeViewItemModel { LanguageType = referenceModel.LanguageType, TargetFile = targetFile };
                candidates = CreateDefinitionTree(nextReferenceModel);
                nextReferenceModel = SearchModelForCSharp(candidates, baseTypeStartOffset);
            }

            var nextModel = CreateDefinitionHeaderModel(nextReferenceModel, true);
            nextModel.RelationID = model.ID;
            models.Add(nextModel);

            if (isDifferenceFile)
            {
                nextModel.IsDifferenceFile = true;
                nextModel.DifferenceFile = result.Item1;
                nextModel.DifferenceName = Path.GetFileName(result.Item1);
            }

            // 継承元コレクション探しとセット
            AddBaseTypesForCSharp(nextReferenceModel, models, nextModel);
        }

        private static Tuple<string, int> FoundIfUsingAlias(Tuple<string, int> result)
        {
            // 見つかった先が、using alias の場合（継承元が using alias 名だった場合）
            var code = ReadAllText(result.Item1);
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();

            for (var i = 0; i < root.DescendantNodes().Count(); i++)
            {
                var node = root.DescendantNodes().ElementAt(i);
                if (node.Span.Start == result.Item2)
                {
                    // NameEqualsSyntax

                    // +2 index after

                    // QualifiedNameSyntax ClassLibrary1.Class1
                    var syntax = root.DescendantNodes().ElementAt(i + 2);
                    var dotSplitCount = syntax.ToString().Split('.').Length;

                    // IdentifierNameSyntax Class1
                    syntax = root.DescendantNodes().ElementAt(i + 2 + dotSplitCount);

                    var offset = syntax.Span.Start;
                    var sourceFile = result.Item1;

                    var result2 = FindDefinitionSourceAtPositionForCSharp(sourceFile, offset);
                    if (!string.IsNullOrEmpty(result2.Item1))
                        return result2;
                }

                if (node is NamespaceDeclarationSyntax)
                    break;
            }

            return Tuple.Create(string.Empty, -1);
        }

        // 指定の定義開始位置に対応するモデルを探します。
        private static TreeViewItemModel SearchModelForCSharp(ObservableCollection<TreeViewItemModel> candidates, int offset)
        {
            foreach (var model in candidates)
            {
                if (model.IdentifierTokenStartOffset == offset)
                    return model;

                var child = SearchModelForCSharp(model, offset);
                if (child != null)
                    return child;
            }

            return null;
        }

        // 指定の定義開始位置に対応するモデルを探します。
        private static TreeViewItemModel SearchModelForCSharp(TreeViewItemModel parent, int offset)
        {
            foreach (var model in parent.Children)
            {
                if (model.IdentifierTokenStartOffset == offset)
                    return model;

                var child = SearchModelForCSharp(model, offset);
                if (child != null)
                    return child;
            }

            return null;
        }

        private static void AddBaseTypesForVisualBasic(TreeViewItemModel referenceModel, List<DefinitionHeaderModel> models, DefinitionHeaderModel model)
        {
            // Class
            if (referenceModel.DefinitionType == DefinitionTypes.Class)
            {
                // 継承元クラス、またはインターフェースがある場合
                var node = referenceModel.Tag as ClassBlockSyntax;
                var hasInherits = node.ChildNodes().OfType<InheritsStatementSyntax>().Any();
                var hasImplements = node.ChildNodes().OfType<ImplementsStatementSyntax>().Any();
                if (hasInherits || hasImplements)
                {
                    if (hasInherits)
                    {
                        var inheritsNode = node.ChildNodes().OfType<InheritsStatementSyntax>().FirstOrDefault();
                        var childNodes = inheritsNode.ChildNodes();

                        // Class の場合、多重継承はできない仕様だが、将来仕様変更されるか？されないと思う
                        foreach (var childNode in childNodes)
                            AddBaseTypesForVisualBasic(referenceModel, models, model, childNode);
                    }

                    if (hasImplements)
                    {
                        var implementsNode = node.ChildNodes().OfType<ImplementsStatementSyntax>().FirstOrDefault();
                        var childNodes = implementsNode.ChildNodes();

                        foreach (var childNode in childNodes)
                            AddBaseTypesForVisualBasic(referenceModel, models, model, childNode);
                    }
                }
            }

            // Struct
            if (referenceModel.DefinitionType == DefinitionTypes.Struct)
            {
                // 継承元クラス、またはインターフェースがある場合
                var node = referenceModel.Tag as StructureBlockSyntax;
                var hasInherits = node.ChildNodes().OfType<InheritsStatementSyntax>().Any();
                var hasImplements = node.ChildNodes().OfType<ImplementsStatementSyntax>().Any();
                if (hasInherits || hasImplements)
                {
                    if (hasInherits)
                    {
                        // Struct の場合、継承ができないから以下はありえないのだが、将来仕様変更されるか？されないと思う
                        var inheritsNode = node.ChildNodes().OfType<InheritsStatementSyntax>().FirstOrDefault();
                        var childNodes = inheritsNode.ChildNodes();

                        // Struct の場合、多重継承はできない仕様だが、将来仕様変更されるか？されないと思う
                        foreach (var childNode in childNodes)
                            AddBaseTypesForVisualBasic(referenceModel, models, model, childNode);
                    }

                    if (hasImplements)
                    {
                        var implementsNode = node.ChildNodes().OfType<ImplementsStatementSyntax>().FirstOrDefault();
                        var childNodes = implementsNode.ChildNodes();

                        foreach (var childNode in childNodes)
                            AddBaseTypesForVisualBasic(referenceModel, models, model, childNode);
                    }
                }
            }

            // Interface
            if (referenceModel.DefinitionType == DefinitionTypes.Interface)
            {
                // 継承元クラス、またはインターフェースがある場合
                var node = referenceModel.Tag as InterfaceBlockSyntax;
                var hasInherits = node.ChildNodes().OfType<InheritsStatementSyntax>().Any();
                var hasImplements = node.ChildNodes().OfType<ImplementsStatementSyntax>().Any();
                if (hasInherits || hasImplements)
                {
                    if (hasInherits)
                    {
                        var inheritsNode = node.ChildNodes().OfType<InheritsStatementSyntax>().FirstOrDefault();
                        var childNodes = inheritsNode.ChildNodes();

                        // Interface の場合、Inherits, IInterface1, IInterface2 などと記述する
                        // Implements ではなく Inherits でインターフェースを継承する
                        foreach (var childNode in childNodes)
                            AddBaseTypesForVisualBasic(referenceModel, models, model, childNode);
                    }

                    if (hasImplements)
                    {
                        // 上記仕様から以下はありえないのだが、将来仕様変更されるか？されないと思う
                        var implementsNode = node.ChildNodes().OfType<ImplementsStatementSyntax>().FirstOrDefault();
                        var childNodes = implementsNode.ChildNodes();

                        foreach (var childNode in childNodes)
                            AddBaseTypesForVisualBasic(referenceModel, models, model, childNode);
                    }
                }
            }
        }

        private static void AddBaseTypesForVisualBasic(TreeViewItemModel referenceModel, List<DefinitionHeaderModel> models, DefinitionHeaderModel model, SyntaxNode baseType)
        {
            // 継承元の型名
            var typeName = baseType.ToString();

            // クローズドジェネリック型の場合
            var node = baseType.ChildNodes().FirstOrDefault();
            if (node is Microsoft.CodeAnalysis.VisualBasic.Syntax.GenericNameSyntax)
            {
                var listNode = node.ChildNodes().FirstOrDefault();           // TypeArgumentListSyntax
                var genericTypes = listNode.ChildNodes();                    // PredefinedTypeSyntax, GenericNameSyntax, ...
                typeName = $"{typeName}(Of {string.Join(", ", genericTypes)})";
            }

            model.BaseTypes.Add(typeName);

            // 定義元を探す
            var targetFile = referenceModel.TargetFile;
            var baseTypeStartOffset = baseType.Span.Start;

            if (typeName.Contains("::"))
            {
                // Xxx::Class1, Xxx::NS1.Class1 などのような記述の場合、オフセット位置を Using Alias 名前空間ではなく、最後のコンテナ系定義位置に移動させる
                if (typeName.Contains("."))
                {
                    var index = typeName.LastIndexOf(".") + 1;
                    baseTypeStartOffset += index;
                }
                else
                {
                    var index = typeName.LastIndexOf("::") + 2;
                    baseTypeStartOffset += index;
                }
            }
            else if (typeName.Contains("."))
            {
                // ClassLibrary1.Class1 などのような記述の場合、オフセット位置を名前空間ではなく、最後のコンテナ系定義位置に移動させる
                var index = typeName.LastIndexOf(".") + 1;
                baseTypeStartOffset += index;
            }

            var result = FindDefinitionSourceAtPositionForVisualBasic(targetFile, baseTypeStartOffset);

            // 見つからない場合
            if (string.IsNullOrEmpty(result.Item1))
            {
                var definitionType = typeName.ToLower().StartsWith("i") ? DefinitionTypes.Interface : DefinitionTypes.Class;
                var argument = new TreeViewItemModel { Text = typeName, DefinitionType = definitionType };
                var notFoundModel = CreateDefinitionHeaderModel(argument, true);
                notFoundModel.RelationID = model.ID;
                models.Add(notFoundModel);

                return;
            }

            // 見つかった場合

            // コンテナ系定義とベース定義の、ソースファイルが違う場合
            var isDifferenceFile = false;
            if (targetFile != result.Item1)
                isDifferenceFile = true;

            //
            targetFile = result.Item1;
            baseTypeStartOffset = result.Item2;

            var nextReferenceModel = new TreeViewItemModel { LanguageType = referenceModel.LanguageType, TargetFile = targetFile };
            var candidates = CreateDefinitionTree(nextReferenceModel);
            nextReferenceModel = SearchModelForVisualBasic(candidates, baseTypeStartOffset);

            if (nextReferenceModel == null)
            {
                // 見つかった先が、Imports Alias の場合（継承元が Imports Alias 名だった場合）
                result = FoundIfImportsAlias(result);

                // 見つからない場合
                if (string.IsNullOrEmpty(result.Item1))
                {
                    var definitionType = typeName.ToLower().StartsWith("i") ? DefinitionTypes.Interface : DefinitionTypes.Class;
                    var argument = new TreeViewItemModel { Text = typeName, DefinitionType = definitionType };
                    var notFoundModel = CreateDefinitionHeaderModel(argument, true);
                    notFoundModel.RelationID = model.ID;
                    models.Add(notFoundModel);

                    return;
                }

                if (referenceModel.TargetFile != result.Item1)
                    isDifferenceFile = true;

                targetFile = result.Item1;
                baseTypeStartOffset = result.Item2;

                nextReferenceModel = new TreeViewItemModel { LanguageType = referenceModel.LanguageType, TargetFile = targetFile };
                candidates = CreateDefinitionTree(nextReferenceModel);
                nextReferenceModel = SearchModelForVisualBasic(candidates, baseTypeStartOffset);
            }

            var nextModel = CreateDefinitionHeaderModel(nextReferenceModel, true);
            nextModel.RelationID = model.ID;
            models.Add(nextModel);

            if (isDifferenceFile)
            {
                nextModel.IsDifferenceFile = true;
                nextModel.DifferenceFile = result.Item1;
                nextModel.DifferenceName = Path.GetFileName(result.Item1);
            }

            // 継承元コレクション探しとセット
            AddBaseTypesForVisualBasic(nextReferenceModel, models, nextModel);
        }

        private static Tuple<string, int> FoundIfImportsAlias(Tuple<string, int> result)
        {
            // 見つかった先が、Imports Alias の場合（継承元が Imports Alias 名だった場合）
            var code = ReadAllText(result.Item1);
            var tree = VisualBasicSyntaxTree.ParseText(code);
            var root = tree.GetRoot();

            for (var i = 0; i < root.DescendantNodes().Count(); i++)
            {
                var node = root.DescendantNodes().ElementAt(i);
                if (node.Span.Start == result.Item2)
                {
                    // SimpleImportsClauseSyntax

                    // +2 index after

                    // QualifiedNameSyntax ClassLibrary1.Class1
                    var syntax = root.DescendantNodes().ElementAt(i + 2);
                    var dotSplitCount = syntax.ToString().Split('.').Length;

                    // IdentifierNameSyntax Class1
                    syntax = root.DescendantNodes().ElementAt(i + 2 + dotSplitCount);

                    var offset = syntax.Span.Start;
                    var sourceFile = result.Item1;

                    var result2 = FindDefinitionSourceAtPositionForVisualBasic(sourceFile, offset);
                    if (!string.IsNullOrEmpty(result2.Item1))
                        return result2;
                }
            }

            return Tuple.Create(string.Empty, -1);
        }

        // 指定の定義開始位置に対応するモデルを探します。
        private static TreeViewItemModel SearchModelForVisualBasic(ObservableCollection<TreeViewItemModel> candidates, int offset)
        {
            foreach (var model in candidates)
            {
                if (model.IdentifierTokenStartOffset == offset)
                    return model;

                var child = SearchModelForVisualBasic(model, offset);
                if (child != null)
                    return child;
            }

            return null;
        }

        // 指定の定義開始位置に対応するモデルを探します。
        private static TreeViewItemModel SearchModelForVisualBasic(TreeViewItemModel parent, int offset)
        {
            foreach (var model in parent.Children)
            {
                if (model.IdentifierTokenStartOffset == offset)
                    return model;

                var child = SearchModelForVisualBasic(model, offset);
                if (child != null)
                    return child;
            }

            return null;
        }


        private static Regex RemoveNamespaceRegex = null;

        private static string RemoveNamespace(string parameterType)
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

        /// <summary>
        /// 指定のソースファイルの指定位置をもとに、定義先のソースファイルとその位置を返却します。
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="offset"></param>
        /// <returns>
        /// 定義先のソースファイルは、同じファイル内か別のファイルになります。<br></br>
        /// 定義位置は、SyntaxWalker で取得する定義位置ではなく、定義名の位置になるので注意が必要です。
        /// </returns>
        public static Tuple<string, int> FindDefinitionSourceAtPositionForCSharp(string sourceFile, int offset)
        {
            var tree = AppEnv.CSharpSyntaxTrees.FirstOrDefault(x => x.FilePath == sourceFile);
            var symbol = default(ISymbol);

            // 定義元が誤検知してしまうバグの対応
            // 
            // 1プロジェクト毎にコンパイラを作成・登録しているが、登録対象のソースファイルは累積のリストとなっている。
            // 後になるにつれて、他プロジェクトのソースコードも含めてコンパイラを作成しているため、
            // 検索範囲が一番多い最後のコンパイラが一番精度が高くなっている。よって、降順で探してもらうことにする
            for (var i = AppEnv.CSharpCompilations.Count - 1; i >= 0; i--)
            {
                try
                {
                    var compItem = AppEnv.CSharpCompilations[i];
                    var model = compItem.GetSemanticModel(tree);
                    if (model == null)
                        continue;

                    var ws = MSBuildWorkspace.Create();
                    symbol = SymbolFinder.FindSymbolAtPositionAsync(model, offset, ws).Result;
                    if (symbol != null && symbol.Locations.Any() && symbol.Locations[0].IsInSource)
                        break;
                }
                catch (Exception)
                {
                    /*
                     * 以下のタイミングで、例外エラー発生してしまうバグの対応（いつから？開発途中から？最初は出ていなかったような？）
                     * var model = compItem.GetSemanticModel(tree);
                     * 
                     * 
                     * ・内容
                     * 'System.ArgumentException' (Microsoft.CodeAnalysis.CSharp.dll の中)
                     * System.ArgumentException: SyntaxTree はコンパイルの一部ではありません
                     * 
                     * 'System.ArgumentException'
                     * 'SyntaxTree is not part of the compilation
                     * 
                     * ・対策
                     * ネット上を調べても原因が分からず。
                     * 例外エラーを無視して、見つからなければ諦めることにする
                     * 
                     * 
                     * 
                     */
                }
            }

            if (symbol == null || !symbol.Locations.Any())
                return Tuple.Create(string.Empty, -1);

            sourceFile = symbol.Locations[0].SourceTree?.FilePath;
            offset = symbol.Locations[0].SourceSpan.Start;

            if (!string.IsNullOrEmpty(sourceFile) && File.Exists(sourceFile))
                return Tuple.Create(sourceFile, offset);

            return Tuple.Create(string.Empty, -1);
        }

        public static Tuple<string, int> FindDefinitionSourceAtPositionForVisualBasic(string sourceFile, int offset)
        {
            var tree = AppEnv.VisualBasicSyntaxTrees.FirstOrDefault(x => x.FilePath == sourceFile);
            var symbol = default(ISymbol);

            for (var i = AppEnv.VisualBasicCompilations.Count - 1; i >= 0; i--)
            {
                try
                {
                    var compItem = AppEnv.VisualBasicCompilations[i];
                    var model = compItem.GetSemanticModel(tree);
                    if (model == null)
                        continue;

                    var ws = MSBuildWorkspace.Create();
                    symbol = SymbolFinder.FindSymbolAtPositionAsync(model, offset, ws).Result;
                    if (symbol != null && symbol.Locations.Any() && symbol.Locations[0].IsInSource)
                        break;
                }
                catch (Exception)
                {
                }
            }

            if (symbol == null || !symbol.Locations.Any())
                return Tuple.Create(string.Empty, -1);

            sourceFile = symbol.Locations[0].SourceTree?.FilePath;
            offset = symbol.Locations[0].SourceSpan.Start;

            if (!string.IsNullOrEmpty(sourceFile) && File.Exists(sourceFile))
                return Tuple.Create(sourceFile, offset);

            return Tuple.Create(string.Empty, -1);
        }

        #endregion

        #region コンテナ系（class, struct, interface / TreeViewItemModel） から継承先ツリー作成

        public static List<DefinitionHeaderModel> CreateInheritanceTypeModels(TreeViewItemModel selectedModel)
        {
            // １つ分の表示データを作成
            var models = new List<DefinitionHeaderModel>();
            var model = CreateDefinitionHeaderModel(selectedModel);
            models.Add(model);

            // 継承先コレクション探しとセット
            switch (selectedModel.LanguageType)
            {
                case LanguageTypes.CSharp:
                    SetBaseTypesForInheritanceTypesForCSharp(selectedModel, model);
                    AddInheritanceTypesForCSharp(selectedModel, models, model);
                    break;

                case LanguageTypes.VisualBasic:
                    SetBaseTypesForInheritanceTypesForVisualBasic(selectedModel, model);
                    AddInheritanceTypesForVisualBasic(selectedModel, models, model);
                    break;
            }

            return models;
        }

        private static void AddInheritanceTypesForCSharp(TreeViewItemModel referenceModel, List<DefinitionHeaderModel> models, DefinitionHeaderModel model)
        {
            foreach (var tree in AppEnv.CSharpSyntaxTrees)
            {
                var targetFile = tree.FilePath;
                var root = tree.GetRoot();

                // class
                var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
                foreach (var node in classes)
                {
                    if (node.ChildNodes().OfType<BaseListSyntax>().Any())
                    {
                        var defineToken = node.ChildTokens().FirstOrDefault(x => x.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.IdentifierToken));
                        var listNode = node.ChildNodes().OfType<BaseListSyntax>().FirstOrDefault();
                        var baseTypes = listNode.ChildNodes().OfType<SimpleBaseTypeSyntax>();

                        foreach (var baseType in baseTypes)
                        {
                            var baseTypeStartOffset = baseType.Span.Start;
                            var typeName = baseType.ToString();

                            if (typeName.Contains("::"))
                            {
                                // Xxx::Class1, Xxx::NS1.Class1 などのような記述の場合、オフセット位置を Using Alias 名前空間ではなく、最後のコンテナ系定義位置に移動させる
                                if (typeName.Contains("."))
                                {
                                    var index = typeName.LastIndexOf(".") + 1;
                                    baseTypeStartOffset += index;
                                }
                                else
                                {
                                    var index = typeName.LastIndexOf("::") + 2;
                                    baseTypeStartOffset += index;
                                }
                            }
                            else if (typeName.Contains("."))
                            {
                                // ClassLibrary1.Class1 などのような記述の場合、オフセット位置を名前空間ではなく、最後のコンテナ系定義位置に移動させる
                                var index = typeName.LastIndexOf(".") + 1;
                                baseTypeStartOffset += index;
                            }

                            AddInheritanceTypesForCSharp(referenceModel, models, model, targetFile, defineToken.Span.Start, baseTypeStartOffset);
                        }
                    }
                }

                // interface
                var interfaces = root.DescendantNodes().OfType<InterfaceDeclarationSyntax>();
                foreach (var node in interfaces)
                {
                    if (node.ChildNodes().OfType<BaseListSyntax>().Any())
                    {
                        var defineToken = node.ChildTokens().FirstOrDefault(x => x.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.IdentifierToken));
                        var listNode = node.ChildNodes().OfType<BaseListSyntax>().FirstOrDefault();
                        var baseTypes = listNode.ChildNodes().OfType<SimpleBaseTypeSyntax>();

                        foreach (var baseType in baseTypes)
                        {
                            var baseTypeStartOffset = baseType.Span.Start;
                            var typeName = baseType.ToString();

                            if (typeName.Contains("::"))
                            {
                                // Xxx::Class1, Xxx::NS1.Class1 などのような記述の場合、オフセット位置を Using Alias 名前空間ではなく、最後のコンテナ系定義位置に移動させる
                                if (typeName.Contains("."))
                                {
                                    var index = typeName.LastIndexOf(".") + 1;
                                    baseTypeStartOffset += index;
                                }
                                else
                                {
                                    var index = typeName.LastIndexOf("::") + 2;
                                    baseTypeStartOffset += index;
                                }
                            }
                            else if (typeName.Contains("."))
                            {
                                // ClassLibrary1.Class1 などのような記述の場合、オフセット位置を名前空間ではなく、最後のコンテナ系定義位置に移動させる
                                var index = typeName.LastIndexOf(".") + 1;
                                baseTypeStartOffset += index;
                            }

                            AddInheritanceTypesForCSharp(referenceModel, models, model, targetFile, defineToken.Span.Start, baseTypeStartOffset);
                        }
                    }
                }
            }
        }

        private static void AddInheritanceTypesForCSharp(TreeViewItemModel referenceModel, List<DefinitionHeaderModel> models, DefinitionHeaderModel model, string targetFile, int containerTypeStartOffset, int baseTypeStartOffset)
        {
            // BaseType の定義元を探す
            var result = FindDefinitionSourceAtPositionForCSharp(targetFile, baseTypeStartOffset);

            // 見つからない場合
            if (string.IsNullOrEmpty(result.Item1))
                return;

            // 見つかった場合、自分と候補が同じものかどうかチェック
            if (referenceModel.TargetFile == result.Item1 && referenceModel.IdentifierTokenStartOffset == result.Item2)
            {
                // 自分を継承元にしているコンテナ系定義を発見した。クラス定義かインターフェース定義を登録する
                var nextReferenceModel = new TreeViewItemModel { LanguageType = referenceModel.LanguageType, TargetFile = targetFile };
                var candidates = CreateDefinitionTree(nextReferenceModel);
                nextReferenceModel = SearchModelForCSharp(candidates, containerTypeStartOffset);

                var nextModel = CreateDefinitionHeaderModel(nextReferenceModel);
                models.Add(nextModel);

                nextModel.RelationID = model.ID;
                SetBaseTypesForInheritanceTypesForCSharp(nextReferenceModel, nextModel);

                if (referenceModel.TargetFile != targetFile)
                {
                    nextModel.IsDifferenceFile = true;
                    nextModel.DifferenceFile = targetFile;
                    nextModel.DifferenceName = Path.GetFileName(targetFile);
                }

                // 継承元コレクション探しとセット
                AddInheritanceTypesForCSharp(nextReferenceModel, models, nextModel);
            }
            else
            {
                // 不一致の場合、using alias を見つけてきたか？
                result = FoundIfUsingAlias(result);

                // 見つからない場合
                if (string.IsNullOrEmpty(result.Item1))
                    return;

                // 見つかった場合、自分と候補が同じものかどうかチェック
                if (referenceModel.TargetFile == result.Item1 && referenceModel.IdentifierTokenStartOffset == result.Item2)
                {
                    // 自分を継承元にしているコンテナ系定義を発見した。クラス定義かインターフェース定義を登録する
                    var nextReferenceModel = new TreeViewItemModel { LanguageType = referenceModel.LanguageType, TargetFile = targetFile };
                    var candidates = CreateDefinitionTree(nextReferenceModel);
                    nextReferenceModel = SearchModelForCSharp(candidates, containerTypeStartOffset);

                    var nextModel = CreateDefinitionHeaderModel(nextReferenceModel);
                    models.Add(nextModel);

                    nextModel.RelationID = model.ID;
                    SetBaseTypesForInheritanceTypesForCSharp(nextReferenceModel, nextModel);

                    if (referenceModel.TargetFile != targetFile)
                    {
                        nextModel.IsDifferenceFile = true;
                        nextModel.DifferenceFile = targetFile;
                        nextModel.DifferenceName = Path.GetFileName(targetFile);
                    }

                    // 継承元コレクション探しとセット
                    AddInheritanceTypesForCSharp(nextReferenceModel, models, nextModel);
                }

                return;
            }
        }

        // （呼び出し元の）referenceModel.Text だけだと、図形を見た際「継承元が１つだけ」と勘違いしてしまうため、全ての継承元を表示する
        // ただし、継承関係図としては、すべての継承元の図形を描画する必要はない
        private static void SetBaseTypesForInheritanceTypesForCSharp(TreeViewItemModel nextReferenceModel, DefinitionHeaderModel nextModel)
        {
            // Class
            if (nextReferenceModel.DefinitionType == DefinitionTypes.Class)
            {
                // 継承元クラス、またはインターフェースがある場合
                var node = nextReferenceModel.Tag as ClassDeclarationSyntax;
                if (node.ChildNodes().OfType<BaseListSyntax>().Any())
                {
                    var listNode = node.ChildNodes().OfType<BaseListSyntax>().FirstOrDefault();
                    var baseTypes = listNode.ChildNodes().OfType<SimpleBaseTypeSyntax>();

                    foreach (var baseType in baseTypes)
                    {
                        // 継承元の型名
                        var typeName = baseType.ToString();

                        // クローズドジェネリック型の場合
                        var genericNode = baseType.ChildNodes().FirstOrDefault();
                        if (genericNode is Microsoft.CodeAnalysis.CSharp.Syntax.GenericNameSyntax)
                        {
                            var genericListNode = genericNode.ChildNodes().FirstOrDefault(); // TypeArgumentListSyntax
                            var genericTypes = genericListNode.ChildNodes();                 // PredefinedTypeSyntax, GenericNameSyntax, ...
                            typeName = $"{typeName}<{string.Join(", ", genericTypes)}>";
                        }

                        nextModel.BaseTypes.Add(typeName);
                    }
                }
            }

            // Struct
            if (nextReferenceModel.DefinitionType == DefinitionTypes.Struct)
            {
                // 継承元クラス、またはインターフェースがある場合
                var node = nextReferenceModel.Tag as StructDeclarationSyntax;
                if (node.ChildNodes().OfType<BaseListSyntax>().Any())
                {
                    var listNode = node.ChildNodes().OfType<BaseListSyntax>().FirstOrDefault();
                    var baseTypes = listNode.ChildNodes().OfType<SimpleBaseTypeSyntax>();

                    foreach (var baseType in baseTypes)
                    {
                        // 継承元の型名
                        var typeName = baseType.ToString();

                        // クローズドジェネリック型の場合
                        var genericNode = baseType.ChildNodes().FirstOrDefault();
                        if (genericNode is Microsoft.CodeAnalysis.CSharp.Syntax.GenericNameSyntax)
                        {
                            var genericListNode = genericNode.ChildNodes().FirstOrDefault(); // TypeArgumentListSyntax
                            var genericTypes = genericListNode.ChildNodes();                 // PredefinedTypeSyntax, GenericNameSyntax, ...
                            typeName = $"{typeName}<{string.Join(", ", genericTypes)}>";
                        }

                        nextModel.BaseTypes.Add(typeName);
                    }
                }
            }

            // Interface
            if (nextReferenceModel.DefinitionType == DefinitionTypes.Interface)
            {
                // 継承元クラス、またはインターフェースがある場合
                var node = nextReferenceModel.Tag as InterfaceDeclarationSyntax;
                if (node.ChildNodes().OfType<BaseListSyntax>().Any())
                {
                    var listNode = node.ChildNodes().OfType<BaseListSyntax>().FirstOrDefault();
                    var baseTypes = listNode.ChildNodes().OfType<SimpleBaseTypeSyntax>();

                    foreach (var baseType in baseTypes)
                    {
                        // 継承元の型名
                        var typeName = baseType.ToString();

                        // クローズドジェネリック型の場合
                        var genericNode = baseType.ChildNodes().FirstOrDefault();
                        if (genericNode is Microsoft.CodeAnalysis.CSharp.Syntax.GenericNameSyntax)
                        {
                            var genericListNode = genericNode.ChildNodes().FirstOrDefault(); // TypeArgumentListSyntax
                            var genericTypes = genericListNode.ChildNodes();                 // PredefinedTypeSyntax, GenericNameSyntax, ...
                            typeName = $"{typeName}<{string.Join(", ", genericTypes)}>";
                        }

                        nextModel.BaseTypes.Add(typeName);
                    }
                }
            }
        }

        private static void AddInheritanceTypesForVisualBasic(TreeViewItemModel referenceModel, List<DefinitionHeaderModel> models, DefinitionHeaderModel model)
        {
            foreach (var tree in AppEnv.VisualBasicSyntaxTrees)
            {
                var targetFile = tree.FilePath;
                var root = tree.GetRoot();

                // class
                var classes = root.DescendantNodes().OfType<ClassBlockSyntax>();
                foreach (var node in classes)
                {
                    var statementNode = node.ChildNodes().OfType<ClassStatementSyntax>().FirstOrDefault();
                    var defineToken = statementNode.ChildTokens().FirstOrDefault(x => x.IsKind(Microsoft.CodeAnalysis.VisualBasic.SyntaxKind.IdentifierToken));

                    var hasInherits = node.ChildNodes().OfType<InheritsStatementSyntax>().Any();
                    var hasImplements = node.ChildNodes().OfType<ImplementsStatementSyntax>().Any();
                    if (hasInherits || hasImplements)
                    {
                        if (hasInherits)
                        {
                            var inheritsNode = node.ChildNodes().OfType<InheritsStatementSyntax>().FirstOrDefault();
                            var childNodes = inheritsNode.ChildNodes();

                            // Class の場合、多重継承はできない仕様だが、将来仕様変更されるか？されないと思う
                            foreach (var childNode in childNodes)
                            {
                                var baseTypeStartOffset = childNode.Span.Start;
                                var typeName = childNode.ToString();

                                if (typeName.Contains("::"))
                                {
                                    // Xxx::Class1, Xxx::NS1.Class1 などのような記述の場合、オフセット位置を Imports Alias 名前空間ではなく、最後のコンテナ系定義位置に移動させる
                                    if (typeName.Contains("."))
                                    {
                                        var index = typeName.LastIndexOf(".") + 1;
                                        baseTypeStartOffset += index;
                                    }
                                    else
                                    {
                                        var index = typeName.LastIndexOf("::") + 2;
                                        baseTypeStartOffset += index;
                                    }
                                }
                                else if (typeName.Contains("."))
                                {
                                    // ClassLibrary1.Class1 などのような記述の場合、オフセット位置を名前空間ではなく、最後のコンテナ系定義位置に移動させる
                                    var index = typeName.LastIndexOf(".") + 1;
                                    baseTypeStartOffset += index;
                                }

                                AddInheritanceTypesForVisualBasic(referenceModel, models, model, targetFile, defineToken.Span.Start, baseTypeStartOffset);
                            }
                        }

                        if (hasImplements)
                        {
                            var implementsNode = node.ChildNodes().OfType<ImplementsStatementSyntax>().FirstOrDefault();
                            var childNodes = implementsNode.ChildNodes();

                            foreach (var childNode in childNodes)
                            {
                                var baseTypeStartOffset = childNode.Span.Start;
                                var typeName = childNode.ToString();

                                if (typeName.Contains("::"))
                                {
                                    // Xxx::Class1, Xxx::NS1.Class1 などのような記述の場合、オフセット位置を Imports Alias 名前空間ではなく、最後のコンテナ系定義位置に移動させる
                                    if (typeName.Contains("."))
                                    {
                                        var index = typeName.LastIndexOf(".") + 1;
                                        baseTypeStartOffset += index;
                                    }
                                    else
                                    {
                                        var index = typeName.LastIndexOf("::") + 2;
                                        baseTypeStartOffset += index;
                                    }
                                }
                                else if (typeName.Contains("."))
                                {
                                    // ClassLibrary1.Class1 などのような記述の場合、オフセット位置を名前空間ではなく、最後のコンテナ系定義位置に移動させる
                                    var index = typeName.LastIndexOf(".") + 1;
                                    baseTypeStartOffset += index;
                                }

                                AddInheritanceTypesForVisualBasic(referenceModel, models, model, targetFile, defineToken.Span.Start, baseTypeStartOffset);
                            }
                        }
                    }
                }

                // interface
                var interfaces = root.DescendantNodes().OfType<InterfaceBlockSyntax>();
                foreach (var node in interfaces)
                {
                    var statementNode = node.ChildNodes().OfType<InterfaceStatementSyntax>().FirstOrDefault();
                    var defineToken = statementNode.ChildTokens().FirstOrDefault(x => x.IsKind(Microsoft.CodeAnalysis.VisualBasic.SyntaxKind.IdentifierToken));

                    var hasInherits = node.ChildNodes().OfType<InheritsStatementSyntax>().Any();
                    var hasImplements = node.ChildNodes().OfType<ImplementsStatementSyntax>().Any();
                    if (hasInherits || hasImplements)
                    {
                        if (hasInherits)
                        {
                            var inheritsNode = node.ChildNodes().OfType<InheritsStatementSyntax>().FirstOrDefault();
                            var childNodes = inheritsNode.ChildNodes();

                            // Interface の場合、Inherits, IInterface1, IInterface2 などと記述する
                            // Implements ではなく Inherits でインターフェースを継承する
                            foreach (var childNode in childNodes)
                            {
                                var baseTypeStartOffset = childNode.Span.Start;
                                var typeName = childNode.ToString();

                                if (typeName.Contains("::"))
                                {
                                    // Xxx::Class1, Xxx::NS1.Class1 などのような記述の場合、オフセット位置を Imports Alias 名前空間ではなく、最後のコンテナ系定義位置に移動させる
                                    if (typeName.Contains("."))
                                    {
                                        var index = typeName.LastIndexOf(".") + 1;
                                        baseTypeStartOffset += index;
                                    }
                                    else
                                    {
                                        var index = typeName.LastIndexOf("::") + 2;
                                        baseTypeStartOffset += index;
                                    }
                                }
                                else if (typeName.Contains("."))
                                {
                                    // ClassLibrary1.Class1 などのような記述の場合、オフセット位置を名前空間ではなく、最後のコンテナ系定義位置に移動させる
                                    var index = typeName.LastIndexOf(".") + 1;
                                    baseTypeStartOffset += index;
                                }

                                AddInheritanceTypesForVisualBasic(referenceModel, models, model, targetFile, defineToken.Span.Start, baseTypeStartOffset);
                            }
                        }

                        if (hasImplements)
                        {
                            // 上記仕様から以下はありえないのだが、将来仕様変更されるか？されないと思う
                            var implementsNode = node.ChildNodes().OfType<ImplementsStatementSyntax>().FirstOrDefault();
                            var childNodes = implementsNode.ChildNodes();

                            foreach (var childNode in childNodes)
                            {
                                var baseTypeStartOffset = childNode.Span.Start;
                                var typeName = childNode.ToString();

                                if (typeName.Contains("::"))
                                {
                                    // Xxx::Class1, Xxx::NS1.Class1 などのような記述の場合、オフセット位置を Imports Alias 名前空間ではなく、最後のコンテナ系定義位置に移動させる
                                    if (typeName.Contains("."))
                                    {
                                        var index = typeName.LastIndexOf(".") + 1;
                                        baseTypeStartOffset += index;
                                    }
                                    else
                                    {
                                        var index = typeName.LastIndexOf("::") + 2;
                                        baseTypeStartOffset += index;
                                    }
                                }
                                else if (typeName.Contains("."))
                                {
                                    // ClassLibrary1.Class1 などのような記述の場合、オフセット位置を名前空間ではなく、最後のコンテナ系定義位置に移動させる
                                    var index = typeName.LastIndexOf(".") + 1;
                                    baseTypeStartOffset += index;
                                }

                                AddInheritanceTypesForVisualBasic(referenceModel, models, model, targetFile, defineToken.Span.Start, baseTypeStartOffset);
                            }
                        }
                    }
                }
            }
        }

        private static void AddInheritanceTypesForVisualBasic(TreeViewItemModel referenceModel, List<DefinitionHeaderModel> models, DefinitionHeaderModel model, string targetFile, int containerTypeStartOffset, int baseTypeStartOffset)
        {
            // BaseType の定義元を探す
            var result = FindDefinitionSourceAtPositionForVisualBasic(targetFile, baseTypeStartOffset);

            // 見つからない場合
            if (string.IsNullOrEmpty(result.Item1))
                return;

            // 見つかった場合、自分と候補が同じものかどうかチェック
            if (referenceModel.TargetFile == result.Item1 && referenceModel.IdentifierTokenStartOffset == result.Item2)
            {
                // 自分を継承元にしているコンテナ系定義を発見した。クラス定義かインターフェース定義を登録する
                var nextReferenceModel = new TreeViewItemModel { LanguageType = referenceModel.LanguageType, TargetFile = targetFile };
                var candidates = CreateDefinitionTree(nextReferenceModel);
                nextReferenceModel = SearchModelForVisualBasic(candidates, containerTypeStartOffset);

                var nextModel = CreateDefinitionHeaderModel(nextReferenceModel);
                models.Add(nextModel);

                nextModel.RelationID = model.ID;
                SetBaseTypesForInheritanceTypesForVisualBasic(nextReferenceModel, nextModel);

                if (referenceModel.TargetFile != targetFile)
                {
                    nextModel.IsDifferenceFile = true;
                    nextModel.DifferenceFile = targetFile;
                    nextModel.DifferenceName = Path.GetFileName(targetFile);
                }

                // 継承元コレクション探しとセット
                AddInheritanceTypesForVisualBasic(nextReferenceModel, models, nextModel);
            }
            else
            {
                // 不一致の場合、Imports Alias を見つけてきたか？
                result = FoundIfImportsAlias(result);

                // 見つからない場合
                if (string.IsNullOrEmpty(result.Item1))
                    return;

                // 見つかった場合、自分と候補が同じものかどうかチェック
                if (referenceModel.TargetFile == result.Item1 && referenceModel.IdentifierTokenStartOffset == result.Item2)
                {
                    // 自分を継承元にしているコンテナ系定義を発見した。クラス定義かインターフェース定義を登録する
                    var nextReferenceModel = new TreeViewItemModel { LanguageType = referenceModel.LanguageType, TargetFile = targetFile };
                    var candidates = CreateDefinitionTree(nextReferenceModel);
                    nextReferenceModel = SearchModelForVisualBasic(candidates, containerTypeStartOffset);

                    var nextModel = CreateDefinitionHeaderModel(nextReferenceModel);
                    models.Add(nextModel);

                    nextModel.RelationID = model.ID;
                    SetBaseTypesForInheritanceTypesForVisualBasic(nextReferenceModel, nextModel);

                    if (referenceModel.TargetFile != targetFile)
                    {
                        nextModel.IsDifferenceFile = true;
                        nextModel.DifferenceFile = targetFile;
                        nextModel.DifferenceName = Path.GetFileName(targetFile);
                    }

                    // 継承元コレクション探しとセット
                    AddInheritanceTypesForVisualBasic(nextReferenceModel, models, nextModel);
                }

                return;
            }
        }

        private static void SetBaseTypesForInheritanceTypesForVisualBasic(TreeViewItemModel nextReferenceModel, DefinitionHeaderModel nextModel)
        {
            // Class
            if (nextReferenceModel.DefinitionType == DefinitionTypes.Class)
            {
                // 継承元クラス、またはインターフェースがある場合
                var node = nextReferenceModel.Tag as ClassBlockSyntax;
                var hasInherits = node.ChildNodes().OfType<InheritsStatementSyntax>().Any();
                var hasImplements = node.ChildNodes().OfType<ImplementsStatementSyntax>().Any();
                if (hasInherits || hasImplements)
                {
                    if (hasInherits)
                    {
                        var inheritsNode = node.ChildNodes().OfType<InheritsStatementSyntax>().FirstOrDefault();
                        var childNodes = inheritsNode.ChildNodes();

                        // Class の場合、多重継承はできない仕様だが、将来仕様変更されるか？されないと思う
                        foreach (var childNode in childNodes)
                        {
                            // 継承元の型名
                            var typeName = childNode.ToString();

                            // クローズドジェネリック型の場合
                            var genericNode = childNode.ChildNodes().FirstOrDefault();
                            if (genericNode is Microsoft.CodeAnalysis.VisualBasic.Syntax.GenericNameSyntax)
                            {
                                var listNode = genericNode.ChildNodes().FirstOrDefault(); // TypeArgumentListSyntax
                                var genericTypes = listNode.ChildNodes();                 // PredefinedTypeSyntax, GenericNameSyntax, ...
                                typeName = $"{typeName}(Of {string.Join(", ", genericTypes)})";
                            }

                            nextModel.BaseTypes.Add(typeName);
                        }
                    }

                    if (hasImplements)
                    {
                        var implementsNode = node.ChildNodes().OfType<ImplementsStatementSyntax>().FirstOrDefault();
                        var childNodes = implementsNode.ChildNodes();

                        foreach (var childNode in childNodes)
                        {
                            // 継承元の型名
                            var typeName = childNode.ToString();

                            // クローズドジェネリック型の場合
                            var genericNode = childNode.ChildNodes().FirstOrDefault();
                            if (genericNode is Microsoft.CodeAnalysis.VisualBasic.Syntax.GenericNameSyntax)
                            {
                                var listNode = genericNode.ChildNodes().FirstOrDefault(); // TypeArgumentListSyntax
                                var genericTypes = listNode.ChildNodes();                 // PredefinedTypeSyntax, GenericNameSyntax, ...
                                typeName = $"{typeName}(Of {string.Join(", ", genericTypes)})";
                            }

                            nextModel.BaseTypes.Add(typeName);
                        }
                    }
                }
            }

            // Struct
            if (nextReferenceModel.DefinitionType == DefinitionTypes.Struct)
            {
                // 継承元クラス、またはインターフェースがある場合
                var node = nextReferenceModel.Tag as StructureBlockSyntax;
                var hasInherits = node.ChildNodes().OfType<InheritsStatementSyntax>().Any();
                var hasImplements = node.ChildNodes().OfType<ImplementsStatementSyntax>().Any();
                if (hasInherits || hasImplements)
                {
                    if (hasInherits)
                    {
                        // Struct の場合、継承ができないから以下はありえないのだが、将来仕様変更されるか？されないと思う
                        var inheritsNode = node.ChildNodes().OfType<InheritsStatementSyntax>().FirstOrDefault();
                        var childNodes = inheritsNode.ChildNodes();

                        // Struct の場合、多重継承はできない仕様だが、将来仕様変更されるか？されないと思う
                        foreach (var childNode in childNodes)
                        {
                            // 継承元の型名
                            var typeName = childNode.ToString();

                            // クローズドジェネリック型の場合
                            var genericNode = childNode.ChildNodes().FirstOrDefault();
                            if (genericNode is Microsoft.CodeAnalysis.VisualBasic.Syntax.GenericNameSyntax)
                            {
                                var listNode = genericNode.ChildNodes().FirstOrDefault(); // TypeArgumentListSyntax
                                var genericTypes = listNode.ChildNodes();                 // PredefinedTypeSyntax, GenericNameSyntax, ...
                                typeName = $"{typeName}(Of {string.Join(", ", genericTypes)})";
                            }

                            nextModel.BaseTypes.Add(typeName);
                        }
                    }

                    if (hasImplements)
                    {
                        var implementsNode = node.ChildNodes().OfType<ImplementsStatementSyntax>().FirstOrDefault();
                        var childNodes = implementsNode.ChildNodes();

                        foreach (var childNode in childNodes)
                        {
                            // 継承元の型名
                            var typeName = childNode.ToString();

                            // クローズドジェネリック型の場合
                            var genericNode = childNode.ChildNodes().FirstOrDefault();
                            if (genericNode is Microsoft.CodeAnalysis.VisualBasic.Syntax.GenericNameSyntax)
                            {
                                var listNode = genericNode.ChildNodes().FirstOrDefault(); // TypeArgumentListSyntax
                                var genericTypes = listNode.ChildNodes();                 // PredefinedTypeSyntax, GenericNameSyntax, ...
                                typeName = $"{typeName}(Of {string.Join(", ", genericTypes)})";
                            }

                            nextModel.BaseTypes.Add(typeName);
                        }
                    }
                }
            }

            // Interface
            if (nextReferenceModel.DefinitionType == DefinitionTypes.Interface)
            {
                // 継承元クラス、またはインターフェースがある場合
                var node = nextReferenceModel.Tag as InterfaceBlockSyntax;
                var hasInherits = node.ChildNodes().OfType<InheritsStatementSyntax>().Any();
                var hasImplements = node.ChildNodes().OfType<ImplementsStatementSyntax>().Any();
                if (hasInherits || hasImplements)
                {
                    if (hasInherits)
                    {
                        var inheritsNode = node.ChildNodes().OfType<InheritsStatementSyntax>().FirstOrDefault();
                        var childNodes = inheritsNode.ChildNodes();

                        // Interface の場合、Inherits, IInterface1, IInterface2 などと記述する
                        // Implements ではなく Inherits でインターフェースを継承する
                        foreach (var childNode in childNodes)
                        {
                            // 継承元の型名
                            var typeName = childNode.ToString();

                            // クローズドジェネリック型の場合
                            var genericNode = childNode.ChildNodes().FirstOrDefault();
                            if (genericNode is Microsoft.CodeAnalysis.VisualBasic.Syntax.GenericNameSyntax)
                            {
                                var listNode = genericNode.ChildNodes().FirstOrDefault(); // TypeArgumentListSyntax
                                var genericTypes = listNode.ChildNodes();                 // PredefinedTypeSyntax, GenericNameSyntax, ...
                                typeName = $"{typeName}(Of {string.Join(", ", genericTypes)})";
                            }

                            nextModel.BaseTypes.Add(typeName);
                        }
                    }

                    if (hasImplements)
                    {
                        // 上記仕様から以下はありえないのだが、将来仕様変更されるか？されないと思う
                        var implementsNode = node.ChildNodes().OfType<ImplementsStatementSyntax>().FirstOrDefault();
                        var childNodes = implementsNode.ChildNodes();

                        foreach (var childNode in childNodes)
                        {
                            // 継承元の型名
                            var typeName = childNode.ToString();

                            // クローズドジェネリック型の場合
                            var genericNode = childNode.ChildNodes().FirstOrDefault();
                            if (genericNode is Microsoft.CodeAnalysis.VisualBasic.Syntax.GenericNameSyntax)
                            {
                                var listNode = genericNode.ChildNodes().FirstOrDefault(); // TypeArgumentListSyntax
                                var genericTypes = listNode.ChildNodes();                 // PredefinedTypeSyntax, GenericNameSyntax, ...
                                typeName = $"{typeName}(Of {string.Join(", ", genericTypes)})";
                            }

                            nextModel.BaseTypes.Add(typeName);
                        }
                    }
                }
            }
        }

        #endregion

        #region ソリューションノード（TreeViewItemModel） からプロジェクト間の参照ツリー（全体）作成

        public static List<DefinitionHeaderModel> CreateSolutionReferenceModels(TreeViewItemModel selectedModel)
        {
            var models = new List<DefinitionHeaderModel>();
            var solution = selectedModel.Tag as Solution;
            var projects = solution.Projects;
            var projectIDs = solution.ProjectIds;

            foreach (var project in projects)
                AddProjectTree(models, projects, projectIDs, project);

            return models;
        }

        private static void AddProjectTree(List<DefinitionHeaderModel> models, IEnumerable<Project> projects, IReadOnlyList<ProjectId> projectIDs, Project project, string relationID = "")
        {
            //
            var langType =
                (GetLanguageTypes(project) == LanguageTypes.CSharp) ? DefinitionTypes.CSharpProjectFile :
                (GetLanguageTypes(project) == LanguageTypes.VisualBasic) ? DefinitionTypes.VisualBasicProjectFile :
                DefinitionTypes.Unknown;

            var model = new DefinitionHeaderModel
            {
                ID = Guid.NewGuid().ToString(),
                RelationID = relationID,
                DefinitionName = project.Name,
                DefinitionType = langType,
                IsArrowDirectionEnd = true,
                IsExpanded = true,
            };
            models.Add(model);

            // アセンブリファイル名
            var assemblyNameHeaderModel = new TreeViewItemModel { Text = "アセンブリファイル名", IsExpanded = true };
            assemblyNameHeaderModel.Children.Add(new TreeViewItemModel { Text = Path.GetFileName(project.OutputFilePath) });
            model.MemberTreeItems.Add(assemblyNameHeaderModel);

            // 参照 dll
            var allRefDlls = project.MetadataReferences;
            var refDlls = allRefDlls.Where(x => x.Display != null && !x.Display.Contains(@"\packages\"));

            if (refDlls.Any())
            {
                var headerModel = new TreeViewItemModel
                {
                    Text = "参照",
                    DefinitionType = DefinitionTypes.Dependency,
                    IsExpanded = true,
                };
                model.MemberTreeItems.Add(headerModel);

                foreach (var refDll in refDlls)
                {
                    var memberModel = new TreeViewItemModel
                    {
                        Text = Path.GetFileNameWithoutExtension(refDll.Display),
                        DefinitionType = DefinitionTypes.Dependency,
                    };
                    headerModel.Children.Add(memberModel);
                }
            }

            // NuGet 参照 dll
            var nugetDlls = allRefDlls.Where(x => x.Display != null && x.Display.Contains(@"\packages\"));

            if (nugetDlls.Any())
            {
                var headerModel = new TreeViewItemModel
                {
                    Text = "NuGet 参照",
                    DefinitionType = DefinitionTypes.Dependency,
                    IsExpanded = true,
                };
                model.MemberTreeItems.Add(headerModel);

                foreach (var nugetDll in nugetDlls)
                {
                    var memberModel = new TreeViewItemModel
                    {
                        Text = Path.GetFileNameWithoutExtension(nugetDll.Display),
                        DefinitionType = DefinitionTypes.Dependency,
                    };
                    headerModel.Children.Add(memberModel);
                }
            }

            // プロジェクト参照 dll
            var projectDlls = project.ProjectReferences;

            if (projectDlls.Any())
            {
                foreach (var projectDll in projectDlls)
                {
                    var found = projects.First(x => x.Id == projectDll.ProjectId);
                    AddProjectTree(models, projects, projectIDs, found, model.ID);
                }
            }
        }

        #endregion

        #region プロジェクトノード（TreeViewItemModel） からプロジェクト間の参照ツリー（個別）作成

        public static List<DefinitionHeaderModel> CreateProjectReferenceModels(TreeViewItemModel selectedModel)
        {
            var models = new List<DefinitionHeaderModel>();
            var project = selectedModel.Tag as Project;
            var solution = project.Solution;
            var projects = solution.Projects;
            var projectIDs = solution.ProjectIds;

            AddProjectTree(models, projects, projectIDs, project);
            return models;
        }

        #endregion

        #region 指定のソースファイルをもとに、整形した一時ファイルのパスを返却

        public static string CreateFormattedFile(string sourceFile)
        {
            var extension = Path.GetExtension(sourceFile).ToLower();
            var code = ReadAllText(sourceFile);
            var formattedCode = string.Empty;

            if (extension == ".cs")
            {
                var tree = CSharpSyntaxTree.ParseText(code) as CSharpSyntaxTree;
                var root = tree.GetCompilationUnitRoot();
                var ws = new AdhocWorkspace();
                var result = Formatter.Format(root, ws);
                result = result.NormalizeWhitespace();
                formattedCode = result.ToString();
            }

            if (extension == ".vb")
            {
                var tree = VisualBasicSyntaxTree.ParseText(code) as VisualBasicSyntaxTree;
                var root = tree.GetCompilationUnitRoot();
                var ws = new AdhocWorkspace();
                var result = Formatter.Format(root, ws);
                result = result.NormalizeWhitespace();
                formattedCode = result.ToString();
            }

            var tempFile = Path.GetTempFileName();
            var renamedFile = $"{tempFile}{extension}";
            File.WriteAllText(renamedFile, formattedCode);

            return renamedFile;
        }


        #endregion

        #region ソースコードをフォーマットする

        /// <summary>
        /// 指定のソースファイルを読み込んで、フォーマット後のソースコードを返却します。
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <returns></returns>
        public static string FormatSourceFile(string sourceFile)
        {
            var code = ReadAllText(sourceFile);
            return FormatSourceCode(code);
        }

        /// <summary>
        /// 指定のソースコードをフォーマットして、フォーマット後のソースコードを返却します。
        /// </summary>
        /// <param name="sourceCode"></param>
        /// <returns></returns>
        public static string FormatSourceCode(string sourceCode)
        {
            var tree = CSharpSyntaxTree.ParseText(sourceCode);
            var root = tree.GetCompilationUnitRoot();
            var workspace = new AdhocWorkspace();

            var formatted = Formatter.Format(root, workspace);
            formatted = formatted.NormalizeWhitespace();

            return formatted.ToString();
        }

        #endregion

        #region Encoding

        /// <summary>
        /// 指定のファイルのエンコードを判別・返却します。
        /// </summary>
        /// <param name="targetFile"></param>
        /// <returns></returns>
        public static Encoding GetEncoding(string targetFile)
        {
            /*
             * how to get encoding
             * https://github.com/hnx8/ReadJEnc/blob/master/ReadJEnc_Readme.txt
             * 
             */

            var fi = new FileInfo(targetFile);
            using (var reader = new FileReader(fi))
            {
                var code = reader.Read(fi);
                var encode = code.GetEncoding();
                return encode;
            }
        }

        /// <summary>
        /// 指定のファイルの内容を読み込んで、返却します。
        /// </summary>
        /// <param name="targetFile"></param>
        /// <returns></returns>
        public static string ReadAllText(string targetFile) => File.ReadAllText(targetFile, GetEncoding(targetFile));

        #endregion

    }
}
