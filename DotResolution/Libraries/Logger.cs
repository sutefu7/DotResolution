using System;
using System.IO;

namespace DotResolution.Libraries
{
    /// <summary>
    /// ロガークラスです。
    /// </summary>
    public class Logger
    {
        /// <summary>
        /// ログファイルに追記します。
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="s"></param>
        public static void AppendAllText(Exception ex, string s = null)
        {
            if (!string.IsNullOrWhiteSpace(s))
                AppendAllText(s);

            AppendAllText(ex.ToString());
        }

        /// <summary>
        /// ログファイルに追記します。
        /// </summary>
        /// <param name="s"></param>
        public static void AppendAllText(string s) => File.AppendAllText(AppEnv.LogFile, $"[{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff")}] {s}{Environment.NewLine}");
    }
}
