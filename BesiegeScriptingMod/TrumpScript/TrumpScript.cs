using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BesiegeScriptingMod.TrumpScript
{
    class TrumpScript
    {
        private List<String> pattern = new List<string>();
        private List<String> replacements = new List<string>(); 
        private Allowed allowed = new Allowed();
        private String sauce;

        public void Init(String sauce)
        {
            this.sauce = sauce;

            //make sure ends with "America is great"
            if (!sauce.EndsWith("America is great"))
            {
                UnityEngine.Debug.LogError(allowed.Errors["freedom"]);
            }
            else
            {
                #region patterns
                pattern.Add("[0-9]"); //Numbers
                pattern.Add("<plus>"); //Addition
                pattern.Add("<minus>"); //Minus
                pattern.Add("<times>"); //Multiply
                pattern.Add("<over>"); //Division
                pattern.Add("<less|fewer|smaller>"); //Less than
                pattern.Add("<more|greater|larger>"); //More than
                pattern.Add("<,>"); //( respectively , in TS
                pattern.Add("<;>"); //) respectively ; in TS
                pattern.Add("<:>"); //{
                pattern.Add("<!>"); //}
                pattern.Add("<is|are> \"?[a-zA-Z0-9]+\"?<?>"); //Equality
                pattern.Add("<is|are>"); //Assignment No. 1
                pattern.Add("<make|Make> [a-zA-Z0-9]+ [a-zA-Z0-9]+"); //Assignment No.2
                pattern.Add("<tell|say> [a-zA-Z0-9]+"); //Debug.Log basically
                pattern.Add("<as long as>"); //while
                #endregion
            }
        }
    }
}
