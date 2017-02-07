#region usings

using System;

#endregion

namespace BesiegeScriptingMod.Chef
{
    public class ChefException : Exception
    {
        public static readonly int STRUCTURAL = 0;
        public static readonly int LOCAL = 1;
        public static readonly int INGREDIENT = 2;
        public static readonly int METHOD = 3;

        public ChefException()
        {
        }

        public ChefException(string message) : base(message)
        {
        }

        public ChefException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ChefException(int ingredient, string[] tokens, string error)
            : base(ingredient + " " + error)
        {
        }

        public ChefException(int ingredient, string error)
            : base(ingredient + " " + error)
        {
        }
    }
}