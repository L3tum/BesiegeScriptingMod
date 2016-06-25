using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BesiegeScriptingMod
{
    public class Script
    {
        public String sourceFile;
        public String refFile;

        public Script(String source, String refs)
        {
            sourceFile = source;
            refFile = refs;
        }
    }
}
