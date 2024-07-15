using DotResolution.Views;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace DotResolution.Libraries
{
    /// <summary>
    /// アプリケーション環境を管理するクラスです。
    /// </summary>
    public static class AppEnv
    {
        #region ファイル、フォルダのパス

        /// <summary>
        /// 実行ファイルのパス を返却します。
        /// </summary>
        public static string ExeFile
        {
            get
            {
                if (string.IsNullOrEmpty(_ExeFile))
                {
                    _ExeFile = Assembly.GetEntryAssembly().Location;
                }

                return _ExeFile;
            }
        }

        private static string _ExeFile;

        /// <summary>
        /// 実行ファイルがあるフォルダパス を返却します。
        /// </summary>
        public static string ExeFolder
        {
            get
            {
                if (string.IsNullOrEmpty(_ExeFolder))
                    _ExeFolder = Path.GetDirectoryName(ExeFile);

                return _ExeFolder;
            }
        }

        private static string _ExeFolder = string.Empty;

        /// <summary>
        /// ログファイルのパス を返却します。
        /// </summary>
        public static string LogFile
        {
            get
            {
                if (string.IsNullOrEmpty(_LogFile))
                {
                    var logName = Path.GetFileNameWithoutExtension(ExeFile);
                    _LogFile = Path.Combine(ExeFolder, $"{logName}.log");
                }

                return _LogFile;
            }
        }

        private static string _LogFile = string.Empty;

        #endregion

        #region Roslyn

        /// <summary>
        /// 扱っているソースコードのプログラミング言語の種類 を取得、または設定します。
        /// </summary>
        /// <remarks>
        /// プログラミング言語の判定をしたい時に、利用したい直前で更新する必要があります。<br></br>
        /// 扱い方に注意しないと誤判定のもとになってしまいます。
        /// </remarks>
        public static LanguageTypes LanguageType { get; set; } = LanguageTypes.CSharp;

        /// <summary>
        /// C# 用の構文木のコレクション を取得、または設定します。
        /// </summary>
        /// <remarks>
        /// C# のソースコードを Roslyn に読み込ませて、分析・生成した構文木です。
        /// </remarks>
        public static List<SyntaxTree> CSharpSyntaxTrees { get; set; } = new List<SyntaxTree>();

        /// <summary>
        /// Visual Basic 用の構文木のコレクション を取得、または設定します。
        /// </summary>
        /// <remarks>
        /// Visual Basic のソースコードを Roslyn に読み込ませて、分析・生成した構文木です。
        /// </remarks>
        public static List<SyntaxTree> VisualBasicSyntaxTrees { get; set; } = new List<SyntaxTree>();

        /// <summary>
        /// 読み込んだソースコードをもとに作成した、C# 用のコンパイラ を取得、または設定します。
        /// </summary>
        /// <remarks>
        /// アプリケーション実行中のみ動作する、メモリ上に作成したコンパイラです。
        /// </remarks>
        public static List<CSharpCompilation> CSharpCompilations { get; set; } = new List<CSharpCompilation>();

        /// <summary>
        /// 読み込んだソースコードをもとに作成した、Visual Basic 用のコンパイラ を取得、または設定します。
        /// </summary>
        /// <remarks>
        /// アプリケーション実行中のみ動作する、メモリ上に作成したコンパイラです。
        /// </remarks>
        public static List<VisualBasicCompilation> VisualBasicCompilations { get; set; } = new List<VisualBasicCompilation>();

        #endregion

        #region 各画面の横断用

        public static MainView MainView { get; set; } = null;

        #endregion

    }
}
