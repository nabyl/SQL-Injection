using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLIA.Model
{
    public class AttackTypeHelper
    {
        public static IQueryable<AttackType> GetAttackTypes()
        {
            var db = new SQLLiteralsEntities();
            return db.AttackTypes;

        }
    }
}
