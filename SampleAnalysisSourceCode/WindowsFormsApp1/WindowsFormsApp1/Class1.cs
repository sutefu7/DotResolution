using ClassLibrary2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    /// <summary>
    /// Class1 です。
    /// </summary>
    public class Class1 : Interface1, Interface3
    {
        /// <summary>
        /// WindowsFormsApp1_Interface1 です。
        /// </summary>
        public string WindowsFormsApp1_Interface1 { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <summary>
        /// ClassLibrary1_Interface2 です。
        /// </summary>
        public string ClassLibrary1_Interface2 { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <summary>
        /// ClassLibrary2_Interface3 です。
        /// </summary>
        public string ClassLibrary2_Interface3 { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
