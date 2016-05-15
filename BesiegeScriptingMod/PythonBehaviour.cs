using System;
using System.Linq;
using System.Reflection;
using Microsoft.Scripting;
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
        String[] refs;
        private object pythonClass;

        /// <summary>
        /// Initializes the $PythonBehaviour with the Source code and the References needed for the Script
        /// </summary>
        /// <param name="sauce">Source code</param>
        /// <param name="refs">References</param>
        public void SourceCodening(String sauce, String[] refs)
        {
            this.sauce = sauce;
            this.refs = refs;
        }

        /// <summary>
        /// Starts the $PythonBehaviour
        /// </summary>
        /// <param name="classname">Name of the Script</param>
        public bool Awakening(String classname)
        {
            scope = engine.CreateScope();
            scope.SetVariable("this", this);
            scope.SetVariable("gameObject", gameObject);
            scope.SetVariable("transform", transform);
            scope.SetVariable("enabled", enabled);
            spaar.ModLoader.Game.OnSimulationToggle += GameOnOnSimulationToggle;
            spaar.ModLoader.Game.OnLevelWon += GameOnOnLevelWon;

            foreach (string @ref in refs.Where(@ref => !String.IsNullOrEmpty(@ref)))
            {
                try
                {

                    #region OBSOLETE
                    /*
                    Assembly assembly = Assembly.Load(@ref);
                    var namespaces = assembly.GetTypes()
                        .Select(t => t.Namespace)
                        .Distinct();
                    String[] lines = Util.splitStringAtNewline(sauce);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (!lines[i].Contains("import") && !String.IsNullOrEmpty(lines[i]))
                        {
                            foreach (string ns in namespaces)
                            {
                                if (!String.IsNullOrEmpty(ns))
                                {
                                    if (lines[i].Contains((ns + ".")))
                                    {
                                        lines[i] = Regex.Replace(lines[i], ns + ".", string.Empty);
                                    }
                                }
                            }
                        }
                        lines[i] += Util.getNewLine();
                    }
                    lines = lines.Where(x => !string.IsNullOrEmpty(x) && !x.Equals("\r\n") && !x.Equals("\r") && !x.Equals("\n") && !String.IsNullOrEmpty(x.Trim())).ToArray();
                    sauce = String.Concat(lines);
                    */
                    #endregion 

                    engine.Runtime.LoadAssembly(Assembly.Load(@ref));
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    return false;
                }
            }
            ScriptSource source = engine.CreateScriptSourceFromString(sauce);
            ErrorListener error = new ErrorSinkProxyListener(ErrorSink.Default);
            code = source.Compile(error);
            if (code == null)
            {
                Debug.LogError(error);
                return false;
            }
            code.Execute(scope);
            pythonClass = engine.Operations.Invoke(scope.GetVariable(classname));
            CallMethod("Awake");
            return true;
        }

        private void GameOnOnLevelWon()
        {
            CallMethod("OnLevelWon");
        }

        private void GameOnOnSimulationToggle(bool simulating)
        {
            CallMethod("OnSimulationToggle", simulating);
        }

        public void Awake()
        {
            CallMethod("Awake");
        }

        public void Start()
        {
            CallMethod("Start");
        }

        public void Update()
        {
            CallMethod("Update");
        }

        public void OnGUI()
        {
            CallMethod("OnGUI");
        }

        public void LateUpdate()
        {
            CallMethod("LateUpdate");
        }

        public void FixedUpdate()
        {
            CallMethod("FixedUpdate");
        }

        public void OnLevelWasLoaded(int level)
        {
            CallMethod("OnLevelWasLoaded", level);
        }

        public void OnDestroy()
        {
            CallMethod("OnDestroy");
            code = null;
            engine = null;
            scope = null;
        }

        /// <summary>
        /// Sets variable in the script scope
        /// </summary>
        /// <param name="variable">Variable name</param>
        /// <param name="value">Variable value</param>
        public void SetVariable(string variable, object value)
        {
            scope.SetVariable(variable, value);
        }

        /// <summary>
        /// Gets a variable specified in the script scope
        /// </summary>
        /// <param name="variable">Name of the variable</param>
        /// <returns>The variable with the $name</returns>
        public object GetVariable(string variable)
        {
            return scope.GetVariable(variable);
        }

        /// <summary>
        /// Calls method in Script Scope
        /// </summary>
        /// <param name="method">Method name</param>
        private void CallMethod(string method)
        {
            try
            {
                if (engine.Operations.GetMember(pythonClass, method) != null)
                {
                    engine.Operations.InvokeMember(pythonClass, method);
                }
            }
            catch(Exception ex)
            {
                if (ex.GetType() != typeof (MissingMemberException))
                {
                    Debug.LogException(ex);
                }
            }
        }

        /// <summary>
        /// Calls method in Script Scope with specified arguments
        /// </summary>
        /// <param name="method">Method name</param>
        /// <param name="arguments">Arguments to pass to method</param>
        private void CallMethod(string method, params object[] arguments)
        {
            try
            {
                if (engine.Operations.GetMember(pythonClass, method) != null)
                {
                    engine.Operations.InvokeMember(pythonClass, method, arguments);
                }
            }
            catch(Exception ex)
            {
                if (ex.GetType() != typeof(MissingMemberException))
                {
                    Debug.LogException(ex);
                }
            }
        }

        /// <summary>
        /// Calls a method in Script Scope which returns something
        /// </summary>
        /// <param name="method">Method name</param>
        /// <param name="arguments">Arguments to pass to method</param>
        /// <returns>The returned value by the method</returns>
        private object CallFunction(string method, params object[] arguments)
        {
            try
            {
                if (engine.Operations.GetMember(pythonClass, method) != null)
                {
                    return engine.Operations.InvokeMember(pythonClass, method, arguments);
                }
            }
            catch(Exception ex)
            {
                if (ex.GetType() != typeof(MissingMemberException))
                {
                    Debug.LogException(ex);
                }
            }
            return null;
        }
    }
}