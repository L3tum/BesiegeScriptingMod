#region usings

using System;

#endregion

namespace KeraLua
{
    public struct LuaTag
    {
        public LuaTag(IntPtr tag)
            : this()
        {
            Tag = tag;
        }

        public static implicit operator LuaTag(IntPtr ptr)
        {
            return new LuaTag(ptr);
        }

        public IntPtr Tag { get; set; }
    }
}