namespace DotResolution.Libraries
{
    /// <summary>
    /// 定義の種類を表す列挙体です。
    /// </summary>
    public enum DefinitionTypes
    {
        None,

        Folder,

        SolutionFile,

        CSharpProjectFile,
        VisualBasicProjectFile,

        CSharpSourceFileForHeader,
        VisualBasicSourceFileForHeader,

        CSharpSourceFile,
        VisualBasicSourceFile,

        GeneratedFile,
        Dependency,

        Namespace,
        Class,
        Struct,
        Interface,
        Module,

        Field,
        Indexer,
        Property,
        Constructor,
        WindowsAPI,
        Operator,
        EventHandler,
        Method,
        Event,
        Delegate,
        Enum,
        EnumItem,

        Unknown,
    }
}
