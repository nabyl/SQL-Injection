using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLIA.Model
{
    public class ModelConstants
    {
        public static class AttackVectorTypes
        {
            public const int UNION = 1;
            public const int BOOLEAN = 2;
            public const int ERROR_BASED = 3;
            public const int OUT_OF_BAND = 4;
            public const int TIME_DELAY = 5;
            public const int STORED_PROCEDURE = 6;
            public const int COMMENTS = 7;
            public const int PIGGY_BACK_QUERIES = 8;
            public const int NOT_ALLOWED_QUERY = 9;
        }
    }
}
