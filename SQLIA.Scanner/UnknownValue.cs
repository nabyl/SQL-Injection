using System;
using System.Collections.Generic;
using System.Text;

namespace antiSQLInjection
{
    class UnknownValue
    {
        public String toString()
        {
            return "unknown value";
        }

        public String AsText
        {
            get { return this.toString(); }
        }
    }
}
