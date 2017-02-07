#region usings

using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Scripting;

#endregion

namespace BesiegeScriptingMod.LolCode
{
    internal class LolcodeInterpreter
    {
        public List<string> pattern = new List<string>();
        public List<string> replacements = new List<string>();
        public string Sauce = "";

        public LolcodeInterpreter(string Sauce)
        {
            this.Sauce = Sauce;
        }

        public string convert()
        {
            if (!Sauce.StartsWith("Hai 1.2"))
            {
                return new SyntaxErrorException("No 'Hai 1.2' starting command!").ToString();
            }
            Sauce = Sauce.Remove(0, 7);
            if (!Sauce.EndsWith("KTHXBYE"))
            {
                return new SyntaxErrorException("No KTHXBYE ending command!").ToString();
            }
            Sauce = Sauce.Remove(Sauce.Length - 8);

            #region Pattern

            pattern.Add(@"I HAS A ([a-zA-Z]+)( ITZ )([a-zA-Z0-9\.]+)"); //Variable declaration and init
            pattern.Add(@"I HAS A ([a-zA-Z]+)"); //Only declaration
            pattern.Add(@"([a-zA-Z]+) R (""?[a - zA - Z0 - 9\.] + ""?)"); //only init
            pattern.Add(@",? *	*BTW"); //One line comment
            pattern.Add(@",? *	*OBTW"); //Multi line comment 1.
            pattern.Add(@",? *	*TLDR"); //Multi line comment 2.
            pattern.Add(@"WIN"); //truec
            pattern.Add(@"FAIL"); //false
            pattern.Add(@","); //Line breaks
            pattern.Add(@"YARN"); //String
            pattern.Add(@"NUMBR"); //int
            pattern.Add(@"NUMBAR"); //float
            pattern.Add(@"TROOF"); //bool
            pattern.Add(@"NOOB"); //untyped
            pattern.Add(@"(""[a-zA-Z0-9.]*)(:\))([a-zA-Z0-9.]*"")"); //newline in String
            pattern.Add(@"(""[a-zA-Z0-9.]*)(:\>)([a-zA-Z0-9.]*"")"); //tab in String
            pattern.Add(@"(""[a-zA-Z0-9.]*)(:o)([a-zA-Z0-9.]*"")"); //beep in String
            pattern.Add(@"(""[a-zA-Z0-9.]*)(:"")([a-zA-Z0-9.]*"")"); //literal quote in String
            pattern.Add(@"(""[a-zA-Z0-9.]*)(::)([a-zA-Z0-9.]*"")"); //: in String
            pattern.Add(Util.Util.getNewLineInString(Sauce) + @"? *   *(\.\.\.) ?" + Util.Util.getNewLineInString(Sauce) + @" *   *([a-zA-Z0-9 .]+)"); //Line continuation
            pattern.Add(@"[^{};](" + Util.Util.getNewLineInString(Sauce) + @")"); //obvsl new line

            #endregion

            #region replacements

            replacements.Add(@"var $1 = $2;" + Util.Util.getNewLine());
            replacements.Add(@"var $1;" + Util.Util.getNewLine());
            replacements.Add(@"$1 = $2;" + Util.Util.getNewLine());
            replacements.Add(@"//");
            replacements.Add(@"/\*");
            replacements.Add(@"\*/");
            replacements.Add(@"true");
            replacements.Add(@"false");
            replacements.Add(Util.Util.getNewLine());
            replacements.Add(@"$2");
            replacements.Add(@";" + Util.Util.getNewLine());

            #endregion

            for (int i = 0; i < pattern.Count; i++)
            {
                Sauce = Regex.Replace(Sauce, pattern[i], replacements[i]);
            }

            return Sauce;
        }
    }
}