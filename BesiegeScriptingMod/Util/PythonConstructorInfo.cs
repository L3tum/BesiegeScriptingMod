#region usings

using System.Collections.Generic;
using ICSharpCode.NRefactory.Ast;

#endregion

namespace BesiegeScriptingMod.Util
{
    public class PythonConstructorInfo
    {
        private PythonConstructorInfo(ConstructorDeclaration constructor, List<FieldDeclaration> fields)
        {
            Constructor = constructor;
            Fields = fields;
        }

        public ConstructorDeclaration Constructor { get; }

        public List<FieldDeclaration> Fields { get; } = new List<FieldDeclaration>();

        public static PythonConstructorInfo GetConstructorInfo(TypeDeclaration type)
        {
            PythonConstructorInfo pythonConstructorInfo;
            List<FieldDeclaration> fieldDeclarations = new List<FieldDeclaration>();
            ConstructorDeclaration constructorDeclaration = null;
            foreach (INode child in type.Children)
            {
                ConstructorDeclaration constructorDeclaration1 = child as ConstructorDeclaration;
                FieldDeclaration fieldDeclaration = child as FieldDeclaration;
                if (constructorDeclaration1 != null)
                {
                    constructorDeclaration = constructorDeclaration1;
                }
                else if (fieldDeclaration != null)
                {
                    fieldDeclarations.Add(fieldDeclaration);
                }
            }
            if (fieldDeclarations.Count > 0 ? false : constructorDeclaration == null)
            {
                pythonConstructorInfo = null;
            }
            else
            {
                pythonConstructorInfo = new PythonConstructorInfo(constructorDeclaration, fieldDeclarations);
            }
            return pythonConstructorInfo;
        }
    }
}