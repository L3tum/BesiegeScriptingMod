namespace BesiegeScriptingMod
{
    public class Script
    {
        public string refFile;
        public string sourceFile;

        public Script(string source, string refs)
        {
            sourceFile = source;
            refFile = refs;
        }
    }
}