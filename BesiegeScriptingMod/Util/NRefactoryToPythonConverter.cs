﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using BesiegeScriptingMod.Python;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.Ast;
using ICSharpCode.NRefactory.PrettyPrinter;
using ICSharpCode.NRefactory.Visitors;

namespace BesiegeScriptingMod.Util
{
    public class NRefactoryToPythonConverter : NodeTrackingAstVisitor, IOutputFormatter
    {
        private string indentString = "\t";

        private PythonCodeBuilder codeBuilder;

        private PythonConstructorInfo constructorInfo;

        private List<ParameterDeclarationExpression> methodParameters = new List<ParameterDeclarationExpression>();

        private MethodDeclaration currentMethod;

        private List<string> propertyNames = new List<string>();

        private SupportedLanguage language;

        private List<MethodDeclaration> entryPointMethods;

        private SpecialNodesInserter specialNodesInserter;

        private INode currentNode;

        private List<Comment> xmlDocComments = new List<Comment>();

        private readonly static string Docstring;

        public ReadOnlyCollection<MethodDeclaration> EntryPointMethods
        {
            get
            {
                return this.entryPointMethods.AsReadOnly();
            }
        }

        int ICSharpCode.NRefactory.PrettyPrinter.IOutputFormatter.IndentationLevel
        {
            get
            {
                return this.codeBuilder.Indent;
            }
            set
            {
            }
        }

        bool ICSharpCode.NRefactory.PrettyPrinter.IOutputFormatter.IsInMemberBody
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        string ICSharpCode.NRefactory.PrettyPrinter.IOutputFormatter.Text
        {
            get
            {
                return string.Empty;
            }
        }

        public string IndentString
        {
            get
            {
                return this.indentString;
            }
            set
            {
                this.indentString = value;
            }
        }

        public SupportedLanguage SupportedLanguage
        {
            get
            {
                return this.language;
            }
        }

        static NRefactoryToPythonConverter()
        {
            NRefactoryToPythonConverter.Docstring = "\"\"\"";
        }

        public NRefactoryToPythonConverter(SupportedLanguage language)
        {
            this.language = language;
        }

        public NRefactoryToPythonConverter()
        {
        }

        private void AddParameters(ParametrizedNode method)
        {
            this.Append("(");
            List<ParameterDeclarationExpression> parameters = method.Parameters;
            if (parameters.Count > 0)
            {
                if (!this.IsStatic(method))
                {
                    this.Append("self, ");
                }
                for (int i = 0; i < parameters.Count; i++)
                {
                    if (i > 0)
                    {
                        this.Append(", ");
                    }
                    this.Append(parameters[i].ParameterName);
                }
            }
            else if (!this.IsStatic(method))
            {
                this.Append("self");
            }
            this.Append("):");
        }

        private void Append(string code)
        {
            this.codeBuilder.Append(code);
        }

        private void AppendBaseTypes(List<TypeReference> baseTypes)
        {
            this.Append("(");
            if (baseTypes.Count != 0)
            {
                for (int i = 0; i < baseTypes.Count; i++)
                {
                    TypeReference item = baseTypes[i];
                    if (i > 0)
                    {
                        this.Append(", ");
                    }
                    this.Append(this.GetTypeName(item));
                }
            }
            else
            {
                this.Append("object");
            }
            this.Append("):");
        }

        private void AppendDocstring(List<Comment> xmlDocComments)
        {
            if (xmlDocComments.Count > 1)
            {
                for (int i = 0; i < xmlDocComments.Count; i++)
                {
                    string commentText = xmlDocComments[i].CommentText;
                    if (i != 0)
                    {
                        this.AppendIndented(string.Empty);
                    }
                    else
                    {
                        this.AppendIndented(NRefactoryToPythonConverter.Docstring);
                    }
                    this.Append(commentText);
                    this.AppendLine();
                }
                this.AppendIndentedLine(NRefactoryToPythonConverter.Docstring);
            }
            else if (xmlDocComments.Count == 1)
            {
                this.AppendIndentedLine(string.Concat(NRefactoryToPythonConverter.Docstring, xmlDocComments[0].CommentText, NRefactoryToPythonConverter.Docstring));
            }
        }

        private void AppendForeachVariableName(ForeachStatement foreachStatement)
        {
            IdentifierExpression expression = foreachStatement.Expression as IdentifierExpression;
            InvocationExpression invocationExpression = foreachStatement.Expression as InvocationExpression;
            MemberReferenceExpression memberReferenceExpression = foreachStatement.Expression as MemberReferenceExpression;
            if (expression != null)
            {
                this.Append(expression.Identifier);
            }
            else if (invocationExpression != null)
            {
                invocationExpression.AcceptVisitor(this, null);
            }
            else if (memberReferenceExpression != null)
            {
                memberReferenceExpression.AcceptVisitor(this, null);
            }
        }

        private void AppendGenericTypes(ObjectCreateExpression expression)
        {
            this.Append("[");
            List<TypeReference> genericTypes = expression.CreateType.GenericTypes;
            for (int i = 0; i < genericTypes.Count; i++)
            {
                if (i != 0)
                {
                    this.Append(", ");
                }
                TypeReference item = genericTypes[i];
                if (!item.IsArrayType)
                {
                    this.Append(this.GetTypeName(item));
                }
                else
                {
                    this.Append(string.Concat("Array[", this.GetTypeName(item), "]"));
                }
            }
            this.Append("]");
        }

        private void AppendIndented(string code)
        {
            this.codeBuilder.AppendIndented(code);
        }

        private void AppendIndentedLine(string code)
        {
            this.codeBuilder.AppendIndentedLine(code);
        }

        private void AppendIndentedPassStatement()
        {
            this.AppendIndentedLine("pass");
        }

        private void AppendLine()
        {
            this.codeBuilder.AppendLine();
        }

        private void AppendMultilineComment(Comment comment)
        {
            string[] strArrays = comment.CommentText.Split(new char[] { '\n' });
            for (int i = 0; i < (int)strArrays.Length; i++)
            {
                string str = string.Concat("# ", strArrays[i].Trim());
                if ((i != 0 ? true : comment.CommentStartsLine))
                {
                    this.AppendIndentedLine(str);
                }
                else
                {
                    this.codeBuilder.AppendToPreviousLine(string.Concat(" ", str));
                }
            }
        }

        private void AppendPropertyDecorator(PropertyDeclaration propertyDeclaration)
        {
            string name = propertyDeclaration.Name;
            this.AppendIndented(name);
            this.Append(" = property(");
            bool flag = false;
            if (propertyDeclaration.HasGetRegion)
            {
                this.Append(string.Concat("fget=get_", name));
                flag = true;
            }
            if (propertyDeclaration.HasSetRegion)
            {
                if (flag)
                {
                    this.Append(", ");
                }
                this.Append(string.Concat("fset=set_", name));
            }
            this.Append(")");
            this.AppendLine();
        }

        private void AppendSingleLineComment(Comment comment)
        {
            if (!comment.CommentStartsLine)
            {
                this.codeBuilder.AppendToPreviousLine(string.Concat(" #", comment.CommentText));
            }
            else
            {
                this.codeBuilder.AppendIndentedLine(string.Concat("#", comment.CommentText));
            }
        }

        protected override void BeginVisit(INode node)
        {
            this.xmlDocComments.Clear();
            this.currentNode = node;
            this.specialNodesInserter.AcceptNodeStart(node);
        }

        public static bool CanConvert(string fileName)
        {
            bool flag;
            string extension = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(extension))
            {
                flag = false;
            }
            else
            {
                extension = extension.ToLowerInvariant();
                flag = (extension == ".cs" ? true : extension == ".vb");
            }
            return flag;
        }

        public string Convert(string source)
        {
            return this.Convert(source, this.language);
        }

        public string Convert(string source, SupportedLanguage language)
        {
            CompilationUnit compilationUnit = this.GenerateCompilationUnit(source, language);
            SpecialOutputVisitor specialOutputVisitor = new SpecialOutputVisitor(this);
            this.specialNodesInserter = new SpecialNodesInserter(compilationUnit.UserData as List<ISpecial>, specialOutputVisitor);
            this.entryPointMethods = new List<MethodDeclaration>();
            this.codeBuilder = new PythonCodeBuilder()
            {
                IndentString = this.indentString
            };
            compilationUnit.AcceptVisitor(this, null);
            return this.codeBuilder.ToString().Trim();
        }

        public static NRefactoryToPythonConverter Create(string fileName)
        {
            NRefactoryToPythonConverter nRefactoryToPythonConverter;
            if (!NRefactoryToPythonConverter.CanConvert(fileName))
            {
                nRefactoryToPythonConverter = null;
            }
            else
            {
                nRefactoryToPythonConverter = new NRefactoryToPythonConverter(NRefactoryToPythonConverter.GetSupportedLanguage(fileName));
            }
            return nRefactoryToPythonConverter;
        }

        private void CreateConstructor(PythonConstructorInfo constructorInfo)
        {
            if (constructorInfo.Constructor == null)
            {
                this.AppendIndented("def __init__(self):");
            }
            else
            {
                this.AppendIndented("def __init__");
                this.AddParameters(constructorInfo.Constructor);
                this.methodParameters = constructorInfo.Constructor.Parameters;
            }
            this.AppendLine();
            this.IncreaseIndent();
            this.AppendDocstring(this.xmlDocComments);
            if (constructorInfo.Fields.Count > 0)
            {
                foreach (FieldDeclaration field in constructorInfo.Fields)
                {
                    if (NRefactoryToPythonConverter.FieldHasInitialValue(field))
                    {
                        this.CreateFieldInitialization(field);
                    }
                }
            }
            if (!NRefactoryToPythonConverter.IsEmptyConstructor(constructorInfo.Constructor))
            {
                constructorInfo.Constructor.Body.AcceptVisitor(this, null);
                this.AppendLine();
            }
            else if (constructorInfo.Fields.Count != 0)
            {
                this.AppendLine();
            }
            else
            {
                this.AppendIndentedPassStatement();
            }
            this.DecreaseIndent();
        }

        private object CreateDecrementStatement(UnaryOperatorExpression unaryOperatorExpression)
        {
            object obj = this.CreateIncrementStatement(unaryOperatorExpression, 1, NRefactoryToPythonConverter.GetBinaryOperator(BinaryOperatorType.Subtract));
            return obj;
        }

        private object CreateDelegateCreateExpression(Expression eventHandlerExpression)
        {
            IdentifierExpression identifierExpression = eventHandlerExpression as IdentifierExpression;
            ObjectCreateExpression objectCreateExpression = eventHandlerExpression as ObjectCreateExpression;
            MemberReferenceExpression memberReferenceExpression = eventHandlerExpression as MemberReferenceExpression;
            if (identifierExpression != null)
            {
                this.Append(string.Concat("self.", identifierExpression.Identifier));
            }
            else if (memberReferenceExpression != null)
            {
                memberReferenceExpression.AcceptVisitor(this, null);
            }
            else if (objectCreateExpression != null)
            {
                this.CreateDelegateCreateExpression(objectCreateExpression.Parameters[0]);
            }
            return null;
        }

        private object CreateEventReferenceExpression(Expression eventExpression)
        {
            (eventExpression as MemberReferenceExpression).AcceptVisitor(this, null);
            return null;
        }

        private void CreateFieldInitialization(FieldDeclaration field)
        {
            VariableDeclaration item = field.Fields[0];
            string name = item.Name;
            item.Name = string.Concat("self._", item.Name);
            this.VisitVariableDeclaration(item, null);
            item.Name = name;
        }

        private object CreateHandlerStatement(Expression eventExpression, string addRemoveOperator, Expression eventHandlerExpression)
        {
            this.CreateEventReferenceExpression(eventExpression);
            this.Append(string.Concat(" ", addRemoveOperator, " "));
            this.CreateDelegateCreateExpression(eventHandlerExpression);
            return null;
        }

        private object CreateIncrementStatement(UnaryOperatorExpression unaryOperatorExpression)
        {
            object obj = this.CreateIncrementStatement(unaryOperatorExpression, 1, NRefactoryToPythonConverter.GetBinaryOperator(BinaryOperatorType.Add));
            return obj;
        }

        private object CreateIncrementStatement(UnaryOperatorExpression unaryOperatorExpression, int increment, string binaryOperator)
        {
            unaryOperatorExpression.Expression.AcceptVisitor(this, null);
            this.Append(string.Concat(" ", binaryOperator, "= "));
            this.Append(increment.ToString());
            return null;
        }

        private object CreateInitStatement(ForeachStatement foreachStatement)
        {
            this.Append("enumerator = ");
            this.AppendForeachVariableName(foreachStatement);
            this.Append(".GetEnumerator()");
            return null;
        }

        private object CreateSimpleAssignment(AssignmentExpression assignmentExpression, string op, object data)
        {
            assignmentExpression.Left.AcceptVisitor(this, data);
            this.Append(string.Concat(" ", op, " "));
            assignmentExpression.Right.AcceptVisitor(this, data);
            return null;
        }

        private void CreateSwitchCaseBody(SwitchSection section)
        {
            int num = 0;
            foreach (INode child in section.Children)
            {
                if (!(child is BreakStatement))
                {
                    num++;
                    child.AcceptVisitor(this, null);
                }
            }
            if (num == 0)
            {
                this.AppendIndentedLine("pass");
            }
        }

        private void CreateSwitchCaseCondition(Expression switchExpression, SwitchSection section, bool firstSection)
        {
            bool flag = true;
            foreach (CaseLabel switchLabel in section.SwitchLabels)
            {
                if (!flag)
                {
                    this.CreateSwitchCaseCondition(" or ", switchExpression, switchLabel);
                }
                else if (switchLabel.IsDefault)
                {
                    this.AppendIndented("else");
                }
                else if (!firstSection)
                {
                    this.AppendIndented(string.Empty);
                    this.CreateSwitchCaseCondition("elif ", switchExpression, switchLabel);
                }
                else
                {
                    this.AppendIndented(string.Empty);
                    this.CreateSwitchCaseCondition("if ", switchExpression, switchLabel);
                }
                flag = false;
            }
            this.Append(":");
            this.AppendLine();
        }

        private void CreateSwitchCaseCondition(string prefix, Expression switchExpression, CaseLabel label)
        {
            this.Append(prefix);
            switchExpression.AcceptVisitor(this, null);
            this.Append(" == ");
            label.Label.AcceptVisitor(this, null);
        }

        private object CreateUnaryOperatorStatement(string op, Expression expression)
        {
            this.Append(op);
            expression.AcceptVisitor(this, null);
            return null;
        }

        private void DecreaseIndent()
        {
            this.codeBuilder.DecreaseIndent();
        }

        private static bool FieldHasInitialValue(FieldDeclaration fieldDeclaration)
        {
            return !fieldDeclaration.Fields[0].Initializer.IsNull;
        }

        public CompilationUnit GenerateCompilationUnit(string source, SupportedLanguage language)
        {
            CompilationUnit compilationUnit;
            IParser parser = ParserFactory.CreateParser(language, new StringReader(source));
            try
            {
                parser.Parse();
                parser.CompilationUnit.UserData = parser.Lexer.SpecialTracker.RetrieveSpecials();
                compilationUnit = parser.CompilationUnit;
            }
            finally
            {
                if (parser != null)
                {
                    parser.Dispose();
                }
            }
            return compilationUnit;
        }

        public string GenerateMainMethodCall(MethodDeclaration methodDeclaration)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(NRefactoryToPythonConverter.GetTypeName(methodDeclaration));
            stringBuilder.Append('.');
            stringBuilder.Append(methodDeclaration.Name);
            stringBuilder.Append('(');
            if (methodDeclaration.Parameters.Count > 0)
            {
                stringBuilder.Append("None");
            }
            stringBuilder.Append(')');
            return stringBuilder.ToString();
        }

        public static string GetBinaryOperator(BinaryOperatorType binaryOperatorType)
        {
            string str;
            switch (binaryOperatorType)
            {
                case BinaryOperatorType.BitwiseAnd:
                    {
                        str = "&";
                        break;
                    }
                case BinaryOperatorType.BitwiseOr:
                    {
                        str = "|";
                        break;
                    }
                case BinaryOperatorType.LogicalAnd:
                    {
                        str = "and";
                        break;
                    }
                case BinaryOperatorType.LogicalOr:
                    {
                        str = "or";
                        break;
                    }
                case BinaryOperatorType.ExclusiveOr:
                    {
                        str = "^";
                        break;
                    }
                case BinaryOperatorType.GreaterThan:
                    {
                        str = ">";
                        break;
                    }
                case BinaryOperatorType.GreaterThanOrEqual:
                    {
                        str = ">=";
                        break;
                    }
                case BinaryOperatorType.Equality:
                case BinaryOperatorType.Power:
                    {
                        str = "==";
                        break;
                    }
                case BinaryOperatorType.InEquality:
                    {
                        str = "!=";
                        break;
                    }
                case BinaryOperatorType.LessThan:
                    {
                        str = "<";
                        break;
                    }
                case BinaryOperatorType.LessThanOrEqual:
                    {
                        str = "<=";
                        break;
                    }
                case BinaryOperatorType.Add:
                    {
                        str = "+";
                        break;
                    }
                case BinaryOperatorType.Subtract:
                    {
                        str = "-";
                        break;
                    }
                case BinaryOperatorType.Multiply:
                    {
                        str = "*";
                        break;
                    }
                case BinaryOperatorType.Divide:
                case BinaryOperatorType.DivideInteger:
                    {
                        str = "/";
                        break;
                    }
                case BinaryOperatorType.Modulus:
                    {
                        str = "%";
                        break;
                    }
                case BinaryOperatorType.Concat:
                    {
                        str = "+";
                        break;
                    }
                case BinaryOperatorType.ShiftLeft:
                    {
                        str = "<<";
                        break;
                    }
                case BinaryOperatorType.ShiftRight:
                    {
                        str = ">>";
                        break;
                    }
                case BinaryOperatorType.ReferenceEquality:
                    {
                        str = "is";
                        break;
                    }
                default:
                    {
                        goto case BinaryOperatorType.Power;
                    }
            }
            return str;
        }

        private static SupportedLanguage GetSupportedLanguage(string fileName)
        {
            SupportedLanguage supportedLanguage;
            supportedLanguage = (!(Path.GetExtension(fileName.ToLowerInvariant()) == ".vb") ? SupportedLanguage.CSharp : SupportedLanguage.VBNet);
            return supportedLanguage;
        }

        private string GetTypeName(TypeReference typeRef)
        {
            string str;
            string type = typeRef.Type;
            if (type == typeof(string).FullName)
            {
                str = "str";
            }
            else if ((type == typeof(int).FullName ? false : !(type == typeof(int).Name)))
            {
                if (typeRef.IsKeyword)
                {
                    int num = type.LastIndexOf('.');
                    if (num > 0)
                    {
                        str = type.Substring(num + 1);
                        return str;
                    }
                }
                str = type;
            }
            else
            {
                str = "int";
            }
            return str;
        }

        private static string GetTypeName(MethodDeclaration methodDeclaration)
        {
            return (methodDeclaration.Parent as TypeDeclaration).Name;
        }

        void ICSharpCode.NRefactory.PrettyPrinter.IOutputFormatter.Indent()
        {
        }

        void ICSharpCode.NRefactory.PrettyPrinter.IOutputFormatter.NewLine()
        {
        }

        void ICSharpCode.NRefactory.PrettyPrinter.IOutputFormatter.PrintBlankLine(bool forceWriteInPreviousBlock)
        {
        }

        void ICSharpCode.NRefactory.PrettyPrinter.IOutputFormatter.PrintComment(Comment comment, bool forceWriteInPreviousBlock)
        {
            if (comment.CommentType == CommentType.SingleLine)
            {
                this.AppendSingleLineComment(comment);
            }
            else if (comment.CommentType == CommentType.Block)
            {
                this.AppendMultilineComment(comment);
            }
            else if (comment.CommentType == CommentType.Documentation)
            {
                if (!this.SupportsDocstring(this.currentNode))
                {
                    this.AppendSingleLineComment(comment);
                }
                else
                {
                    this.xmlDocComments.Add(comment);
                }
            }
        }

        void ICSharpCode.NRefactory.PrettyPrinter.IOutputFormatter.PrintPreprocessingDirective(PreprocessingDirective directive, bool forceWriteInPreviousBlock)
        {
        }

        private void IncreaseIndent()
        {
            this.codeBuilder.IncreaseIndent();
        }

        private static bool IsAddEventHandler(AssignmentExpression assignmentExpression)
        {
            return (assignmentExpression.Op != AssignmentOperatorType.Add ? false : assignmentExpression.Left is MemberReferenceExpression);
        }

        private static bool IsEmptyConstructor(ConstructorDeclaration constructor)
        {
            bool flag;
            flag = (constructor == null ? true : constructor.Body.Children.Count == 0);
            return flag;
        }

        private bool IsField(string name)
        {
            bool flag;
            if (!this.IsMethodParameter(name))
            {
                if (this.constructorInfo != null)
                {
                    foreach (FieldDeclaration field in this.constructorInfo.Fields)
                    {
                        if (field.Fields[0].Name == name)
                        {
                            flag = true;
                            return flag;
                        }
                    }
                }
                flag = false;
            }
            else
            {
                flag = false;
            }
            return flag;
        }

        private static bool IsGenericType(ObjectCreateExpression expression)
        {
            return expression.CreateType.GenericTypes.Count > 0;
        }

        private bool IsMethodParameter(string name)
        {
            bool flag;
            foreach (ParameterDeclarationExpression methodParameter in this.methodParameters)
            {
                if (methodParameter.ParameterName == name)
                {
                    flag = true;
                    return flag;
                }
            }
            flag = false;
            return flag;
        }

        private bool IsProperty(string name)
        {
            return this.propertyNames.Contains(name);
        }

        private static bool IsRemoveEventHandler(AssignmentExpression assignmentExpression)
        {
            return (assignmentExpression.Op != AssignmentOperatorType.Subtract ? false : assignmentExpression.Left is MemberReferenceExpression);
        }

        private bool IsStatic(ParametrizedNode method)
        {
            return (method.Modifier & Modifiers.Static) == Modifiers.Static;
        }

        private void SaveMethodIfMainEntryPoint(MethodDeclaration method)
        {
            if (method.Name == "Main")
            {
                this.entryPointMethods.Add(method);
            }
        }

        private bool SupportsDocstring(INode node)
        {
            return (node is TypeDeclaration || node is MethodDeclaration ? true : node is ConstructorDeclaration);
        }

        public override object TrackedVisitAddHandlerStatement(AddHandlerStatement addHandlerStatement, object data)
        {
            Console.WriteLine("VisitAddHandlerStatement");
            return null;
        }

        public override object TrackedVisitAddressOfExpression(AddressOfExpression addressOfExpression, object data)
        {
            Console.WriteLine("VisitAddressOfExpression");
            return null;
        }

        public override object TrackedVisitAnonymousMethodExpression(AnonymousMethodExpression anonymousMethodExpression, object data)
        {
            Console.WriteLine("VisitAnonymousMethodExpression");
            return null;
        }

        public override object TrackedVisitArrayCreateExpression(ArrayCreateExpression arrayCreateExpression, object data)
        {
            string typeName = this.GetTypeName(arrayCreateExpression.CreateType);
            if (arrayCreateExpression.ArrayInitializer.CreateExpressions.Count != 0)
            {
                this.Append(string.Concat("Array[", typeName, "]"));
                this.Append("((");
                bool flag = true;
                foreach (Expression createExpression in arrayCreateExpression.ArrayInitializer.CreateExpressions)
                {
                    if (!flag)
                    {
                        this.Append(", ");
                    }
                    else
                    {
                        flag = false;
                    }
                    createExpression.AcceptVisitor(this, data);
                }
                this.Append("))");
            }
            else
            {
                this.Append(string.Concat("Array.CreateInstance(", typeName));
                if (arrayCreateExpression.Arguments.Count <= 0)
                {
                    this.Append(", 0)");
                }
                else
                {
                    foreach (Expression argument in arrayCreateExpression.Arguments)
                    {
                        this.Append(", ");
                        argument.AcceptVisitor(this, data);
                    }
                    this.Append(")");
                }
            }
            return null;
        }

        public override object TrackedVisitAssignmentExpression(AssignmentExpression assignmentExpression, object data)
        {
            object obj;
            switch (assignmentExpression.Op)
            {
                case AssignmentOperatorType.Assign:
                    {
                        obj = this.CreateSimpleAssignment(assignmentExpression, "=", data);
                        break;
                    }
                case AssignmentOperatorType.Add:
                    {
                        if (!NRefactoryToPythonConverter.IsAddEventHandler(assignmentExpression))
                        {
                            obj = this.CreateSimpleAssignment(assignmentExpression, "+=", data);
                            break;
                        }
                        else
                        {
                            obj = this.CreateHandlerStatement(assignmentExpression.Left, "+=", assignmentExpression.Right);
                            break;
                        }
                    }
                case AssignmentOperatorType.Subtract:
                    {
                        if (!NRefactoryToPythonConverter.IsRemoveEventHandler(assignmentExpression))
                        {
                            obj = this.CreateSimpleAssignment(assignmentExpression, "-=", data);
                            break;
                        }
                        else
                        {
                            obj = this.CreateHandlerStatement(assignmentExpression.Left, "-=", assignmentExpression.Right);
                            break;
                        }
                    }
                case AssignmentOperatorType.Multiply:
                    {
                        obj = this.CreateSimpleAssignment(assignmentExpression, "*=", data);
                        break;
                    }
                case AssignmentOperatorType.Divide:
                case AssignmentOperatorType.DivideInteger:
                    {
                        obj = this.CreateSimpleAssignment(assignmentExpression, "/=", data);
                        break;
                    }
                case AssignmentOperatorType.Modulus:
                    {
                        obj = this.CreateSimpleAssignment(assignmentExpression, "%=", data);
                        break;
                    }
                case AssignmentOperatorType.Power:
                    {
                        obj = this.CreateSimpleAssignment(assignmentExpression, "**=", data);
                        break;
                    }
                case AssignmentOperatorType.ConcatString:
                    {
                        obj = this.CreateSimpleAssignment(assignmentExpression, "+=", data);
                        break;
                    }
                case AssignmentOperatorType.ShiftLeft:
                    {
                        obj = this.CreateSimpleAssignment(assignmentExpression, "<<=", data);
                        break;
                    }
                case AssignmentOperatorType.ShiftRight:
                    {
                        obj = this.CreateSimpleAssignment(assignmentExpression, ">>=", data);
                        break;
                    }
                case AssignmentOperatorType.BitwiseAnd:
                    {
                        obj = this.CreateSimpleAssignment(assignmentExpression, "&=", data);
                        break;
                    }
                case AssignmentOperatorType.BitwiseOr:
                    {
                        obj = this.CreateSimpleAssignment(assignmentExpression, "|=", data);
                        break;
                    }
                case AssignmentOperatorType.ExclusiveOr:
                    {
                        obj = this.CreateSimpleAssignment(assignmentExpression, "^=", data);
                        break;
                    }
                default:
                    {
                        obj = null;
                        break;
                    }
            }
            return obj;
        }

        public override object TrackedVisitAttributeSection(AttributeSection attributeSection, object data)
        {
            return null;
        }

        public override object TrackedVisitBaseReferenceExpression(BaseReferenceExpression baseReferenceExpression, object data)
        {
            this.Append("self");
            return null;
        }

        public override object TrackedVisitBinaryOperatorExpression(BinaryOperatorExpression binaryOperatorExpression, object data)
        {
            binaryOperatorExpression.Left.AcceptVisitor(this, data);
            this.Append(" ");
            this.Append(NRefactoryToPythonConverter.GetBinaryOperator(binaryOperatorExpression.Op));
            this.Append(" ");
            binaryOperatorExpression.Right.AcceptVisitor(this, data);
            return null;
        }

        public override object TrackedVisitBlockStatement(BlockStatement blockStatement, object data)
        {
            return blockStatement.AcceptChildren(this, data);
        }

        public override object TrackedVisitBreakStatement(BreakStatement breakStatement, object data)
        {
            this.AppendIndentedLine("break");
            return null;
        }

        public override object TrackedVisitCaseLabel(CaseLabel caseLabel, object data)
        {
            return null;
        }

        public override object TrackedVisitCastExpression(CastExpression castExpression, object data)
        {
            return castExpression.Expression.AcceptVisitor(this, data);
        }

        public override object TrackedVisitCatchClause(CatchClause catchClause, object data)
        {
            Console.WriteLine("VisitCatchClause");
            return null;
        }

        public override object TrackedVisitCheckedExpression(CheckedExpression checkedExpression, object data)
        {
            Console.WriteLine("VisitCheckedExpression");
            return null;
        }

        public override object TrackedVisitCheckedStatement(CheckedStatement checkedStatement, object data)
        {
            Console.WriteLine("VisitCheckedStatement");
            return null;
        }

        public override object TrackedVisitClassReferenceExpression(ClassReferenceExpression classReferenceExpression, object data)
        {
            Console.WriteLine("VisitClassReferenceExpression");
            return null;
        }

        public override object TrackedVisitCollectionInitializerExpression(CollectionInitializerExpression collectionInitializerExpression, object data)
        {
            return null;
        }

        public override object TrackedVisitCompilationUnit(CompilationUnit compilationUnit, object data)
        {
            compilationUnit.AcceptChildren(this, data);
            return null;
        }

        public override object TrackedVisitConditionalExpression(ConditionalExpression conditionalExpression, object data)
        {
            conditionalExpression.TrueExpression.AcceptVisitor(this, data);
            this.Append(" if ");
            conditionalExpression.Condition.AcceptVisitor(this, data);
            this.Append(" else ");
            conditionalExpression.FalseExpression.AcceptVisitor(this, data);
            return null;
        }

        public override object TrackedVisitConstructorDeclaration(ConstructorDeclaration constructorDeclaration, object data)
        {
            this.CreateConstructor(this.constructorInfo);
            return null;
        }

        public override object TrackedVisitConstructorInitializer(ConstructorInitializer constructorInitializer, object data)
        {
            Console.WriteLine("VisitConstructorInitializer");
            return null;
        }

        public override object TrackedVisitContinueStatement(ContinueStatement continueStatement, object data)
        {
            this.AppendIndentedLine("continue");
            return null;
        }

        public override object TrackedVisitDeclareDeclaration(DeclareDeclaration declareDeclaration, object data)
        {
            Console.WriteLine("VisitDeclareDeclaration");
            return null;
        }

        public override object TrackedVisitDefaultValueExpression(DefaultValueExpression defaultValueExpression, object data)
        {
            Console.WriteLine("VisitDefaultValueExpression");
            return null;
        }

        public override object TrackedVisitDelegateDeclaration(DelegateDeclaration delegateDeclaration, object data)
        {
            Console.WriteLine("VisitDelegateDeclaration");
            return null;
        }

        public override object TrackedVisitDestructorDeclaration(DestructorDeclaration destructorDeclaration, object data)
        {
            this.AppendIndentedLine("def __del__(self):");
            this.IncreaseIndent();
            destructorDeclaration.Body.AcceptVisitor(this, data);
            this.DecreaseIndent();
            return null;
        }

        public override object TrackedVisitDirectionExpression(DirectionExpression directionExpression, object data)
        {
            Console.WriteLine("VisitDirectionExpression");
            return null;
        }

        public override object TrackedVisitDoLoopStatement(DoLoopStatement doLoopStatement, object data)
        {
            this.AppendIndented("while ");
            doLoopStatement.Condition.AcceptVisitor(this, data);
            this.Append(":");
            this.AppendLine();
            this.IncreaseIndent();
            doLoopStatement.EmbeddedStatement.AcceptVisitor(this, data);
            this.DecreaseIndent();
            return null;
        }

        public override object TrackedVisitElseIfSection(ElseIfSection elseIfSection, object data)
        {
            this.AppendIndented("elif ");
            elseIfSection.Condition.AcceptVisitor(this, data);
            this.Append(":");
            this.AppendLine();
            this.IncreaseIndent();
            elseIfSection.EmbeddedStatement.AcceptVisitor(this, data);
            this.DecreaseIndent();
            return null;
        }

        public override object TrackedVisitEmptyStatement(EmptyStatement emptyStatement, object data)
        {
            Console.WriteLine("VisitEmptyStatement");
            return null;
        }

        public override object TrackedVisitEndStatement(EndStatement endStatement, object data)
        {
            Console.WriteLine("VistEndStatement");
            return null;
        }

        public override object TrackedVisitEraseStatement(EraseStatement eraseStatement, object data)
        {
            Console.WriteLine("VisitEraseStatement");
            return null;
        }

        public override object TrackedVisitErrorStatement(ErrorStatement errorStatement, object data)
        {
            Console.WriteLine("VisitErrorStatement");
            return null;
        }

        public override object TrackedVisitEventAddRegion(EventAddRegion eventAddRegion, object data)
        {
            Console.WriteLine("VisitEventAddRegion");
            return null;
        }

        public override object TrackedVisitEventDeclaration(EventDeclaration eventDeclaration, object data)
        {
            Console.WriteLine("VisitEventDeclaration");
            return null;
        }

        public override object TrackedVisitEventRaiseRegion(EventRaiseRegion eventRaiseRegion, object data)
        {
            Console.WriteLine("VisitEventRaiseRegion");
            return null;
        }

        public override object TrackedVisitEventRemoveRegion(EventRemoveRegion eventRemoveRegion, object data)
        {
            Console.WriteLine("VisitEventRemoveRegion");
            return null;
        }

        public override object TrackedVisitExitStatement(ExitStatement exitStatement, object data)
        {
            Console.WriteLine("VisitExitStatement");
            return null;
        }

        public override object TrackedVisitExpressionRangeVariable(ExpressionRangeVariable expressionRangeVariable, object data)
        {
            return null;
        }

        public override object TrackedVisitExpressionStatement(ExpressionStatement expressionStatement, object data)
        {
            this.AppendIndented(string.Empty);
            expressionStatement.Expression.AcceptVisitor(this, data);
            this.AppendLine();
            return null;
        }

        public override object TrackedVisitExternAliasDirective(ExternAliasDirective externAliasDirective, object data)
        {
            Console.WriteLine("ExternAliasDirective");
            return null;
        }

        public override object TrackedVisitFieldDeclaration(FieldDeclaration fieldDeclaration, object data)
        {
            return null;
        }

        public override object TrackedVisitFixedStatement(FixedStatement fixedStatement, object data)
        {
            Console.WriteLine("VisitFixedStatement");
            return null;
        }

        public override object TrackedVisitForeachStatement(ForeachStatement foreachStatement, object data)
        {
            this.AppendIndented(string.Empty);
            this.CreateInitStatement(foreachStatement);
            this.AppendLine();
            this.AppendIndentedLine("while enumerator.MoveNext():");
            this.IncreaseIndent();
            this.AppendIndentedLine(string.Concat(foreachStatement.VariableName, " = enumerator.Current"));
            foreachStatement.EmbeddedStatement.AcceptVisitor(this, data);
            this.DecreaseIndent();
            return null;
        }

        public override object TrackedVisitForNextStatement(ForNextStatement forNextStatement, object data)
        {
            Console.WriteLine("VisitForNextStatement");
            return null;
        }

        public override object TrackedVisitForStatement(ForStatement forStatement, object data)
        {
            foreach (Statement initializer in forStatement.Initializers)
            {
                initializer.AcceptVisitor(this, data);
            }
            this.AppendIndented("while ");
            forStatement.Condition.AcceptVisitor(this, data);
            this.Append(":");
            this.AppendLine();
            this.IncreaseIndent();
            forStatement.EmbeddedStatement.AcceptVisitor(this, data);
            foreach (Statement iterator in forStatement.Iterator)
            {
                iterator.AcceptVisitor(this, data);
            }
            this.DecreaseIndent();
            return null;
        }

        public override object TrackedVisitGotoCaseStatement(GotoCaseStatement gotoCaseStatement, object data)
        {
            Console.WriteLine("VisitGotoCaseStatement");
            return null;
        }

        public override object TrackedVisitGotoStatement(GotoStatement gotoStatement, object data)
        {
            Console.WriteLine("VisitGotoStatement");
            return null;
        }

        public override object TrackedVisitIdentifierExpression(IdentifierExpression identifierExpression, object data)
        {
            string identifier = identifierExpression.Identifier;
            if (this.IsField(identifier))
            {
                this.Append(string.Concat("self._", identifier));
            }
            else if ((!IsProperty(identifier) || this.IsMethodParameter(identifier)))
            {
                this.Append(identifier);
            }
            else
            {
                this.Append(string.Concat("self.", identifier));
            }
            return null;
        }

        public override object TrackedVisitIfElseStatement(IfElseStatement ifElseStatement, object data)
        {
            this.AppendIndented("if ");
            ifElseStatement.Condition.AcceptVisitor(this, data);
            this.Append(":");
            this.AppendLine();
            this.IncreaseIndent();
            foreach (Statement trueStatement in ifElseStatement.TrueStatement)
            {
                trueStatement.AcceptVisitor(this, data);
            }
            this.DecreaseIndent();
            if (ifElseStatement.HasElseIfSections)
            {
                foreach (ElseIfSection elseIfSection in ifElseStatement.ElseIfSections)
                {
                    elseIfSection.AcceptVisitor(this, data);
                }
            }
            if (ifElseStatement.HasElseStatements)
            {
                this.AppendIndentedLine("else:");
                this.IncreaseIndent();
                foreach (Statement falseStatement in ifElseStatement.FalseStatement)
                {
                    falseStatement.AcceptVisitor(this, data);
                }
                this.DecreaseIndent();
            }
            return null;
        }

        public override object TrackedVisitIndexerDeclaration(IndexerDeclaration indexerDeclaration, object data)
        {
            Console.WriteLine("VisitIndexerDeclaration");
            return null;
        }

        public override object TrackedVisitIndexerExpression(IndexerExpression indexerExpression, object data)
        {
            indexerExpression.TargetObject.AcceptVisitor(this, data);
            foreach (Expression index in indexerExpression.Indexes)
            {
                this.Append("[");
                index.AcceptVisitor(this, data);
                this.Append("]");
            }
            return null;
        }

        public override object TrackedVisitInnerClassTypeReference(InnerClassTypeReference innerClassTypeReference, object data)
        {
            Console.WriteLine("VisitInnerClassTypeReference");
            return null;
        }

        public override object TrackedVisitInterfaceImplementation(InterfaceImplementation interfaceImplementation, object data)
        {
            Console.WriteLine("VisitInterfaceImplementation");
            return null;
        }

        public override object TrackedVisitInvocationExpression(InvocationExpression invocationExpression, object data)
        {
            MemberReferenceExpression targetObject = invocationExpression.TargetObject as MemberReferenceExpression;
            IdentifierExpression identifierExpression = invocationExpression.TargetObject as IdentifierExpression;
            if (targetObject != null)
            {
                targetObject.TargetObject.AcceptVisitor(this, data);
                this.Append(string.Concat(".", targetObject.MemberName));
            }
            else if (identifierExpression != null)
            {
                if ((this.currentMethod == null ? true : !this.IsStatic(this.currentMethod)))
                {
                    this.Append("self.");
                }
                else
                {
                    this.Append(string.Concat(NRefactoryToPythonConverter.GetTypeName(this.currentMethod), "."));
                }
                this.Append(identifierExpression.Identifier);
            }
            this.Append("(");
            bool flag = true;
            foreach (Expression argument in invocationExpression.Arguments)
            {
                if (!flag)
                {
                    this.Append(", ");
                }
                else
                {
                    flag = false;
                }
                argument.AcceptVisitor(this, data);
            }
            this.Append(")");
            return null;
        }

        public override object TrackedVisitLabelStatement(LabelStatement labelStatement, object data)
        {
            Console.WriteLine("VisitLabelStatement");
            return null;
        }

        public override object TrackedVisitLambdaExpression(LambdaExpression lambdaExpression, object data)
        {
            return null;
        }

        public override object TrackedVisitLocalVariableDeclaration(LocalVariableDeclaration localVariableDeclaration, object data)
        {
            VariableDeclaration item = localVariableDeclaration.Variables[0];
            if (!item.Initializer.IsNull)
            {
                this.AppendIndented(string.Concat(item.Name, " = "));
                item.Initializer.AcceptVisitor(this, data);
                this.AppendLine();
            }
            return null;
        }

        public override object TrackedVisitLockStatement(LockStatement lockStatement, object data)
        {
            Console.WriteLine("VisitLockStatement");
            return null;
        }

        public override object TrackedVisitMemberReferenceExpression(MemberReferenceExpression memberReferenceExpression, object data)
        {
            memberReferenceExpression.TargetObject.AcceptVisitor(this, data);
            if ((!(memberReferenceExpression.TargetObject is ThisReferenceExpression) ? true : this.IsProperty(memberReferenceExpression.MemberName)))
            {
                this.Append(".");
            }
            else
            {
                this.Append("._");
            }
            this.Append(memberReferenceExpression.MemberName);
            return null;
        }

        public override object TrackedVisitMethodDeclaration(MethodDeclaration methodDeclaration, object data)
        {
            this.currentMethod = methodDeclaration;
            this.AppendIndented(string.Concat("def ", methodDeclaration.Name));
            this.AddParameters(methodDeclaration);
            this.methodParameters = methodDeclaration.Parameters;
            this.AppendLine();
            this.IncreaseIndent();
            this.AppendDocstring(this.xmlDocComments);
            if (methodDeclaration.Body.Children.Count <= 0)
            {
                this.AppendIndentedPassStatement();
            }
            else
            {
                methodDeclaration.Body.AcceptVisitor(this, data);
            }
            this.DecreaseIndent();
            this.AppendLine();
            if (this.IsStatic(methodDeclaration))
            {
                this.AppendIndentedLine(string.Concat(methodDeclaration.Name, " = staticmethod(", methodDeclaration.Name, ")"));
                this.AppendLine();
                this.SaveMethodIfMainEntryPoint(methodDeclaration);
            }
            this.currentMethod = null;
            return null;
        }

        public override object TrackedVisitNamedArgumentExpression(NamedArgumentExpression namedArgumentExpression, object data)
        {
            this.Append(namedArgumentExpression.Name);
            this.Append(" = ");
            namedArgumentExpression.Expression.AcceptVisitor(this, data);
            return null;
        }

        public override object TrackedVisitNamespaceDeclaration(NamespaceDeclaration namespaceDeclaration, object data)
        {
            return namespaceDeclaration.AcceptChildren(this, data);
        }

        public override object TrackedVisitObjectCreateExpression(ObjectCreateExpression objectCreateExpression, object data)
        {
            this.Append(objectCreateExpression.CreateType.Type);
            if (NRefactoryToPythonConverter.IsGenericType(objectCreateExpression))
            {
                this.AppendGenericTypes(objectCreateExpression);
            }
            this.Append("(");
            bool flag = true;
            foreach (Expression parameter in objectCreateExpression.Parameters)
            {
                if (!flag)
                {
                    this.Append(", ");
                }
                parameter.AcceptVisitor(this, data);
                flag = false;
            }
            bool flag1 = true;
            foreach (Expression createExpression in objectCreateExpression.ObjectInitializer.CreateExpressions)
            {
                if (!flag1)
                {
                    this.Append(", ");
                }
                createExpression.AcceptVisitor(this, data);
                flag1 = false;
            }
            this.Append(")");
            return null;
        }

        public override object TrackedVisitOnErrorStatement(OnErrorStatement onErrorStatement, object data)
        {
            return null;
        }

        public override object TrackedVisitOperatorDeclaration(OperatorDeclaration operatorDeclaration, object data)
        {
            Console.WriteLine("VisitOperatorDeclaration");
            return null;
        }

        public override object TrackedVisitOptionDeclaration(OptionDeclaration optionDeclaration, object data)
        {
            Console.WriteLine("VisitOptionDeclaration");
            return null;
        }

        public override object TrackedVisitParameterDeclarationExpression(ParameterDeclarationExpression parameterDeclarationExpression, object data)
        {
            Console.WriteLine("VisitParameterDeclarationExpression");
            return null;
        }

        public override object TrackedVisitParenthesizedExpression(ParenthesizedExpression parenthesizedExpression, object data)
        {
            this.Append("(");
            parenthesizedExpression.Expression.AcceptVisitor(this, data);
            this.Append(")");
            return null;
        }

        public override object TrackedVisitPointerReferenceExpression(PointerReferenceExpression pointerReferenceExpression, object data)
        {
            Console.WriteLine("VisitPointerReferenceExpression");
            return null;
        }

        public override object TrackedVisitPrimitiveExpression(PrimitiveExpression primitiveExpression, object data)
        {
            if (primitiveExpression.Value == null)
            {
                this.Append("None");
            }
            else if (!(primitiveExpression.Value is bool))
            {
                this.Append(primitiveExpression.StringValue);
            }
            else
            {
                this.Append(primitiveExpression.Value.ToString());
            }
            return null;
        }

        public override object TrackedVisitPropertyDeclaration(PropertyDeclaration propertyDeclaration, object data)
        {
            string name = propertyDeclaration.Name;
            this.propertyNames.Add(name);
            if (propertyDeclaration.HasGetRegion)
            {
                this.AppendIndentedLine(string.Concat("def get_", name, "(self):"));
                this.IncreaseIndent();
                propertyDeclaration.GetRegion.Block.AcceptVisitor(this, data);
                this.DecreaseIndent();
                this.AppendLine();
            }
            if (propertyDeclaration.HasSetRegion)
            {
                this.AppendIndentedLine(string.Concat("def set_", name, "(self, value):"));
                this.IncreaseIndent();
                propertyDeclaration.SetRegion.Block.AcceptVisitor(this, data);
                this.DecreaseIndent();
                this.AppendLine();
            }
            this.AppendPropertyDecorator(propertyDeclaration);
            this.AppendLine();
            return null;
        }

        public override object TrackedVisitPropertyGetRegion(PropertyGetRegion propertyGetRegion, object data)
        {
            Console.WriteLine("VisitPropertyGetRegion");
            return null;
        }

        public override object TrackedVisitPropertySetRegion(PropertySetRegion propertySetRegion, object data)
        {
            Console.WriteLine("VisitPropertySetRegion");
            return null;
        }

        public override object TrackedVisitQueryExpression(QueryExpression queryExpression, object data)
        {
            return null;
        }

        public override object TrackedVisitQueryExpressionAggregateClause(QueryExpressionAggregateClause queryExpressionAggregateClause, object data)
        {
            return null;
        }

        public override object TrackedVisitQueryExpressionDistinctClause(QueryExpressionDistinctClause queryExpressionDistinctClause, object data)
        {
            return null;
        }

        public override object TrackedVisitQueryExpressionFromClause(QueryExpressionFromClause queryExpressionFromClause, object data)
        {
            return null;
        }

        public override object TrackedVisitQueryExpressionGroupClause(QueryExpressionGroupClause queryExpressionGroupClause, object data)
        {
            return null;
        }

        public override object TrackedVisitQueryExpressionGroupJoinVBClause(QueryExpressionGroupJoinVBClause queryExpressionGroupJoinVBClause, object data)
        {
            return null;
        }

        public override object TrackedVisitQueryExpressionGroupVBClause(QueryExpressionGroupVBClause queryExpressionGroupVBClause, object data)
        {
            return null;
        }

        public override object TrackedVisitQueryExpressionJoinClause(QueryExpressionJoinClause queryExpressionJoinClause, object data)
        {
            return null;
        }

        public override object TrackedVisitQueryExpressionJoinConditionVB(QueryExpressionJoinConditionVB queryExpressionJoinConditionVB, object data)
        {
            return null;
        }

        public override object TrackedVisitQueryExpressionJoinVBClause(QueryExpressionJoinVBClause queryExpressionJoinVBClause, object data)
        {
            return null;
        }

        public override object TrackedVisitQueryExpressionLetClause(QueryExpressionLetClause queryExpressionLetClause, object data)
        {
            return null;
        }

        public override object TrackedVisitQueryExpressionLetVBClause(QueryExpressionLetVBClause queryExpressionLetVBClause, object data)
        {
            return null;
        }

        public override object TrackedVisitQueryExpressionOrderClause(QueryExpressionOrderClause queryExpressionOrderClause, object data)
        {
            return null;
        }

        public override object TrackedVisitQueryExpressionOrdering(QueryExpressionOrdering queryExpressionOrdering, object data)
        {
            return null;
        }

        public override object TrackedVisitQueryExpressionPartitionVBClause(QueryExpressionPartitionVBClause queryExpressionPartitionVBClause, object data)
        {
            return null;
        }

        public override object TrackedVisitQueryExpressionSelectClause(QueryExpressionSelectClause queryExpressionSelectClause, object data)
        {
            return null;
        }

        public override object TrackedVisitQueryExpressionSelectVBClause(QueryExpressionSelectVBClause queryExpressionSelectVBClause, object data)
        {
            return null;
        }

        public override object TrackedVisitQueryExpressionWhereClause(QueryExpressionWhereClause queryExpressionWhereClause, object data)
        {
            return null;
        }

        public override object TrackedVisitRaiseEventStatement(RaiseEventStatement raiseEventStatement, object data)
        {
            Console.WriteLine("VisitRaiseEventStatement");
            return null;
        }

        public override object TrackedVisitReDimStatement(ReDimStatement reDimStatement, object data)
        {
            Console.WriteLine("VisitReDimStatement");
            return null;
        }

        public override object TrackedVisitRemoveHandlerStatement(RemoveHandlerStatement removeHandlerStatement, object data)
        {
            Console.WriteLine("VisitRemoveHandlerStatement");
            return null;
        }

        public override object TrackedVisitResumeStatement(ResumeStatement resumeStatement, object data)
        {
            Console.WriteLine("VisitResumeStatement");
            return null;
        }

        public override object TrackedVisitReturnStatement(ReturnStatement returnStatement, object data)
        {
            this.AppendIndented("return ");
            returnStatement.Expression.AcceptVisitor(this, data);
            this.AppendLine();
            return null;
        }

        public override object TrackedVisitSizeOfExpression(SizeOfExpression sizeOfExpression, object data)
        {
            Console.WriteLine("VisitSizeOfExpression");
            return null;
        }

        public override object TrackedVisitStackAllocExpression(StackAllocExpression stackAllocExpression, object data)
        {
            return null;
        }

        public override object TrackedVisitStopStatement(StopStatement stopStatement, object data)
        {
            return null;
        }

        public override object TrackedVisitSwitchSection(SwitchSection switchSection, object data)
        {
            return null;
        }

        public override object TrackedVisitSwitchStatement(SwitchStatement switchStatement, object data)
        {
            bool flag = true;
            foreach (SwitchSection switchSection in switchStatement.SwitchSections)
            {
                this.CreateSwitchCaseCondition(switchStatement.SwitchExpression, switchSection, flag);
                this.IncreaseIndent();
                this.CreateSwitchCaseBody(switchSection);
                this.DecreaseIndent();
                flag = false;
            }
            return null;
        }

        public override object TrackedVisitTemplateDefinition(TemplateDefinition templateDefinition, object data)
        {
            return null;
        }

        public override object TrackedVisitThisReferenceExpression(ThisReferenceExpression thisReferenceExpression, object data)
        {
            this.Append("self");
            return null;
        }

        public override object TrackedVisitThrowStatement(ThrowStatement throwStatement, object data)
        {
            this.AppendIndented("raise ");
            throwStatement.Expression.AcceptVisitor(this, data);
            this.AppendLine();
            return null;
        }

        public override object TrackedVisitTryCatchStatement(TryCatchStatement tryCatchStatement, object data)
        {
            this.AppendIndentedLine("try:");
            this.IncreaseIndent();
            tryCatchStatement.StatementBlock.AcceptVisitor(this, data);
            this.DecreaseIndent();
            foreach (CatchClause catchClause in tryCatchStatement.CatchClauses)
            {
                this.AppendIndented("except ");
                this.Append(catchClause.TypeReference.Type);
                this.Append(string.Concat(", ", catchClause.VariableName, ":"));
                this.AppendLine();
                this.IncreaseIndent();
                catchClause.StatementBlock.AcceptVisitor(this, data);
                this.DecreaseIndent();
            }
            this.AppendIndentedLine("finally:");
            this.IncreaseIndent();
            tryCatchStatement.FinallyBlock.AcceptVisitor(this, data);
            this.DecreaseIndent();
            return null;
        }

        public override object TrackedVisitTypeDeclaration(TypeDeclaration typeDeclaration, object data)
        {
            this.codeBuilder.AppendLineIfPreviousLineIsCode();
            this.AppendIndented(string.Concat("class ", typeDeclaration.Name));
            this.AppendBaseTypes(typeDeclaration.BaseTypes);
            this.AppendLine();
            this.IncreaseIndent();
            this.AppendDocstring(this.xmlDocComments);
            if (typeDeclaration.Children.Count <= 0)
            {
                this.AppendIndentedPassStatement();
            }
            else
            {
                this.constructorInfo = PythonConstructorInfo.GetConstructorInfo(typeDeclaration);
                if (this.constructorInfo != null)
                {
                    if (this.constructorInfo.Constructor == null)
                    {
                        this.CreateConstructor(this.constructorInfo);
                    }
                }
                typeDeclaration.AcceptChildren(this, data);
            }
            this.DecreaseIndent();
            return null;
        }

        public override object TrackedVisitTypeOfExpression(TypeOfExpression typeOfExpression, object data)
        {
            this.codeBuilder.InsertIndentedLine("import clr\r\n");
            this.Append("clr.GetClrType(");
            this.Append(this.GetTypeName(typeOfExpression.TypeReference));
            this.Append(")");
            return null;
        }

        public override object TrackedVisitTypeOfIsExpression(TypeOfIsExpression typeOfIsExpression, object data)
        {
            Console.WriteLine("VisitTypeOfIsExpression");
            return null;
        }

        public override object TrackedVisitTypeReference(TypeReference typeReference, object data)
        {
            Console.WriteLine("VisitTypeReference");
            return null;
        }

        public override object TrackedVisitTypeReferenceExpression(TypeReferenceExpression typeReferenceExpression, object data)
        {
            this.Append(this.GetTypeName(typeReferenceExpression.TypeReference));
            return null;
        }

        public override object TrackedVisitUnaryOperatorExpression(UnaryOperatorExpression unaryOperatorExpression, object data)
        {
            object obj;
            switch (unaryOperatorExpression.Op)
            {
                case UnaryOperatorType.Not:
                    {
                        obj = this.CreateUnaryOperatorStatement("not ", unaryOperatorExpression.Expression);
                        break;
                    }
                case UnaryOperatorType.BitNot:
                    {
                        obj = this.CreateUnaryOperatorStatement("~", unaryOperatorExpression.Expression);
                        break;
                    }
                case UnaryOperatorType.Minus:
                    {
                        obj = this.CreateUnaryOperatorStatement(NRefactoryToPythonConverter.GetBinaryOperator(BinaryOperatorType.Subtract), unaryOperatorExpression.Expression);
                        break;
                    }
                case UnaryOperatorType.Plus:
                    {
                        obj = this.CreateUnaryOperatorStatement(NRefactoryToPythonConverter.GetBinaryOperator(BinaryOperatorType.Add), unaryOperatorExpression.Expression);
                        break;
                    }
                case UnaryOperatorType.Increment:
                case UnaryOperatorType.PostIncrement:
                    {
                        obj = this.CreateIncrementStatement(unaryOperatorExpression);
                        break;
                    }
                case UnaryOperatorType.Decrement:
                case UnaryOperatorType.PostDecrement:
                    {
                        obj = this.CreateDecrementStatement(unaryOperatorExpression);
                        break;
                    }
                default:
                    {
                        obj = null;
                        break;
                    }
            }
            return obj;
        }

        public override object TrackedVisitUncheckedExpression(UncheckedExpression uncheckedExpression, object data)
        {
            return null;
        }

        public override object TrackedVisitUncheckedStatement(UncheckedStatement uncheckedStatement, object data)
        {
            return null;
        }

        public override object TrackedVisitUnsafeStatement(UnsafeStatement unsafeStatement, object data)
        {
            return null;
        }

        public override object TrackedVisitUsing(Using @using, object data)
        {
            return null;
        }

        public override object TrackedVisitUsingDeclaration(UsingDeclaration usingDeclaration, object data)
        {
            foreach (Using @using in usingDeclaration.Usings)
            {
                this.AppendIndentedLine(string.Concat("from ", @using.Name, " import *"));
            }
            return null;
        }

        public override object TrackedVisitUsingStatement(UsingStatement usingStatement, object data)
        {
            return null;
        }

        public override object TrackedVisitVariableDeclaration(VariableDeclaration variableDeclaration, object data)
        {
            this.AppendIndented(string.Concat(variableDeclaration.Name, " = "));
            variableDeclaration.Initializer.AcceptVisitor(this, data);
            this.AppendLine();
            return null;
        }

        public override object TrackedVisitWithStatement(WithStatement withStatement, object data)
        {
            return null;
        }

        public override object TrackedVisitYieldStatement(YieldStatement yieldStatement, object data)
        {
            return null;
        }
    }
}