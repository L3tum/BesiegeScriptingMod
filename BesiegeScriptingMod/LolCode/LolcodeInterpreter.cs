using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Metadata;
using UnityEngine;

namespace BesiegeScriptingMod.LolCode
{
    class LolcodeInterpreter
    {
        public String Sauce = "";

        public LolcodeInterpreter(String Sauce)
        {
            this.Sauce = Sauce;
        }

        public String convert()
        {
            String[] sauce2 = Sauce.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
            sauce2 = Replace(sauce2);
            return Sauce;
        }

        private String[] Replace(String[] sauce)
        {
            for (int i = 0; i < sauce.Length; i++)
            {
                if (sauce[i].Equals(","))
                {
                    if (sauce[i + 1].Equals("BTW") || sauce[i+1].Equals("OBTW") || sauce[i+1].Equals("TLDR"))
                    {
                        sauce[i] = "";
                    }
                }
                else if (sauce[i].Equals("BTW"))
                {
                    sauce[i] = "//";
                }
                else if (sauce[i].Equals("OBTW"))
                {
                    sauce[i] = "/*";
                }
                else if (sauce[i].Equals("TLDR"))
                {
                    sauce[i] = "*/";
                }
                else if (sauce[i].Equals("I") && sauce[i + 1].Equals("HAS") && sauce[i + 2].Equals("A"))
                {
                    if (sauce[i + 4].Equals("ITZ"))
                    {
                        sauce[i] = "var " + sauce[i + 3] + "= " + sauce[i + 5] + ";";
                        sauce[i + 1] = String.Empty;
                        sauce[i + 2] = String.Empty;
                        sauce[i + 3] = String.Empty;
                        sauce[i + 4] = String.Empty;
                        sauce[i + 5] = String.Empty;
                    }
                    else
                    {
                        sauce[i] = "var " + sauce[i + 3] + ";";
                        sauce[i + 1] = String.Empty;
                        sauce[i + 2] = String.Empty;
                        sauce[i + 3] = String.Empty;
                    }
                }
                else if (sauce[i].Equals("R"))
                {
                    sauce[i - 1] = sauce[i - 1] + " = " + sauce[i + 1] + ";";
                    sauce[i] = string.Empty;
                    sauce[i + 1] = String.Empty;
                }
                else if (sauce[i].Equals("WIN"))
                {
                    sauce[i] = "true";
                }
                else if (sauce[i].Equals("FAIL"))
                {
                    sauce[i] = "false";
                }
            }
            return sauce;
        }
    }
}
