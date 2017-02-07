#region usings

using System;
using System.Collections.Generic;

#endregion

namespace BesiegeScriptingMod.Chef
{
    public class Container
    {
        private List<Component> contents;

        public Container()
        {
            contents = new List<Component>();
        }

        public Container(Container container)
        {
            contents = new List<Component>(container.contents);
        }

        public void push(Component c)
        {
            contents.Add(c);
        }

        public Component peek()
        {
            return contents[contents.Count - 1];
        }

        public Component pop()
        {
            if (contents.Count == 0)
                throw new ChefException(ChefException.LOCAL, "Folded from empty container");
            Component last = contents[contents.Count - 1];
            contents.RemoveAt(contents.Count - 1);
            return last;
        }

        public int size()
        {
            return contents.Count;
        }

        public void combine(Container c)
        {
            contents.AddRange(c.contents);
        }

        public void liquefy()
        {
            foreach (Component component in contents)
            {
                component.liquefy();
            }
        }

        public void clean()
        {
            contents = new List<Component>();
        }

        public string serve()
        {
            string result = "";
            for (int i = contents.Count - 1; i >= 0; i--)
            {
                if (contents[i].getState() == Ingredient.State.Dry)
                {
                    result += contents[i].getValue() + " ";
                }
                else
                {
                    result += (contents[i].getValue()%1112064).ToString().ToCharArray()[0];
                }
            }
            return result;
        }

        public void shuffle()
        {
            Random rng = new Random();
            int n = contents.Count;

            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Component tmp = contents[k];
                contents[k] = contents[n];
                contents[n] = tmp;
            }
        }

        public void stir(int time)
        {
            for (int i = 0; i < time && i + 1 < contents.Count; i++)
            {
                Component tmp = contents[contents.Count - i - 1];
                contents[contents.Count - i - 1] = contents[contents.Count - i - 2];
                contents[contents.Count - i - 2] = tmp;
            }
        }
    }
}