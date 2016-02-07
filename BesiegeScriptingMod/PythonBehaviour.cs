using System;
using Microsoft.Scripting.Hosting;
using UnityEngine;

namespace BesiegeScriptingMod
{
    class PythonBehaviour : MonoBehaviour
    {
        ScriptEngine engine = IronPython.Hosting.Python.CreateEngine();
        ScriptScope scope;
        String sauce;
        CompiledCode code;

        public void Awakening()
        {
            scope = engine.CreateScope();
            scope.SetVariable("this", this);
            scope.SetVariable("gameObject", gameObject);
            scope.SetVariable("transform", transform);
            scope.SetVariable("enabled", enabled);
            ScriptSource source = engine.CreateScriptSourceFromString(sauce);
            code = source.Compile();
        }

        public void Run()
        {
            code.Execute(scope);
        }
    }
}
