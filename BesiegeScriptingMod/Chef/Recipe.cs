#region usings

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

#endregion

namespace BesiegeScriptingMod.Chef
{
    public class Recipe
    {
        private string comment;
        private int cookingtime;
        private int gasmark;
        private Dictionary<string, Ingredient> ingredients;
        private List<Method> methods;
        private int oventemp;
        private int serves;
        private readonly string title;

        public Recipe(string title)
        {
            this.title = title;
        }

        public void setIngredients(string ingredients)
        {
            this.ingredients = new Dictionary<string, Ingredient>
                ();
            TextReader scanner = new StreamReader(ingredients);
            string s = ingredients;
            int numLines = s.Length - s.Replace(Util.Util.getNewLine(), string.Empty).Length;
            int i = 0;
            scanner.ReadLine();
            i++;
            while (i < numLines)
            {
                Ingredient ing = new Ingredient(scanner.ReadLine());
                this.ingredients.Add(ing.getName().ToLower(), ing);
                i++;
            }
        }

        public void setComments(string comment)
        {
            this.comment = comment;
        }

        public void setMethod(string method)
        {
            methods = new List<Method>();
            method = method.Replace("\n", "");
            method = method.Replace("\\. ", ".");
            string[] strings = Regex.Split(method, "\\.");
            //Clearing the 'Method.' header
            for (int i = 1; i < strings.Length; i++)
            {
                methods.Add(new Method(strings[i] + ".", i));
            }
        }

        public void setCookingTime(string cookingtime)
        {
            this.cookingtime = int.Parse(cookingtime.Split()[2]);
        }

        public void setOvenTemp(string oventemp)
        {
            this.oventemp = int.Parse(oventemp.Split()[3]);
            if (Regex.Match(oventemp, "gas mark").Success)
            {
                string mark = oventemp.Split()[8];
                gasmark = int.Parse(mark.Substring(0, mark.Length - 1));
            }
        }

        public void setServes(string serves)
        {
            this.serves = int.Parse(serves.Substring("Serves ".Length, serves.Length - 1));
        }

        public int getServes()
        {
            return serves;
        }

        public string getTitle()
        {
            return title;
        }

        public int getIngredientValue(string s)
        {
            return ingredients[s].getAmount();
        }

        public void setIngredientValue(string s, int n)
        {
            ingredients[s].setAmount(n);
        }

        public Method getMethod(int i)
        {
            return methods[i];
        }

        public List<Method> getMethods()
        {
            return methods;
        }

        public Dictionary<string, Ingredient> getIngredients()
        {
            return ingredients;
        }
    }
}