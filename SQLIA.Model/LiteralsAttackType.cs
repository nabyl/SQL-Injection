//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SQLIA.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class LiteralsAttackType
    {
        public int LiiteralAttackTypeID { get; set; }
        public int LiteralID { get; set; }
        public int AttackTypeID { get; set; }
    
        public virtual AttackType AttackType { get; set; }
        public virtual Literal Literal { get; set; }
    }
}
