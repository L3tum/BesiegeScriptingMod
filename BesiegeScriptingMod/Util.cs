using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CSharp;
using Microsoft.Scripting;
using UnityEngine;

namespace BesiegeScriptingMod
{
    public static class Util
    {
        public static FileInfo Compile(FileInfo sauce, String refs, String name)
        {
            CompilerParameters cp = new CompilerParameters();
            cp.GenerateExecutable = false;
            cp.GenerateInMemory = false;
            cp.TreatWarningsAsErrors = false;
            cp.CompilerOptions = " /out:" + Application.dataPath + "/Mods/Scripts/TempScripts/" + name + ".dll";

            Process compiler = new Process();
            ProcessStartInfo info = new ProcessStartInfo(Application.dataPath + "/Mods/Scripts/Resource/csws.exe",
                "/cd /co:" + (cp.CompilerOptions.Trim()) + " /r:" + (refs.Trim(':')) + " " + sauce);
            UnityEngine.Debug.Log(info.Arguments);
            compiler.StartInfo = info;
            compiler.Start();
            return new FileInfo(Application.dataPath + "/Mods/Scripts/TempScripts/" + name + ".dll");
        }



        public static string ConvertJSToC(string output, string className)
        {
            string VAR = @"[A-Za-z0-9_\[\]\.]";
            string VAR_NONARRAY = @"[A-Za-z0-9_]";

            string[] patterns = new string[32];
            string[] replacements = new string[32];
            int patrs = 0;
            int reps = 0;

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
            patterns[patrs++] = @"FIXME_VAR_TYPE(\s+)(" + VAR_NONARRAY + @"+)(\s*)=(\s*)(([0-9\-]*\.+[0-9]+[f]?)+)(\s*);";
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
            patterns[patrs++] = @"function(\s+)(\w+)(\s*)\(([\n\r\tA-Za-z0-9_\[\]\*\/ \.:\,]*)\)(\s*):(\s*)(" + VAR + @"+)(\s*)\{";
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

            string before = "";
            while (before != output)
            {
                //( XX : YY) rewrite
                before = output;
                string patt = @"\(([\t\n\rA-Za-z0-9_*\/ \.\,\]\[]*)\b(\w+)\b(\s*):(\s*)(" + VAR + @"+)([\s\,]*)([\[\]\n\r\t\sA-Za-z0-9_\*\/ :\,\.]*)\)";
                string repp = "(${1} ${5} ${2} ${6} ${7})";//'(${1} 5--${5}-- 2--${2}-- 5--${6}-- 7=--${7})'
                output = Regex.Replace(output, patt, repp);
            }


            output = "using UnityEngine;\nusing System.Collections;\nusing System;\nusing spaar;\nusing System.Collections.Generic;\n\npublic class " + className + " : MonoBehaviour {\n" + output + "\n}";
            return output;
        }


        static string PregReplace(string input, string[] pattern, string[] replacements)
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
