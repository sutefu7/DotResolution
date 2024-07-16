using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

// 各メンバーチェック

namespace ConsoleApp2
{
    /// <summary>
    /// Class2 です。
    /// </summary>
    internal class Class2
    {
        /// <summary>
        /// Aaa です。
        /// </summary>
        public enum Aaa
        {
            B1,
            B2,
            B3
        }

        /// <summary>
        /// AaaDelegate です。
        /// </summary>
        /// <param name="aa"></param>
        public delegate void AaaDelegate(Aaa aa);

        /// <summary>
        /// Bbb です。
        /// </summary>
        public event EventHandler Bbb;

        /// <summary>
        /// age です。
        /// </summary>
        private int age = 0;

        /// <summary>
        /// i1, i2, i3 です。
        /// </summary>
        private int i1, i2, i3 = 0;

        /// <summary>
        /// インデクサーです。
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int this[int index]
        {
            get { return 0; }
            set { }
        }

        /// <summary>
        /// Age です。
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        public Class2() { }

        /// <summary>
        /// SetData() です。
        /// </summary>
        public void SetData() { }

        /// <summary>
        /// GetAge() です。
        /// </summary>
        /// <returns></returns>
        public int GetAge() { return age; }

        public class Class2A
        {
            public int Age { get; set; }

            /// <summary>
            /// Class2A の + オペレーターです。
            /// </summary>
            /// <param name="self"></param>
            /// <param name="other"></param>
            /// <returns></returns>
            public static Class2A operator +(Class2A self, Class2A other)
            {
                return new Class2A { Age = self.Age + other.Age };
            }
        }

        internal static class NativeMethods
        {
            /// <summary>
            /// SetWindowText です。
            /// </summary>
            /// <param name="hWnd"></param>
            /// <param name="lpString"></param>
            /// <returns></returns>
            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            public static extern int SetWindowText(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)] string lpString);
        }


    }
}
