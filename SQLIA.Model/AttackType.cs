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
    
    public partial class AttackType
    {
        public AttackType()
        {
            this.LiteralsAttackTypes = new HashSet<LiteralsAttackType>();
            this.ScanEntryPossibleAttackTypes = new HashSet<ScanEntryPossibleAttackType>();
        }
    
        public int AttackTypeID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Implemented { get; set; }
    
        public virtual ICollection<LiteralsAttackType> LiteralsAttackTypes { get; set; }
        public virtual ICollection<ScanEntryPossibleAttackType> ScanEntryPossibleAttackTypes { get; set; }
    }
}
