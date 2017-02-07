#region usings

using System;
using System.Runtime.InteropServices;

#endregion

#if UNITY_3D

namespace KeraLua
{
    /// <summary>
    /// Disables CLS Compliance in Unity3D.
    /// </summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.All)]
    [ComVisible(true)]
    public class CLSCompliantAttribute : Attribute
    {
        public CLSCompliantAttribute(bool isCompliant)
        {
            IsCompliant = isCompliant;
        }

        public bool IsCompliant { get; }
    }
}

#endif