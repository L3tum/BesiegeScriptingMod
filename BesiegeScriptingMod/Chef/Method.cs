#region usings

using System.Text.RegularExpressions;

#endregion

namespace BesiegeScriptingMod.Chef
{
    public class Method
    {
        public enum Type
        {
            Take,
            Put,
            Fold,
            Add,
            Remove,
            Combine,
            Divide,
            AddDry,
            Liquefy,
            LiquefyBowl,
            Stir,
            StirInto,
            Mix,
            Clean,
            Pour,
            Verb,
            VerbUntil,
            SetAside,
            Serve,
            Refrigerate,
            Remember
        }

        public string auxrecipe;
        public int bakingdish;

        public string ingredient;
        public int mixingbowl;
        public int n;
        public int time; //'Refrigerate for number of hours' / 'Stir for number of minutes'
        public Type type;
        public string verb;

        public Method(string line, int n)
        {
            line = line.Trim();
            this.n = n;
            Match[] matchers =
            {
                Regex.Match("^Take ([a-zA-Z ]+) from refrigerator.$", line),
                Regex.Match("^(Put|Fold) ([a-zA-Z ]+) into( the)?( (\\d+)(nd|rd|th|st))? mixing bowl.$", line),
                Regex.Match("^Add dry ingredients( to( (\\d+)(nd|rd|th|st))? mixing bowl)?.$", line),
                Regex.Match(
                    "^(Add|Remove|Combine|Divide) ([a-zA-Z ]+)( (to|into|from)( (\\d+)(nd|rd|th|st))? mixing bowl)?.$",
                    line),
                Regex.Match("^Liquefy contents of the( (\\d+)(nd|rd|th|st))? mixing bowl.$", line),
                Regex.Match("^Liquefy ([a-zA-Z ]+).$", line),
                Regex.Match("^Stir( the( (\\d+)(nd|rd|th|st))? mixing bowl)? for (\\d+) minutes.$", line),
                Regex.Match("^Stir ([a-zA-Z ]+) into the( (\\d+)(nd|rd|th|st))? mixing bowl.$", line),
                Regex.Match("^Mix( the( (\\d+)(nd|rd|th|st))? mixing bowl)? well.$", line),
                Regex.Match("^Clean( (\\d+)(nd|rd|th|st))? mixing bowl.$", line),
                Regex.Match(
                    "^Pour contents of the( (\\d+)(nd|rd|th|st))? mixing bowl into the( (\\d+)(nd|rd|th|st))? baking dish.$",
                    line),
                Regex.Match("^Set aside.$", line),
                Regex.Match("^Refrigerate( for (\\d+) hours)?.$", line),
                Regex.Match("^Serve with ([a-zA-Z ]+).$", line),
                Regex.Match("^Suggestion: (.*).$", line),
                Regex.Match("^([a-zA-Z]+)( the ([a-zA-Z ]+))? until ([a-zA-Z]+).$", line),
                Regex.Match("^([a-zA-Z]+) the ([a-zA-Z ]+).$", line)
            };
            if (matchers[0].Success)
            {
                ingredient = matchers[0].Groups[1].Value;
                type = Type.Take;
            }
            else if (matchers[1].Success)
            {
                type = matchers[1].Groups[1].Value.Equals("Put") ? Type.Put : Type.Fold;
                ingredient = matchers[1].Groups[2].Value;
                mixingbowl = (string.IsNullOrEmpty(matchers[1].Groups[5].Value)
                    ? 1
                    : int.Parse(matchers[1].Groups[5].Value)) - 1;
            }
            else if (matchers[2].Success)
            {
                type = Type.AddDry;
                mixingbowl = (string.IsNullOrEmpty(matchers[2].Groups[3].Value)
                    ? 1
                    : int.Parse(matchers[2].Groups[3].Value)) - 1;
            }
            else if (matchers[3].Success)
            {
                type = matchers[3].Groups[1].Value.Equals("Add")
                    ? Type.Add
                    : (matchers[3].Groups[1].Value.Equals("Remove")
                        ? Type.Remove
                        : (matchers[3].Groups[1].Value.Equals("Combine") ? Type.Combine : Type.Divide));
                ingredient = matchers[3].Groups[2].Value;
                mixingbowl = (string.IsNullOrEmpty(matchers[3].Groups[6].Value)
                    ? 1
                    : int.Parse(matchers[3].Groups[6].Value)) - 1;
            }
            else if (matchers[4].Success)
            {
                type = Type.LiquefyBowl;
                mixingbowl = (string.IsNullOrEmpty(matchers[4].Groups[2].Value)
                    ? 1
                    : int.Parse(matchers[4].Groups[2].Value)) - 1;
            }
            else if (matchers[5].Success)
            {
                type = Type.Liquefy;
                ingredient = matchers[5].Groups[1].Value;
            }
            else if (matchers[6].Success)
            {
                type = Type.Stir;
                mixingbowl = (string.IsNullOrEmpty(matchers[6].Groups[3].Value)
                    ? 1
                    : int.Parse(matchers[6].Groups[3].Value)) - 1;
                time = int.Parse(matchers[6].Groups[5].Value);
            }
            //Stir ingredient into the [nth] mixing bowl.
            else if (matchers[7].Success)
            {
                type = Type.StirInto;
                ingredient = matchers[7].Groups[1].Value;
                mixingbowl = (string.IsNullOrEmpty(matchers[7].Groups[3].Value)
                    ? 1
                    : int.Parse(matchers[7].Groups[3].Value)) - 1;
            }
            else if (matchers[8].Success)
            {
                type = Type.Mix;
                mixingbowl = (string.IsNullOrEmpty(matchers[8].Groups[3].Value)
                    ? 1
                    : int.Parse(matchers[8].Groups[3].Value)) - 1;
            }
            else if (matchers[9].Success)
            {
                type = Type.Clean;
                mixingbowl = (string.IsNullOrEmpty(matchers[9].Groups[2].Value)
                    ? 1
                    : int.Parse(matchers[9].Groups[2].Value)) - 1;
            }
            else if (matchers[10].Success)
            {
                type = Type.Pour;
                mixingbowl = (string.IsNullOrEmpty(matchers[10].Groups[2].Value)
                    ? 1
                    : int.Parse(matchers[10].Groups[2].Value)) - 1;
                bakingdish = (string.IsNullOrEmpty(matchers[10].Groups[5].Value)
                    ? 1
                    : int.Parse(matchers[10].Groups[5].Value)) - 1;
            }
            else if (matchers[11].Success)
            {
                type = Type.SetAside;
            }
            else if (matchers[12].Success)
            {
                type = Type.Refrigerate;
                time = string.IsNullOrEmpty(matchers[12].Groups[2].Value) ? 0 : int.Parse(matchers[12].Groups[2].Value);
            }
            else if (matchers[13].Success)
            {
                type = Type.Serve;
                auxrecipe = matchers[13].Groups[1].Value;
            }
            else if (matchers[14].Success)
            {
                type = Type.Remember;
            }
            else if (matchers[15].Success)
            {
                type = Type.VerbUntil;
                verb = matchers[15].Groups[4].Value;
                ingredient = matchers[15].Groups[3].Value;
            }
            else if (matchers[16].Success)
            {
                type = Type.Verb;
                verb = matchers[16].Groups[1].Value;
                ingredient = matchers[16].Groups[2].Value;
            }
            else
                throw new ChefException(ChefException.METHOD, "Unsupported method found!");
        }
    }
}