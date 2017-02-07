#region usings

using System.Collections.Generic;
using BesiegeScriptingMod.Util;
using UnityEngine;

#endregion

namespace BesiegeScriptingMod.Extensions
{
    public abstract class LanguageExtension
    {
        /// <summary>
        /// Name of your language
        /// </summary>
        public abstract string name { get; }

        /// <summary>
        /// Whether the script needs to be converted before being executed
        /// </summary>
        public abstract bool needsConvertion { get; }

        /// <summary>
        /// The file-ending of your language (e.g. 'cs' for CSharp)
        /// </summary>
        public abstract string extension { get; }

        /// <summary>
        /// Method that is called when a script of your language should be executed
        /// </summary>
        /// <param name="refs">References declared for the Script</param>
        /// <param name="sauce">Source code of the Script</param>
        /// <param name="gos">GameObjects the Script should be attached to</param>
        /// <param name="sname">Name of the Script</param>
        /// <param name="addedScripts">List of Scripts. Every instance of your script you add to a GameObject needs to be added to this Dictionary</param>
        /// <returns>Boolean whether the execution worked(true) or failed(false)</returns>
        public abstract bool Execute(string refs, string sauce, List<GameObject> gos, string sname, ref Dictionary<Tuple<string, GameObject>, Component> addedScripts);

        /// <summary>
        /// Method that is called when a script of your language should be converted
        /// </summary>
        /// <param name="Sauce">Source code for the Script</param>
        /// <param name="sname">Name of the Script</param>
        /// <param name="refs">References declared for the Script</param>
        /// <returns>Source code that can be executed via the $Execute method</returns>
        public virtual string Convert(string Sauce, string sname, string refs)
        {
            return "";
        }
    }
}