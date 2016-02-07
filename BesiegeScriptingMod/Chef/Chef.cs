using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace BesiegeScriptingMod.Chef
{
    public class Chef
    {
        public static void main(String[] args)
        {
            Chef interpreter = new Chef(args[0]);
            interpreter.bake();
        }

        FileStream file;
        TextReader scan;
        Dictionary<String, Recipe> recipes;
        Recipe mainrecipe;

        public Chef(String filename)
        {
            recipes = new Dictionary<string, Recipe>();
            //System.out.print("Reading recipe(s) from '"+filename+"'.. ");
            file = File.Open(filename, FileMode.OpenOrCreate);
            scan = new StreamReader(file);
            String lines = scan.ReadToEnd();
            String[] linees = Regex.Split(lines, "\n\n");
            int progress = 0;
            Recipe r = null;
            while (progress < linees.Length)
            {
                String line = linees[progress];
                if (line.StartsWith("Ingredients"))
                {
                    if (progress > 3)
                        throw new ChefException(global::BesiegeScriptingMod.Chef.ChefException.STRUCTURAL, "Read unexpected " + progressToExpected(2) + ". Expecting " + progressToExpected(progress));
                    progress = 3;
                    r.setIngredients(line);
                }
                else if (line.StartsWith("Cooking time"))
                {
                    if (progress > 4)
                        throw new ChefException(global::BesiegeScriptingMod.Chef.ChefException.STRUCTURAL,
                            "Read unexpected " + progressToExpected(3) + ". Expecting " + progressToExpected(progress));
                    progress = 4;
                    r.setCookingTime(line);
                }
                else if (line.StartsWith("Pre-heat oven"))
                {
                    if (progress > 5)
                        throw new ChefException(global::BesiegeScriptingMod.Chef.ChefException.STRUCTURAL,
                            "Read unexpected " + progressToExpected(4) + ". Expecting " + progressToExpected(progress));
                    progress = 5;
                    r.setOvenTemp(line);
                }
                else if (line.StartsWith("Method"))
                {
                    if (progress > 5)
                        throw new ChefException(global::BesiegeScriptingMod.Chef.ChefException.STRUCTURAL,
                            "Read unexpected " + progressToExpected(5) + ". Expecting " + progressToExpected(progress));
                    progress = 6;
                    r.setMethod(line);
                }
                else if (line.StartsWith("Serves"))
                {
                    if (progress != 6)
                        throw new ChefException(global::BesiegeScriptingMod.Chef.ChefException.STRUCTURAL,
                            "Read unexpected " + progressToExpected(6) + ". Expecting " + progressToExpected(progress));
                    progress = 0;
                    r.setServes(line);
                }
                else
                {
                    if (progress == 0 || progress >= 6)
                    {
                        r = new Recipe(line);
                        if (mainrecipe == null)
                            mainrecipe = r;
                        progress = 1;
                        recipes.Add(parseTitle(line), r);
                    }
                    else if (progress == 1)
                    {
                        progress = 2;
                        r.setComments(line);
                    }
                    else
                    {
                        throw new ChefException(global::BesiegeScriptingMod.Chef.ChefException.STRUCTURAL,
                            "Read unexpected comments/title. Expecting " + progressToExpected(progress) + " Hint:" +
                            structHint(progress));
                    }
                }
            }
            if (mainrecipe == null)
                throw new ChefException(global::BesiegeScriptingMod.Chef.ChefException.STRUCTURAL, "Recipe empty or title missing!");
            //System.out.println("Done!");
        }

        private String parseTitle(String title)
        {
            if (title[title.Length - 1] == '.')
            {
                title =title.Substring(0, title.Length - 1);
            }
            return title.ToLower();
        }

        private String structHint(int progress)
        {
            switch (progress)
            {
                case 2:
                    return "did you specify 'Ingredients.' above the ingredient list?";
                case 3:
                    return "did you specify 'Methods.' above the methods?";
            }
            return "no hint available";
        }

        private String progressToExpected(int progress)
        {
            switch (progress)
            {
                case 0:
                    return "title";
                case 1:
                    return "comments";
                case 2:
                    return "ingredient list";
                case 3:
                    return "cooking time";
                case 4:
                    return "oven temperature";
                case 5:
                    return "methods";
                case 6:
                    return "serve amount";
            }
            return null;
        }

        public void bake()
        {
            Kitchen k = new Kitchen(recipes, mainrecipe);
            k.cook();
        }
    }
}