using System;
using System.Collections.Generic;
using spaar.ModLoader;
using UnityEngine;

namespace BesiegeScriptingMod
{
    class PlannedExecutionWrapper
    {
        public String Sauce;
        public String Refs;
        public String Name;
        public List<GameObject> gos;
        public String Ide;
        public Key key;

        public PlannedExecutionWrapper(String sauce, String refs, String name, List<GameObject> gos, String ide, Key key)
        {
            Sauce = sauce;
            Refs = refs;
            Name = name;
            this.gos = gos;
            Ide = ide;
            this.key = key;
        }
    }
}
