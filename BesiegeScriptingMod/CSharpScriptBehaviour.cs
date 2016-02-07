using System;
using System.Reflection;
using UnityEngine;

namespace BesiegeScriptingMod
{
    class CSharpScriptBehaviour : MonoBehaviour
    {
        string fullPath = @"";
        public Component addedType = null;
        public bool hasUnload = false;

        public void SetPath(string fullpath)
        {
            fullPath = @fullpath;
        }

        public void Awakening(string name)
        {
            Assembly ass = Assembly.LoadFrom(fullPath);
            foreach (Type type in ass.GetExportedTypes())
            {
                if (type.BaseType == typeof (MonoBehaviour))
                {
                    addedType = gameObject.AddComponent(type);
                    foreach (MethodInfo methodInfo in type.GetMethods())
                    {
                        if (methodInfo.Name.Equals("OnLoad"))
                        {
                            addedType.SendMessage("OnLoad");
                        }
                        else if (methodInfo.Name.Equals("OnUnload"))
                        {
                            hasUnload = true;
                        }
                    }
                    break;
                }
            }
            if (addedType == null)
            {
                Debug.Log("No suitable Class found! Are you inheriting MonoBehaviour correctly?");
                Destroy(this);
            }
        }

        public void Unload()
        {
            if (hasUnload)
            {
                addedType.SendMessage("OnUnload");
            }
            Destroy(gameObject);
        }
    }
}
