using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLIA.Model
{
    public class LiteralHelper
    {
        public static IEnumerable<Literal> GetLiterals()
        {
            var db = new SQLLiteralsEntities();
            return db.Literals;
        }
    }
}
