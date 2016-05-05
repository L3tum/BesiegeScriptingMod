using ICSharpCode.NRefactory.Ast;
using System;
using System.Collections.Generic;

namespace ICSharpCode.PythonBinding
{
    public class PythonConstructorInfo
    {
        private ConstructorDeclaration constructor;

        private List<FieldDeclaration> fields = new List<FieldDeclaration>();

        public ConstructorDeclaration Constructor
        {
            get
            {
                return this.constructor;
            }
        }

        public List<FieldDeclaration> Fields
        {
            get
            {
                return this.fields;
            }
        }

        private PythonConstructorInfo(ConstructorDeclaration constructor, List<FieldDeclaration> fields)
        {
            this.constructor = constructor;
            this.fields = fields;
        }

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
            if ((fieldDeclarations.Count > 0 ? false : constructorDeclaration == null))
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