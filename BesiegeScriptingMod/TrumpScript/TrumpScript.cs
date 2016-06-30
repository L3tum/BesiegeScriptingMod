using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BesiegeScriptingMod.TrumpScript
{
    class TrumpScript
    {
        private List<String> pattern = new List<string>();
        private List<String> replacements = new List<string>();
        private Allowed allowed = new Allowed();
        private String sauce;

        public String Convert(String sauce)
        {
            this.sauce = sauce;

            //make sure ends with "America is great"
            if (!sauce.EndsWith("America is great"))
            {
                UnityEngine.Debug.LogError(allowed.Errors["freedom"]);
                return null;
            }
            foreach (string s in sauce.Split(new []{" ", "\r\n", "\r", "\n", "\""}, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!allowed.ALLOWED.Contains(s.ToLower()) && !allowed.trump.Contains(s.ToLower()) && !allowed.vocabulary.Contains(s.ToLower()))
                {
                    UnityEngine.Debug.LogError(allowed.Errors["badword"] + Util.Util.getNewLine() + "Bad word: " + s);
                    return null;
                }
            }
            sauce = this.sauce.Remove(sauce.Length - 17);
            foreach (Match match in Regex.Matches(sauce, "[0-9]"))
            {
                if (int.Parse(match.Value) > 1000000)
                {
                    sauce = sauce.Replace(match.Value, (int.Parse(match.Value) - 1000000).ToString());
                }
                else if (int.Parse(match.Value) < 1000000)
                {
                    UnityEngine.Debug.LogError(allowed.Errors["too_small"]);
                    return null;
                }
            }

            #region patterns

            pattern.Add(@" plus "); //Addition
            pattern.Add(@" minus "); //Minus
            pattern.Add(@" times "); //Multiply
            pattern.Add(@" over "); //Division
            pattern.Add(@" less | fewer | smaller "); //Less than
            pattern.Add(@" more | greater| larger "); //More than
            pattern.Add(@","); //( respectively , in TS
            pattern.Add(@";"); //) respectively ; in TS
            pattern.Add(@":"); //{
            pattern.Add(@"!"); //}
            pattern.Add(@"(is|are) (""?[a-zA-Z0-9]+""?)\?"); //Equality
            pattern.Add(@"is|are"); //Assignment No. 1
            pattern.Add(@"make |Make ([a-zA-Z0-9]+) ([a-zA-Z0-9]+)"); //Assignment No.2
            pattern.Add(@"(tell|say) (""?[a-zA-Z0-9 ?]+""?)"); //Debug.Log basically
            pattern.Add(@"as long as"); //while
            pattern.Add(@"[^{};](" + Util.Util.getNewLineInString(sauce) + @")");
            pattern.Add(@"fact");
            pattern.Add(@"lie");
            pattern.Add(@"I am ([a-zA-Z]+)ing now");

            #endregion

            #region replacements

            replacements.Add("+");
            replacements.Add("-");
            replacements.Add("*");
            replacements.Add("/");
            replacements.Add("[");
            replacements.Add("]");
            replacements.Add("(");
            replacements.Add(")");
            replacements.Add("{");
            replacements.Add("}");
            replacements.Add("== $2;");
            replacements.Add("=");
            replacements.Add("$1 = $2;");
            replacements.Add("Debug.Log($2);");
            replacements.Add("while");
            replacements.Add(";$1");
            replacements.Add("true");
            replacements.Add("false");
            replacements.Add("public void $1");

            #endregion

            for (int i = 0; i < pattern.Count; i++)
            {
                sauce = Regex.Replace(sauce, pattern[i], replacements[i]);
            }

            return sauce;
        }
    }
}
                                