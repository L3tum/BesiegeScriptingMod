#region usings

using System;

#endregion

namespace KeraLua
{
    public struct LuaState
    {
        public LuaState(IntPtr ptrState)
            : this()
        {
            state = ptrState;
        }

        public static implicit operator LuaState(IntPtr ptr)
        {
            return new LuaState(ptr);
        }


        public static implicit operator IntPtr(LuaState luastate)
        {
            return luastate.state;
        }

        private readonly IntPtr state;
    }
}