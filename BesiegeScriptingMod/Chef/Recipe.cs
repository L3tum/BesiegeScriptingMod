using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace BesiegeScriptingMod.Chef
{
    public class Recipe
    {
        private String title;
        private Dictionary<String, Ingredient> ingredients;
        private String comment;
        private int cookingtime;
        private int oventemp;
        private int gasmark;
        private List<Method> methods;
        private int serves;

        public Recipe(String title)
        {
            this.title = title;
        }

        public void setIngredients(String ingredients)
        {
            this.ingredients = new Dictionary<String, Ingredient>
                ();
            TextReader scanner = new StreamReader(ingredients);
            String s = ingredients;
            int numLines = s.Length - s.Replace(Util.getNewLine(), string.Empty).Length;
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

        public void setComments(String comment)
        {
            this.comment = comment;
        }

        public void setMethod(String method)
        {
            this.methods = new List<Method>();
            method = method.Replace("\n", "");
            method = method.Replace("\\. ", ".");
            String[] strings = Regex.Split(method, "\\.");
            //Clearing the 'Method.' header
            for (int i = 1; i < strings.Length; i++)
            {
                this.methods.Add(new Method(strings[i] + ".", i));
            }
        }

        public void setCookingTime(String cookingtime)
        {
            this.cookingtime = int.Parse(cookingtime.Split()[2]);
        }

        public void setOvenTemp(String oventemp)
        {
            this.oventemp = int.Parse(oventemp.Split()[3]);
            if (Regex.Match(oventemp, "gas mark").Success)
            {
                String mark = oventemp.Split()[8];
                this.gasmark = int.Parse(mark.Substring(0, mark.Length - 1));
            }
        }

        public void setServes(String serves)
        {
            this.serves = int.Parse(serves.Substring("Serves ".Length, serves.Length - 1));
        }

        public int getServes()
        {
            return serves;
        }

        public String getTitle()
        {
            return title;
        }

        public int getIngredientValue(String s)
        {
            return ingredients[s].getAmount();
        }

        public void setIngredientValue(String s, int n)
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

        public Dictionary<String, Ingredient> getIngredients()
        {
            return ingredients;
        }
    }
}