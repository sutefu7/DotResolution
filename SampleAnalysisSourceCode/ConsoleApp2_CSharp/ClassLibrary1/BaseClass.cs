using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1
{
    /// <summary>
    /// ClassLibrary1 の BaseClass です。
    /// </summary>
    public class BaseClass : Interface1, Interface2, Interface3, Interface4
    {
        /// <summary>
        /// ID です。
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ClassLibrary1_Interface1_Name です。
        /// </summary>
        public virtual string ClassLibrary1_Interface1_Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <summary>
        /// ClassLibrary1_Interface2_Name です。
        /// </summary>
        public string ClassLibrary1_Interface2_Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <summary>
        /// ClassLibrary1_Interface3_Name です。
        /// </summary>
        public string ClassLibrary1_Interface3_Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <summary>
        /// ClassLibrary1_Interface4_Name です。
        /// </summary>
        public string ClassLibrary1_Interface4_Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
