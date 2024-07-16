using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary2
{
    /// <summary>
    /// ClassLibrary2 の BaseClass です。
    /// </summary>
    public class BaseClass : Interface1, Interface2, Interface3, Interface4
    {
        /// <summary>
        /// ID です。
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ClassLibrary2_Interface1_Name です。
        /// </summary>
        public virtual string ClassLibrary2_Interface1_Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <summary>
        /// ClassLibrary2_Interface2_Name です。
        /// </summary>
        public string ClassLibrary2_Interface2_Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <summary>
        /// ClassLibrary2_Interface3_Name です。
        /// </summary>
        public string ClassLibrary2_Interface3_Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <summary>
        /// ClassLibrary2_Interface4_Name です。
        /// </summary>
        public string ClassLibrary2_Interface4_Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
