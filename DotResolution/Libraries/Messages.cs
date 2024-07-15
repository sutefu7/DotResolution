using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotResolution.Libraries
{
    /// <summary>
    /// メッセージボックスクラスです。
    /// </summary>
    public class Messages
    {
        /// <summary>
        /// 情報メッセージボックスを表示します。
        /// </summary>
        /// <param name="text"></param>
        /// <param name="caption"></param>
        /// <returns></returns>
        public static DialogResult Information(string text, string caption = "インフォメーション") =>
            Internal(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);

        /// <summary>
        /// 警告メッセージボックスを表示します。
        /// </summary>
        /// <param name="text"></param>
        /// <param name="caption"></param>
        /// <returns></returns>
        public static DialogResult Warning(string text, string caption = "ワーニング") =>
            Internal(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);

        /// <summary>
        /// エラーメッセージボックスを表示します。
        /// </summary>
        /// <param name="text"></param>
        /// <param name="caption"></param>
        /// <returns></returns>
        public static DialogResult Error(string text, string caption = "エラー") =>
            Internal(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);

        /// <summary>
        /// 確認メッセージボックスを表示します。
        /// </summary>
        /// <param name="text"></param>
        /// <param name="caption"></param>
        /// <param name="defaultButton"></param>
        /// <returns></returns>
        public static DialogResult QuestionOKCancel(string text, string caption = "確認", MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1) =>
            Internal(text, caption, MessageBoxButtons.OKCancel, MessageBoxIcon.Question, defaultButton);

        /// <summary>
        /// 確認メッセージボックスを表示します。
        /// </summary>
        /// <param name="text"></param>
        /// <param name="caption"></param>
        /// <param name="defaultButton"></param>
        /// <returns></returns>
        public static DialogResult QuestionYesNo(string text, string caption = "確認", MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1) =>
            Internal(text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question, defaultButton);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="caption"></param>
        /// <param name="selector"></param>
        /// <param name="icon"></param>
        /// <param name="defaultSelector"></param>
        /// <returns></returns>
        private static DialogResult Internal(string text, string caption, MessageBoxButtons selector, MessageBoxIcon icon, MessageBoxDefaultButton defaultSelector) =>
             MessageBox.Show(text, caption, selector, icon, defaultSelector);





        // メッセージボックスが最前面で表示されない場合は、以下オーナーを渡して呼び出してみる

        /// <summary>
        /// 情報メッセージボックスを表示します。
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="text"></param>
        /// <param name="caption"></param>
        /// <returns></returns>
        public static DialogResult Information(IWin32Window owner, string text, string caption = "インフォメーション") =>
            Internal(owner, text, caption, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);

        /// <summary>
        /// 警告メッセージボックスを表示します。
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="text"></param>
        /// <param name="caption"></param>
        /// <returns></returns>
        public static DialogResult Warning(IWin32Window owner, string text, string caption = "ワーニング") =>
            Internal(owner, text, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);

        /// <summary>
        /// エラーメッセージボックスを表示します。
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="text"></param>
        /// <param name="caption"></param>
        /// <returns></returns>
        public static DialogResult Error(IWin32Window owner, string text, string caption = "エラー") =>
            Internal(owner, text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);

        /// <summary>
        /// 確認メッセージボックスを表示します。
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="text"></param>
        /// <param name="caption"></param>
        /// <param name="defaultButton"></param>
        /// <returns></returns>
        public static DialogResult QuestionOKCancel(IWin32Window owner, string text, string caption = "確認", MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1) =>
            Internal(owner, text, caption, MessageBoxButtons.OKCancel, MessageBoxIcon.Question, defaultButton);

        /// <summary>
        /// 確認メッセージボックスを表示します。
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="text"></param>
        /// <param name="caption"></param>
        /// <param name="defaultButton"></param>
        /// <returns></returns>
        public static DialogResult QuestionYesNo(IWin32Window owner, string text, string caption = "確認", MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1) =>
            Internal(owner, text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question, defaultButton);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="text"></param>
        /// <param name="caption"></param>
        /// <param name="selector"></param>
        /// <param name="icon"></param>
        /// <param name="defaultSelector"></param>
        /// <returns></returns>
        private static DialogResult Internal(IWin32Window owner, string text, string caption, MessageBoxButtons selector, MessageBoxIcon icon, MessageBoxDefaultButton defaultSelector) =>
             MessageBox.Show(owner, text, caption, selector, icon, defaultSelector);
    }
}
