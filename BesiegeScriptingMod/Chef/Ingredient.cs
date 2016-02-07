using System;
using System.Text.RegularExpressions;

namespace BesiegeScriptingMod.Chef
{
    public class Ingredient
    {
        public enum State
        {
            Dry,
            Liquid
        }

        private String name;
        private int amount;
        private State state;

        public Ingredient(String ingredient)
        {
            String[] tokens = ingredient.Split();

            int i = 0;
            state
                =
                Ingredient.State.Dry;
            if (Regex.Match(tokens[i], "^\\d*$").Success)
            {
                amount = int.Parse(tokens[i]);
                i++;
                if (Regex.Match(tokens[i], "heaped|level").Success)
                {
                    state = Ingredient.State.Dry;
                    i++;
                }
                if (Regex.Match(tokens[i], "^g|kg|pinch(es)?").Success)
                {
                    state = Ingredient.State.Dry;
                    i++;
                }
                else if (Regex.Match(tokens[i], "^ml|l|dash(es)?").Success)
                {
                    state = Ingredient.State.Liquid;
                    i++;
                }
                else if (Regex.Match(tokens[i], "^cup(s)?|teaspoon(s)?|tablespoon(s)?").Success)
                {
                    i++;
                }
            }
            name = "";
            while (i < tokens.Length)
            {
                name += tokens[i] + (i == tokens.Length - 1 ? "" : " ");
                i++;
            }
            if (
                name.Equals(""))
            {
                throw new ChefException(ChefException.INGREDIENT, tokens, "ingredient name missing");
            }
        }

        public Ingredient(int n, State s, String name)
        {
            this.amount = n;
            this.state = s;
            this.name = name;
        }

        public int getAmount()
        {
            return amount;
        }

        public void setAmount(int n)
        {
            amount = n;
        }

        public State getstate()
        {
            return state;
        }

        public void liquefy()
        {
            state = State.Liquid;
        }

        public void dry()
        {
            state = State.Dry;
        }

        public String getName()
        {
            return name;
        }

        public void setState(State s)
        {
            state = s;
        }
    }
}