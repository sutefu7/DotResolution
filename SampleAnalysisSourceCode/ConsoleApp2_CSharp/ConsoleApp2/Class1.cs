using ClassLibrary1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xxx = ClassLibrary1.BaseClass;
using Yyy = ClassLibrary1;

// コメント、ドキュメントコメント、継承のチェック

namespace ConsoleApp2
{
    /// <summary>
    /// ConsoleApp2 の Class1 です。
    /// </summary>
    internal class Class1 : BaseClass, Interface1
    {
        /// <summary>
        /// Name です。
        /// </summary>
        public override string ClassLibrary1_Interface1_Name { get; set; }
    }

    // Class1A です。1
    /// <summary>
    /// Class1A です。2
    /// </summary>
    internal class Class1A { }

    /// <summary>
    /// Class1B です。1
    /// </summary>
    // Class1B です。2
    internal class Class1B : Class1A { }

    // Class1C です。
    internal class Class1C : Xxx { }

    internal class Class1D : Yyy.BaseClass { }

    internal class Class1E : global::ClassLibrary1.BaseClass { }
}
