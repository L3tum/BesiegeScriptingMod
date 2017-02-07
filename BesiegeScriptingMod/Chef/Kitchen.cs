#region usings

using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

namespace BesiegeScriptingMod.Chef
{
    public class Kitchen
    {
        private readonly Container[] bakingdishes;
        private readonly Container[] mixingbowls;
        private readonly Recipe recipe;
        private readonly Dictionary<string, Recipe> recipes;

        public Kitchen(Dictionary<string, Recipe> recipes, Recipe mainrecipe)
        {
            Container[] mbowls = null;
            Container[] bdishes = null;
            this.recipes = recipes;
            //start with at least 1 mixing bowl.
            int maxbowl = 0, maxdish = -1;
            recipe = mainrecipe;
            foreach (Method m in recipe.getMethods())
            {
                if (m.bakingdish != null && m.bakingdish > maxdish)
                    maxdish = m.bakingdish;
                if (m.mixingbowl != null && m.mixingbowl > maxbowl)
                    maxbowl = m.mixingbowl;
            }
            mixingbowls = new Container[mbowls == null ? maxbowl + 1 : Math.Max(maxbowl + 1, mbowls.Length)];
            bakingdishes = new Container[bdishes == null ? maxdish + 1 : Math.Max(maxdish + 1, bdishes.Length)];
            for (int i = 0; i < mixingbowls.Length; i++)
                mixingbowls[i] = new Container();
            for (int i = 0; i < bakingdishes.Length; i++)
                bakingdishes[i] = new Container();
        }

        public Kitchen(Dictionary<string, Recipe> recipes, Recipe mainrecipe,
            Container[] mbowls, Container[] bdishes)
        {
            this.recipes = recipes;
            //start with at least 1 mixing bowl.
            int maxbowl = 0, maxdish = -1;
            recipe = mainrecipe;
            foreach (Method m in recipe.getMethods())
            {
                if (m.bakingdish != null && m.bakingdish > maxdish)
                    maxdish = m.bakingdish;
                if (m.mixingbowl != null && m.mixingbowl > maxbowl)
                    maxbowl = m.mixingbowl;
            }
            mixingbowls = new Container[mbowls == null ? maxbowl + 1 : Math.Max(maxbowl + 1, mbowls.Length)];
            bakingdishes = new Container[bdishes == null ? maxdish + 1 : Math.Max(maxdish + 1, bdishes.Length)];
            for (int i = 0; i < mixingbowls.Length; i++)
                mixingbowls[i] = new Container();
            for (int i = 0; i < bakingdishes.Length; i++)
                bakingdishes[i] = new Container();
            if (mbowls != null)
            {
                for (int i = 0; i < mbowls.Length; i++)
                {
                    mixingbowls[i] = new Container(mbowls[i]);
                }
            }
            if (bdishes != null)
            {
                for (int i = 0; i < bdishes.Length; i++)
                {
                    bakingdishes[i] = new Container(bdishes[i]);
                }
            }
        }

        public Container cook()
        {
            ChefGUI gui = GameObject.Find("Scripting Mod").AddComponent<ChefGUI>();
            Dictionary<string, Ingredient> ingredients = recipe.getIngredients();
            List<Method> methods = recipe.getMethods();
            LinkedList<LoopData> loops = new LinkedList<LoopData>();
            Component c;
            int i = 0;
            bool deepfrozen = false;
            while (i < methods.Count && !deepfrozen)
            {
                Method m = methods[i];
                switch (m.type)
                {
                    case Method.Type.Take:
                    case Method.Type.Put:
                    case Method.Type.Fold:
                    case Method.Type.Add:
                    case Method.Type.Remove:
                    case Method.Type.Combine:
                    case Method.Type.Divide:
                    case Method.Type.Liquefy:
                    case Method.Type.StirInto:
                    case Method.Type.Verb:
                        if (ingredients[m.ingredient] == null)
                            throw new ChefException(ChefException.METHOD, "Ingredient not found: " + m.ingredient);
                        break;
                }
                switch (m.type)
                {
                    case Method.Type.Take:
                        gui.label = "Set amount of ingredient " + ingredients[m.ingredient].getName() + " to take.";
                        while (gui.label.Equals(""))
                        {
                        }
                        ingredients[m.ingredient].setAmount(int.Parse(gui.input));
                        Object.Destroy(gui);
                        break;
                    case Method.Type.Put:
                        mixingbowls[m.mixingbowl].push(new Component(ingredients[m.ingredient]));
                        break;
                    case Method.Type.Fold:
                        if (mixingbowls[m.mixingbowl].size() == 0)
                            throw new ChefException(ChefException.METHOD,
                                "Folded from empty mixing bowl: " + (m.mixingbowl + 1));
                        c = mixingbowls[m.mixingbowl].pop();
                        ingredients[m.ingredient].setAmount(c.getValue());
                        ingredients[m.ingredient].setState(c.getState());
                        break;
                    case Method.Type.Add:
                        c = mixingbowls[m.mixingbowl].peek();
                        c.setValue(c.getValue() + ingredients[m.ingredient].getAmount());
                        break;
                    case Method.Type.Remove:
                        c = mixingbowls[m.mixingbowl].peek();
                        c.setValue(c.getValue() - ingredients[m.ingredient].getAmount());
                        break;
                    case Method.Type.Combine:
                        c = mixingbowls[m.mixingbowl].peek();
                        c.setValue(c.getValue()*ingredients[m.ingredient].getAmount());
                        break;
                    case Method.Type.Divide:
                        c = mixingbowls[m.mixingbowl].peek();
                        c.setValue(c.getValue()/ingredients[m.ingredient].getAmount());
                        break;
                    case Method.Type.AddDry:
                        int sum = 0;
                        foreach (KeyValuePair<string, Ingredient> s in ingredients)
                            if (s.Value.getstate() == Ingredient.State.Dry)
                                sum += s.Value.getAmount();
                        mixingbowls[m.mixingbowl].push(new Component(sum, Ingredient.State.Dry));
                        break;
                    case Method.Type.Liquefy:
                        ingredients[m.ingredient].liquefy();
                        break;
                    case Method.Type.LiquefyBowl:
                        mixingbowls[m.mixingbowl].liquefy();
                        break;
                    case Method.Type.Stir:
                        mixingbowls[m.mixingbowl].stir(m.time);
                        break;
                    case Method.Type.StirInto:
                        mixingbowls[m.mixingbowl].stir(ingredients[m.ingredient].getAmount());
                        break;
                    case Method.Type.Mix:
                        mixingbowls[m.mixingbowl].shuffle();
                        break;
                    case Method.Type.Clean:
                        mixingbowls[m.mixingbowl].clean();
                        break;
                    case Method.Type.Pour:
                        bakingdishes[m.bakingdish].combine(mixingbowls[m.mixingbowl]);
                        break;
                    case Method.Type.Verb:
                        int end = i + 1;
                        for (; end < methods.Count; end++)
                            if (sameVerb(m.verb, methods[end].verb) && methods[end].type == Method.Type.VerbUntil)
                                break;
                        if (end == methods.Count)
                            throw new ChefException(ChefException.METHOD, "Loop end statement not found.");
                        if (ingredients[m.ingredient].getAmount() <= 0)
                        {
                            i = end + 1;
                            continue;
                        }
                        loops.AddFirst(new LoopData(i, end, m.verb));
                        break;
                    case Method.Type.VerbUntil:
                        if (!sameVerb(loops.First.Value.verb, m.verb))
                            throw new ChefException(ChefException.METHOD, "Wrong loop end statement found.");
                        if (ingredients[m.ingredient] != null)
                            ingredients[m.ingredient].setAmount(ingredients[m.ingredient].getAmount() - 1);
                        i = loops.First.Value.@from;
                        loops.RemoveFirst();
                        continue;
                    case Method.Type.SetAside:
                        if (loops.Count == 0)
                            throw new ChefException(ChefException.METHOD, "Cannot set aside when not inside loop.");
                        i = loops.First.Value.to + 1;
                        continue;
                    case Method.Type.Serve:
                        if (recipes[m.auxrecipe.ToLower()] == null)
                            throw new ChefException(ChefException.METHOD, "Unavailable recipe: " + m.auxrecipe);
                        Kitchen k = new Kitchen(recipes, recipes[m.auxrecipe.ToLower()], mixingbowls,
                            bakingdishes);
                        Container con = k.cook();
                        mixingbowls[0].combine(con);
                        break;
                    case Method.Type.Refrigerate:
                        if (m.time > 0)

                            serve(m.time);
                        deepfrozen = true;
                        break;
                    case Method.Type.Remember:
                        break;
                    default:
                        throw new ChefException(ChefException.METHOD, "Unsupported method found!");
                }
                i++;
            }
            if (recipe.getServes() > 0 && !deepfrozen)

                serve(recipe.getServes());
            if (mixingbowls.Length > 0)
                return mixingbowls[0];
            return null;
        }

        private bool sameVerb(string imp, string verb)
        {
            if (verb == null || imp == null)
                return false;
            verb = verb.ToLower();
            imp = imp.ToLower();
            int L = imp.Length;
            return verb.Equals(imp) || //A = A
                   verb.Equals(imp + "n") || //shake ~ shaken
                   verb.Equals(imp + "d") || //prepare ~ prepared
                   verb.Equals(imp + "ed") || //monitor ~ monitored
                   verb.Equals(imp + imp[L - 1] + "ed") || //stir ~ stirred
                   (imp[L - 1] == 'y' && verb.Equals(imp.Substring(0, L - 1) + "ied")); //carry ~ carried
        }

        private void serve(int n)
        {
            for (int i = 0; i < n && i < bakingdishes.Length; i++)
            {
                Debug.Log(bakingdishes[i].serve());
            }
        }

        private class LoopData
        {
            public readonly int from;
            public readonly int to;
            public readonly string verb;

            public LoopData(int from, int to, string verb)
            {
                this.from = from;
                this.to = to;
                this.verb = verb;
            }
        }
    }
}