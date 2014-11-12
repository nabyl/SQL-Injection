using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLIA.Model
{
    [MetadataType(typeof(LiteralMD))]
    public partial class Literal
    {
        private class LiteralMD
        {
            [Required]
            [Display(Name="SQL String")]
            public string Word { get; set; }
        }
    }
}
