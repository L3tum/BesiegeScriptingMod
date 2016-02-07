using System;

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

        public ChefException(int ingredient, String[] tokens, String error)
            : base(ingredient.ToString() + " " + error)
        {
        }
        public ChefException(int ingredient, String error)
            : base(ingredient.ToString() + " " + error)
        {
        }
    }
}