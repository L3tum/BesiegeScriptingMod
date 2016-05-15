using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using ICSharpCode.PythonBinding;
using Debug = UnityEngine.Debug;

namespace BesiegeScriptingMod
{
    public static class Util
    {
        /// <summary>
        /// Converts the C# Source into Python
        /// </summary>
        /// <param name="sauce">Source code written by $user</param>
        /// <param name="refs">References specified by $user</param>
        /// <param name="name">$name of the script</param>
        /// <returns>Source code in Python format</returns>
        public static String Compile(FileInfo sauce, string[] refs, string name)
        {
            NRefactoryToPythonConverter indentString = NRefactoryToPythonConverter.Create(sauce.FullName);
            indentString.IndentString = new string(' ', 2);
            string str = "";
            using (var tr = sauce.OpenText())
            {
                str = indentString.Convert(tr.ReadToEnd());
            }
            FileInfo fi = new FileInfo(Application.dataPath + "/Mods/Scripts/CSharpScripts/" + name + ".temp.py");
            using (TextWriter tw = fi.CreateText())
            {
                tw.Write(str);
            }
            return Application.dataPath + "/Mods/Scripts/CSharpScripts/" + name + ".temp.py";

            #region alternative[OBSOLETE]

            /*
            String refss = "";
            foreach (var @ref in refs)
            {
                refss += ":\"" + @ref + "\"";
            }

            String args = Application.dataPath + "/Mods/Scripts/Resource/cs-script/cscs.exe /cd /r" + refss + sauce.FullName;
            if (Application.platform.Equals(RuntimePlatform.WindowsPlayer) ||
                Application.platform.Equals(RuntimePlatform.WindowsEditor))
            {
                var process = new Process();
                var startInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "cmd.exe",
                    Arguments = args
                };
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
            }
            else if (Application.platform.Equals(RuntimePlatform.LinuxPlayer) ||
                     Application.platform.Equals(RuntimePlatform.OSXEditor) ||
                     Application.platform.Equals(RuntimePlatform.OSXPlayer))
            {
                var psi = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "/bin/bash.sh",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    Arguments = args
                };

                var p = Process.Start(psi);
                p.WaitForExit();
            }
            */

            #endregion
        }

        /// <summary>
        /// Wraps methods written by $user in a class with the specified $name
        /// </summary>
        /// <param name="sauce">The source code written by $user</param>
        /// <param name="name">The name of the script and class specified by $user</param>
        /// <param name="refs">The References for this Script</param>
        /// <returns>Source Code wrapped into a class</returns>
        public static String getMethodsWithClass(String sauce, String name, String[] refs)
        {
            String finalSauce = "";
            String usings = "";
            String[] lines = splitStringAtNewline(sauce);
            Dictionary<String, IEnumerable<String>> nss = new Dictionary<string, IEnumerable<String>>();
            foreach (string @ref in refs)
            {
                String reff = @ref.Trim();
                Assembly assembly = Assembly.LoadFrom(@ref);
                var namespaces = assembly.GetTypes()
                        .Select(t => t.Namespace)
                        .Distinct();
                nss.Add(@ref, namespaces);
            }
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("using"))
                {
                    if (lines[i].EndsWith(";"))
                    {
                        usings += lines[i] + getNewLine();
                        lines[i] = String.Empty;
                    }
                }
                if (lines[i].Contains("using") || String.IsNullOrEmpty(lines[i])) continue;
                foreach (string ns in from key in nss.Keys from ns in nss[key] where !String.IsNullOrEmpty(ns) where lines[i].Contains((ns + ".")) select ns)
                {
                    lines[i] = Regex.Replace(lines[i], ns + ".", string.Empty);
                }
                lines[i] = lines[i] + getNewLine();
            }
            lines = lines.Where(x => !string.IsNullOrEmpty(x) && !x.Equals("\r\n") && !x.Equals("\r") && !x.Equals("\n") && !String.IsNullOrEmpty(x.Trim())).ToArray();
            sauce = String.Concat(lines);
            if (!usings.Contains("using UnityEngine;"))
            {
                usings += "using UnityEngine;" + getNewLine();
            }
            finalSauce += usings;
            finalSauce += @"public class " + name + " : MonoBehaviour{" + getNewLine() + sauce + getNewLine() + "}";
            return finalSauce;
        }

        /// <summary>
        /// Wraps methods written by $user in a clas with the specified $name
        /// Special Case for Python Source Code
        /// </summary>
        /// <param name="sauce">The source code written by $use</param>
        /// <param name="name">The name of the script and class specified by $user</param>
        /// <param name="refs">The References for this Script</param>
        /// <returns>Source Code wrapped into a class</returns>
        public static String getMethodsWithClassPython(String sauce, String name, String[] refs)
        {
            String finalSauce = "";
            String imports = "";
            String[] lines = splitStringAtNewline(sauce);

            Dictionary<String, IEnumerable<String>> nss = new Dictionary<string, IEnumerable<String>>();
            foreach (string @ref in refs)
            {
                String reff = @ref.Trim();
                Assembly assembly = Assembly.LoadFrom(@ref);
                var namespaces = assembly.GetTypes()
                        .Select(t => t.Namespace)
                        .Distinct();
                nss.Add(@ref, namespaces);
            }

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("import"))
                {
                    if (!lines[i].Contains(";") && !lines[i].Contains(":"))
                    {
                        imports += lines[i] + getNewLine();
                        lines[i] = String.Empty;
                    }
                    else
                    {
                        UnityEngine.Debug.LogError(
                            "Please make sure to write each import and the source code on separate lines!");
                    }
                }
                else
                {
                    lines[i] = @"  " + lines[i] + getNewLine();
                }
                if (lines[i].Contains("import") || String.IsNullOrEmpty(lines[i])) continue;
                foreach (string ns in from key in nss.Keys from ns in nss[key] where !String.IsNullOrEmpty(ns) where lines[i].Contains((ns + ".")) select ns)
                {
                    lines[i] = Regex.Replace(lines[i], ns + ".", string.Empty);
                }
            }
            sauce = String.Concat(lines);
            if (!imports.Contains("from UnityEngine import *"))
            {
                imports += "from UnityEngine import *" + getNewLine();
            }
            finalSauce += imports;
            finalSauce += @"class " + name + "(MonoBehaviour):" + getNewLine() + sauce;
            return finalSauce;
        }

        /// <summary>
        /// Splits the String at the new line parameters for each OS
        /// </summary>
        /// <param name="sauce">Source String to split</param>
        /// <returns>String array of splitted source</returns>
        public static String[] splitStringAtNewline(String sauce)
        {
            return sauce.Split(new[] {"\r\n", "\r", "\n"}, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Splits the String at new line and space chararacters for each OS
        /// </summary>
        /// <param name="sauce">Source String to split</param>
        /// <returns>String array of splitted source</returns>
        public static String[] splitStringAtNewlineAndSpace(String sauce)
        {
            return sauce.Split(new[] { " ", "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Gives a new line for the specific OS
        /// </summary>
        /// <returns>New Line Character for OS</returns>
        public static String getNewLine()
        {
            if (Application.platform.Equals(RuntimePlatform.WindowsEditor) ||
                Application.platform.Equals(RuntimePlatform.WindowsPlayer))
            {
                return "\r\n";
            }
            else if (Application.platform.Equals(RuntimePlatform.OSXPlayer) ||
                     Application.platform.Equals(RuntimePlatform.OSXDashboardPlayer) ||
                     Application.platform.Equals(RuntimePlatform.OSXEditor) || Application.platform.Equals(RuntimePlatform.LinuxPlayer))
            {
                return "\n";
            }
            return "\r";
        }

        /// <summary>
        /// Gives a new line specifically for a Script. Should enable easier cross-plattform sharing of Scripts
        /// </summary>
        /// <param name="sauce">Source Code</param>
        /// <returns>New-Line-Character</returns>
        public static String getNewLineInString(String sauce)
        {
            if (sauce.Contains("\r\n"))
            {
                return "\r\n";
            }
            else if (sauce.Contains("\r"))
            {
                return "\r";
            }
            else if (sauce.Contains("\n"))
            {
                return "\n";
            }
            else
            {
                return null;
            }
        }

        #region Java[OBSOLETE]
        /*
        public static void compileJava(String name, String fileName, String[] javaRefs)
        {

            if (Application.platform.Equals(RuntimePlatform.WindowsPlayer) ||
                Application.platform.Equals(RuntimePlatform.WindowsEditor))
            {
                var process = new Process();
                var startInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "cmd.exe",
                    Arguments = "jar cf " + Application.dataPath + "/Mods/Scripts/TempScripts/" +
                                name + ".jar " + fileName + " " + string.Concat(javaRefs)
                };
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
            }
            else if (Application.platform.Equals(RuntimePlatform.LinuxPlayer) ||
                     Application.platform.Equals(RuntimePlatform.OSXEditor) ||
                     Application.platform.Equals(RuntimePlatform.OSXPlayer))
            {
                var psi = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "/bin/bash.sh",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    Arguments = "jar cf " + Application.dataPath + "/Mods/Scripts/TempScripts/" +
                                name + ".jar " + fileName + " " + string.Concat(javaRefs)
                };

                var p = Process.Start(psi);
                p.WaitForExit();
            }
        }
        */
        #endregion 

        /// <summary>
        /// Converts Unity-Script source into C# source
        /// </summary>
        /// <param name="output">Source code in US</param>
        /// <param name="className">Name of the script</param>
        /// <returns>Source code in C# format</returns>
        public static string ConvertJSToC(string output, string className)
        {
            var VAR = @"[A-Za-z0-9_\[\]\.]";
            var VAR_NONARRAY = @"[A-Za-z0-9_]";

            var patterns = new string[32];
            var replacements = new string[32];
            var patrs = 0;
            var reps = 0;

            //var AAA;
            patterns[patrs++] = @"var(\s+)(" + VAR_NONARRAY + @"+)(\s*);";
            replacements[reps++] = "FIXME_VAR_TYPE $2;";
            // var AAAA : XXX	
            patterns[patrs++] = @"var(\s+)(" + VAR_NONARRAY + @"+)(\s*):(\s*)(" + VAR + @"+)";
            replacements[reps++] = "$5 $2";
            // var AAAA =	
            patterns[patrs++] = @"var(\s+)(" + VAR_NONARRAY + @"+)(\s*)=";
            replacements[reps++] = "FIXME_VAR_TYPE $2=";

            //Try to fix the "FIXME_VAR_TYPE" for strings;
            patterns[patrs++] = @"FIXME_VAR_TYPE(\s+)(" + VAR_NONARRAY + @"+)(\s*)=(\s*)""";
            replacements[reps++] = "string $2 = \"";
            //Try to fix the "FIXME_VAR_TYPE" for floats;
            patterns[patrs++] = @"FIXME_VAR_TYPE(\s+)(" + VAR_NONARRAY +
                                @"+)(\s*)=(\s*)(([0-9\-]*\.+[0-9]+[f]?)+)(\s*);";
            replacements[reps++] = "float $2 = $5;";
            //Try to fix the "FIXME_VAR_TYPE" for int;
            patterns[patrs++] = @"FIXME_VAR_TYPE(\s+)(" + VAR_NONARRAY + @"+)(\s*)=(\s*)([0-9\-]+)(\s*);";
            replacements[reps++] = "float $2 = $5;";
            //Try to fix the "FIXME_VAR_TYPE" for bool true/false;
            patterns[patrs++] = @"FIXME_VAR_TYPE(\s+)(" + VAR_NONARRAY + @"+)(\s*)=(\s*)true(\s*);";
            replacements[reps++] = "bool $2 = true;";
            patterns[patrs++] = @"FIXME_VAR_TYPE(\s+)(" + VAR_NONARRAY + @"+)(\s*)=(\s*)false(\s*);";
            replacements[reps++] = "bool $2 = false;";

            //0.05f
            patterns[patrs++] = @"([\s\t\r\n\,=\( ]*)(\d+)\.(\d+)([\s\t\r\,;\) ]*)";
            replacements[reps++] = "$1$2.$3f$4";

            //Style
            patterns[patrs++] = "\n\n\n";
            replacements[reps++] = "\n";

            //RPC
            patterns[patrs++] = @"([\n\s\(\)\{\}\r]+)@RPC";
            replacements[reps++] = "$1[RPC]";

            //boolean
            patterns[patrs++] = @"([\n\t\s \(\)\{\}\r:;]*)\bboolean\b([\n\t\s \,\[=\(\)\{\}\r]+)";
            replacements[reps++] = "${1}bool${2} ";
            //String
            patterns[patrs++] = @"([\n\t\s \(\)\{\}\r:;]*)\bString\b([\n\t\s \,\[=\(\)\{\}\r]+)";
            replacements[reps++] = "${1}string${2}";

            //Remove #pragma strict	
            patterns[patrs++] = "#pragma strict";
            replacements[reps++] = "";
            patterns[patrs++] = "#pragma implicit";
            replacements[reps++] = "";
            patterns[patrs++] = "#pragma downcast";
            replacements[reps++] = "";


            //parseInt parseFloat
            patterns[patrs++] = "parseInt";
            replacements[reps++] = "int.Parse";
            patterns[patrs++] = "parseFloat";
            replacements[reps++] = "float.Parse";

            // (Rect(
            patterns[patrs++] = @"([]\(=)]+)[\s]*Rect[\s]*\(";
            replacements[reps++] = "${1} new Rect(";

            //Yield
            patterns[patrs++] = @"yield(\s+)(\w+);";
            replacements[reps++] = "yield return ${2};";
            patterns[patrs++] = @"yield;";
            replacements[reps++] = "yield return 0;";
            patterns[patrs++] = @"yield(\s+)(\w+)\(";
            replacements[reps++] = "yield return new ${2}(";
            patterns[patrs++] = @"yield new";
            replacements[reps++] = "yield return new";

            //For -> foreach
            patterns[patrs++] = @"for[\s]*\(([A-Za-z0-9_ :\,\.\[\]\s\n\r\t]*) in ([A-Za-z0-9_ :\,\.\s\n\r\t]*)\)";
            replacements[reps++] = "foreach($1 in $2)";

            //function rewrite
            patterns[patrs++] = @"function(\s+)(\w+)(\s*)\(([\n\r\tA-Za-z0-9_\[\]\*\/ \.:\,]*)\)(\s*):(\s*)(" + VAR +
                                @"+)(\s*)\{";
            replacements[reps++] = "$7 $2 ($4){";
            //function rewrite
            patterns[patrs++] = @"function(\s+)(\w+)(\s*)\(([\n\r\tA-Za-z0-9_\[\]\*\/ \.:\,]*)\)(\s*)\{";
            replacements[reps++] = @"void $2 ($4){";

            //Getcomponent
            patterns[patrs++] = @"AddComponent\(([\n\r\tA-Za-z0-9_ ""]*)\)";
            replacements[reps++] = "AddComponent<$1>()";
            patterns[patrs++] = @"GetComponent\(([\n\r\tA-Za-z0-9_ ""]*)\)";
            replacements[reps++] = "GetComponent<$1>()";
            patterns[patrs++] = @"GetComponentsInChildren\(([\n\r\tA-Za-z0-9_ ""]*)\)";
            replacements[reps++] = "GetComponentsInChildren<$1>()";
            patterns[patrs++] = @"GetComponentInChildren\(([\n\r\tA-Za-z0-9_ ''""]*)\)";
            replacements[reps++] = "GetComponentInChildren<$1>()";
            patterns[patrs++] = @"FindObjectsOfType\(([\n\r\tA-Za-z0-9_ ""]*)\)";
            replacements[reps++] = "FindObjectsOfType(typeof($1))";
            patterns[patrs++] = @"FindObjectOfType\(([\n\r\tA-Za-z0-9_ ''""]*)\)";
            replacements[reps++] = "FindObjectOfType(typeof($1))";

            output = PregReplace(output, patterns, replacements);

            var before = "";
            while (before != output)
            {
                //( XX : YY) rewrite
                before = output;
                var patt = @"\(([\t\n\rA-Za-z0-9_*\/ \.\,\]\[]*)\b(\w+)\b(\s*):(\s*)(" + VAR +
                           @"+)([\s\,]*)([\[\]\n\r\t\sA-Za-z0-9_\*\/ :\,\.]*)\)";
                var repp = "(${1} ${5} ${2} ${6} ${7})"; //'(${1} 5--${5}-- 2--${2}-- 5--${6}-- 7=--${7})'
                output = Regex.Replace(output, patt, repp);
            }
            return output;
        }

        /// <summary>
        /// Helper-Method for $ConvertJSToC
        /// </summary>
        /// <param name="input"></param>
        /// <param name="pattern"></param>
        /// <param name="replacements"></param>
        /// <returns></returns>
        private static string PregReplace(string input, string[] pattern, string[] replacements)
        {
            if (replacements.Length != pattern.Length)
                throw new ArgumentException("Replacement and Pattern Arrays must be balanced");

            for (var i = 0; i < pattern.Length; i++)
            {
                input = Regex.Replace(input, pattern[i], replacements[i]);
            }

            return input;
        }
    }
}