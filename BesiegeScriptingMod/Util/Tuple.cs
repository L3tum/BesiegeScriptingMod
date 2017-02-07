namespace BesiegeScriptingMod.Util
{
    public class Tuple<T1, T2>
    {
        internal Tuple(T1 first, T2 second)
        {
            First = first;
            Second = second;
        }

        public T1 First { get; internal set; }
        public T2 Second { get; internal set; }
    }

    public static class Tuple
    {
        public static Tuple<T1, T2> New<T1, T2>(T1 first, T2 second)
        {
            var tuple = new Tuple<T1, T2>(first, second);
            return tuple;
        }
    }
}