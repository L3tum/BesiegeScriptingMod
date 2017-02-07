#region usings

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.Ast;
using ICSharpCode.NRefactory.PrettyPrinter;
using ICSharpCode.NRefactory.Visitors;

#endregion

namespace BesiegeScriptingMod.Util
{
    public class NRefactoryToPythonConverter : NodeTrackingAstVisitor, IOutputFormatter
    {
        private static readonly string Docstring;

        private PythonCodeBuilder codeBuilder;

        private PythonConstructorInfo constructorInfo;

        private MethodDeclaration currentMethod;

        private INode currentNode;

        private List<MethodDeclaration> entryPointMethods;

        private List<ParameterDeclarationExpression> methodParameters = new List<ParameterDeclarationExpression>();

        private readonly List<string> propertyNames = new List<string>();

        private SpecialNodesInserter specialNodesInserter;

        private readonly List<Comment> xmlDocComments = new List<Comment>();

        static NRefactoryToPythonConverter()
        {
            Docstring = "\"\"\"";
        }

        public NRefactoryToPythonConverter(SupportedLanguage language)
        {
            SupportedLanguage = language;
        }

        public NRefactoryToPythonConverter()
        {
        }

        public ReadOnlyCollection<MethodDeclaration> EntryPointMethods
        {
            get { return entryPointMethods.AsReadOnly(); }
        }

        public string IndentString { get; set; } = "\t";

        public SupportedLanguage SupportedLanguage { get; }

        int IOutputFormatter.IndentationLevel
        {
            get { return codeBuilder.Indent; }
            set { }
        }

        bool IOutputFormatter.IsInMemberBody
        {
            get { return false; }
            set { }
        }

        string IOutputFormatter.Text
        {
            get { return string.Empty; }
        }

        void IOutputFormatter.Indent()
        {
        }

        void IOutputFormatter.NewLine()
        {
        }

        void IOutputFormatter.PrintBlankLine(bool forceWriteInPreviousBlock)
        {
        }

        void IOutputFormatter.PrintComment(Comment comment, bool forceWriteInPreviousBlock)
        {
            if (comment.CommentType == CommentType.SingleLine)
            {
                AppendSingleLineComment(comment);
            }
            else if (comment.CommentType == CommentType.Block)
            {
                AppendMultilineComment(comment);
            }
            else if (comment.CommentType == CommentType.Documentation)
            {
                if (!SupportsDocstring(currentNode))
                {
                    AppendSingleLineComment(comment);
                }
                else
                {
                    xmlDocComments.Add(comment);
                }
            }
        }

        void IOutputFormatter.PrintPreprocessingDirective(PreprocessingDirective directive, bool forceWriteInPreviousBlock)
        {
        }

        private void AddParameters(ParametrizedNode method)
        {
            Append("(");
            List<ParameterDeclarationExpression> parameters = method.Parameters;
            if (parameters.Count > 0)
            {
                if (!IsStatic(method))
                {
                    Append("self, ");
                }
                for (int i = 0; i < parameters.Count; i++)
                {
                    if (i > 0)
                    {
                        Append(", ");
                    }
                    Append(parameters[i].ParameterName);
                }
            }
            else if (!IsStatic(method))
            {
                Append("self");
            }
            Append("):");
        }

        private void Append(string code)
        {
            codeBuilder.Append(code);
        }

        private void AppendBaseTypes(List<TypeReference> baseTypes)
        {
            Append("(");
            if (baseTypes.Count != 0)
            {
                for (int i = 0; i < baseTypes.Count; i++)
                {
                    TypeReference item = baseTypes[i];
                    if (i > 0)
                    {
                        Append(", ");
                    }
                    Append(GetTypeName(item));
                }
            }
            else
            {
                Append("object");
            }
            Append("):");
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
                        AppendIndented(string.Empty);
                    }
                    else
                    {
                        AppendIndented(Docstring);
                    }
                    Append(commentText);
                    AppendLine();
                }
                AppendIndentedLine(Docstring);
            }
            else if (xmlDocComments.Count == 1)
            {
                AppendIndentedLine(string.Concat(Docstring, xmlDocComments[0].CommentText, Docstring));
            }
        }

        private void AppendForeachVariableName(ForeachStatement foreachStatement)
        {
            IdentifierExpression expression = foreachStatement.Expression as IdentifierExpression;
            InvocationExpression invocationExpression = foreachStatement.Expression as InvocationExpression;
            MemberReferenceExpression memberReferenceExpression = foreachStatement.Expression as MemberReferenceExpression;
            if (expression != null)
            {
                Append(expression.Identifier);
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
            Append("[");
            List<TypeReference> genericTypes = expression.CreateType.GenericTypes;
            for (int i = 0; i < genericTypes.Count; i++)
            {
                if (i != 0)
                {
                    Append(", ");
                }
                TypeReference item = genericTypes[i];
                if (!item.IsArrayType)
                {
                    Append(GetTypeName(item));
                }
                else
                {
                    Append(string.Concat("Array[", GetTypeName(item), "]"));
                }
            }
            Append("]");
        }

        private void AppendIndented(string code)
        {
            codeBuilder.AppendIndented(code);
        }

        private void AppendIndentedLine(string code)
        {
            codeBuilder.AppendIndentedLine(code);
        }

        private void AppendIndentedPassStatement()
        {
            AppendIndentedLine("pass");
        }

        private void AppendLine()
        {
            codeBuilder.AppendLine();
        }

        private void AppendMultilineComment(Comment comment)
        {
            string[] strArrays = comment.CommentText.Split('\n');
            for (int i = 0; i < strArrays.Length; i++)
            {
                string str = string.Concat("# ", strArrays[i].Trim());
                if (i != 0 ? true : comment.CommentStartsLine)
                {
                    AppendIndentedLine(str);
                }
                else
                {
                    codeBuilder.AppendToPreviousLine(string.Concat(" ", str));
                }
            }
        }

        private void AppendPropertyDecorator(PropertyDeclaration propertyDeclaration)
        {
            string name = propertyDeclaration.Name;
            AppendIndented(name);
            Append(" = property(");
            bool flag = false;
            if (propertyDeclaration.HasGetRegion)
            {
                Append(string.Concat("fget=get_", name));
                flag = true;
            }
            if (propertyDeclaration.HasSetRegion)
            {
                if (flag)
                {
                    Append(", ");
                }
                Append(string.Concat("fset=set_", name));
            }
            Append(")");
            AppendLine();
        }

        private void AppendSingleLineComment(Comment comment)
        {
            if (!comment.CommentStartsLine)
            {
                codeBuilder.AppendToPreviousLine(string.Concat(" #", comment.CommentText));
            }
            else
            {
                codeBuilder.AppendIndentedLine(string.Concat("#", comment.CommentText));
            }
        }

        protected override void BeginVisit(INode node)
        {
            xmlDocComments.Clear();
            currentNode = node;
            specialNodesInserter.AcceptNodeStart(node);
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
                flag = extension == ".cs" ? true : extension == ".vb";
            }
            return flag;
        }

        public string Convert(string source)
        {
            return Convert(source, SupportedLanguage);
        }

        public string Convert(string source, SupportedLanguage language)
        {
            CompilationUnit compilationUnit = GenerateCompilationUnit(source, language);
            SpecialOutputVisitor specialOutputVisitor = new SpecialOutputVisitor(this);
            specialNodesInserter = new SpecialNodesInserter(compilationUnit.UserData as List<ISpecial>, specialOutputVisitor);
            entryPointMethods = new List<MethodDeclaration>();
            codeBuilder = new PythonCodeBuilder
            {
                IndentString = IndentString
            };
            compilationUnit.AcceptVisitor(this, null);
            return codeBuilder.ToString().Trim();
        }

        public static NRefactoryToPythonConverter Create(string fileName)
        {
            NRefactoryToPythonConverter nRefactoryToPythonConverter;
            if (!CanConvert(fileName))
            {
                nRefactoryToPythonConverter = null;
            }
            else
            {
                nRefactoryToPythonConverter = new NRefactoryToPythonConverter(GetSupportedLanguage(fileName));
            }
            return nRefactoryToPythonConverter;
        }

        private void CreateConstructor(PythonConstructorInfo constructorInfo)
        {
            if (constructorInfo.Constructor == null)
            {
                AppendIndented("def __init__(self):");
            }
            else
            {
                AppendIndented("def __init__");
                AddParameters(constructorInfo.Constructor);
                methodParameters = constructorInfo.Constructor.Parameters;
            }
            AppendLine();
            IncreaseIndent();
            AppendDocstring(xmlDocComments);
            if (constructorInfo.Fields.Count > 0)
            {
                foreach (FieldDeclaration field in constructorInfo.Fields)
                {
                    if (FieldHasInitialValue(field))
                    {
                        CreateFieldInitialization(field);
                    }
                }
            }
            if (!IsEmptyConstructor(constructorInfo.Constructor))
            {
                constructorInfo.Constructor.Body.AcceptVisitor(this, null);
                AppendLine();
            }
            else if (constructorInfo.Fields.Count != 0)
            {
                AppendLine();
            }
            else
            {
                AppendIndentedPassStatement();
            }
            DecreaseIndent();
        }

        private object CreateDecrementStatement(UnaryOperatorExpression unaryOperatorExpression)
        {
            object obj = CreateIncrementStatement(unaryOperatorExpression, 1, GetBinaryOperator(BinaryOperatorType.Subtract));
            return obj;
        }

        private object CreateDelegateCreateExpression(Expression eventHandlerExpression)
        {
            IdentifierExpression identifierExpression = eventHandlerExpression as IdentifierExpression;
            ObjectCreateExpression objectCreateExpression = eventHandlerExpression as ObjectCreateExpression;
            MemberReferenceExpression memberReferenceExpression = eventHandlerExpression as MemberReferenceExpression;
            if (identifierExpression != null)
            {
                Append(string.Concat("self.", identifierExpression.Identifier));
            }
            else if (memberReferenceExpression != null)
            {
                memberReferenceExpression.AcceptVisitor(this, null);
            }
            else if (objectCreateExpression != null)
            {
                CreateDelegateCreateExpression(objectCreateExpression.Parameters[0]);
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
            VisitVariableDeclaration(item, null);
            item.Name = name;
        }

        private object CreateHandlerStatement(Expression eventExpression, string addRemoveOperator, Expression eventHandlerExpression)
        {
            CreateEventReferenceExpression(eventExpression);
            Append(string.Concat(" ", addRemoveOperator, " "));
            CreateDelegateCreateExpression(eventHandlerExpression);
            return null;
        }

        private object CreateIncrementStatement(UnaryOperatorExpression unaryOperatorExpression)
        {
            object obj = CreateIncrementStatement(unaryOperatorExpression, 1, GetBinaryOperator(BinaryOperatorType.Add));
            return obj;
        }

        private object CreateIncrementStatement(UnaryOperatorExpression unaryOperatorExpression, int increment, string binaryOperator)
        {
            unaryOperatorExpression.Expression.AcceptVisitor(this, null);
            Append(string.Concat(" ", binaryOperator, "= "));
            Append(increment.ToString());
            return null;
        }

        private object CreateInitStatement(ForeachStatement foreachStatement)
        {
            Append("enumerator = ");
            AppendForeachVariableName(foreachStatement);
            Append(".GetEnumerator()");
            return null;
        }

        private object CreateSimpleAssignment(AssignmentExpression assignmentExpression, string op, object data)
        {
            assignmentExpression.Left.AcceptVisitor(this, data);
            Append(string.Concat(" ", op, " "));
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
                AppendIndentedLine("pass");
            }
        }

        private void CreateSwitchCaseCondition(Expression switchExpression, SwitchSection section, bool firstSection)
        {
            bool flag = true;
            foreach (CaseLabel switchLabel in section.SwitchLabels)
            {
                if (!flag)
                {
                    CreateSwitchCaseCondition(" or ", switchExpression, switchLabel);
                }
                else if (switchLabel.IsDefault)
                {
                    AppendIndented("else");
                }
                else if (!firstSection)
                {
                    AppendIndented(string.Empty);
                    CreateSwitchCaseCondition("elif ", switchExpression, switchLabel);
                }
                else
                {
                    AppendIndented(string.Empty);
                    CreateSwitchCaseCondition("if ", switchExpression, switchLabel);
                }
                flag = false;
            }
            Append(":");
            AppendLine();
        }

        private void CreateSwitchCaseCondition(string prefix, Expression switchExpression, CaseLabel label)
        {
            Append(prefix);
            switchExpression.AcceptVisitor(this, null);
            Append(" == ");
            label.Label.AcceptVisitor(this, null);
        }

        private object CreateUnaryOperatorStatement(string op, Expression expression)
        {
            Append(op);
            expression.AcceptVisitor(this, null);
            return null;
        }

        private void DecreaseIndent()
        {
            codeBuilder.DecreaseIndent();
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
            stringBuilder.Append(GetTypeName(methodDeclaration));
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
            supportedLanguage = !(Path.GetExtension(fileName.ToLowerInvariant()) == ".vb") ? SupportedLanguage.CSharp : SupportedLanguage.VBNet;
            return supportedLanguage;
        }

        private string GetTypeName(TypeReference typeRef)
        {
            string str;
            string type = typeRef.Type;
            if (type == typeof (string).FullName)
            {
                str = "str";
            }
            else if (type == typeof (int).FullName ? false : !(type == typeof (int).Name))
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

        private void IncreaseIndent()
        {
            codeBuilder.IncreaseIndent();
        }

        private static bool IsAddEventHandler(AssignmentExpression assignmentExpression)
        {
            return assignmentExpression.Op != AssignmentOperatorType.Add ? false : assignmentExpression.Left is MemberReferenceExpression;
        }

        private static bool IsEmptyConstructor(ConstructorDeclaration constructor)
        {
            bool flag;
            flag = constructor == null ? true : constructor.Body.Children.Count == 0;
            return flag;
        }

        private bool IsField(string name)
        {
            bool flag;
            if (!IsMethodParameter(name))
            {
                if (constructorInfo != null)
                {
                    foreach (FieldDeclaration field in constructorInfo.Fields)
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
            foreach (ParameterDeclarationExpression methodParameter in methodParameters)
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
            return propertyNames.Contains(name);
        }

        private static bool IsRemoveEventHandler(AssignmentExpression assignmentExpression)
        {
            return assignmentExpression.Op != AssignmentOperatorType.Subtract ? false : assignmentExpression.Left is MemberReferenceExpression;
        }

        private bool IsStatic(ParametrizedNode method)
        {
            return (method.Modifier & Modifiers.Static) == Modifiers.Static;
        }

        private void SaveMethodIfMainEntryPoint(MethodDeclaration method)
        {
            if (method.Name == "Main")
            {
                entryPointMethods.Add(method);
            }
        }

        private bool SupportsDocstring(INode node)
        {
            return node is TypeDeclaration || node is MethodDeclaration ? true : node is ConstructorDeclaration;
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
            string typeName = GetTypeName(arrayCreateExpression.CreateType);
            if (arrayCreateExpression.ArrayInitializer.CreateExpressions.Count != 0)
            {
                Append(string.Concat("Array[", typeName, "]"));
                Append("((");
                bool flag = true;
                foreach (Expression createExpression in arrayCreateExpression.ArrayInitializer.CreateExpressions)
                {
                    if (!flag)
                    {
                        Append(", ");
                    }
                    else
                    {
                        flag = false;
                    }
                    createExpression.AcceptVisitor(this, data);
                }
                Append("))");
            }
            else
            {
                Append(string.Concat("Array.CreateInstance(", typeName));
                if (arrayCreateExpression.Arguments.Count <= 0)
                {
                    Append(", 0)");
                }
                else
                {
                    foreach (Expression argument in arrayCreateExpression.Arguments)
                    {
                        Append(", ");
                        argument.AcceptVisitor(this, data);
                    }
                    Append(")");
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
                    obj = CreateSimpleAssignment(assignmentExpression, "=", data);
                    break;
                }
                case AssignmentOperatorType.Add:
                {
                    if (!IsAddEventHandler(assignmentExpression))
                    {
                        obj = CreateSimpleAssignment(assignmentExpression, "+=", data);
                        break;
                    }
                    obj = CreateHandlerStatement(assignmentExpression.Left, "+=", assignmentExpression.Right);
                    break;
                }
                case AssignmentOperatorType.Subtract:
                {
                    if (!IsRemoveEventHandler(assignmentExpression))
                    {
                        obj = CreateSimpleAssignment(assignmentExpression, "-=", data);
                        break;
                    }
                    obj = CreateHandlerStatement(assignmentExpression.Left, "-=", assignmentExpression.Right);
                    break;
                }
                case AssignmentOperatorType.Multiply:
                {
                    obj = CreateSimpleAssignment(assignmentExpression, "*=", data);
                    break;
                }
                case AssignmentOperatorType.Divide:
                case AssignmentOperatorType.DivideInteger:
                {
                    obj = CreateSimpleAssignment(assignmentExpression, "/=", data);
                    break;
                }
                case AssignmentOperatorType.Modulus:
                {
                    obj = CreateSimpleAssignment(assignmentExpression, "%=", data);
                    break;
                }
                case AssignmentOperatorType.Power:
                {
                    obj = CreateSimpleAssignment(assignmentExpression, "**=", data);
                    break;
                }
                case AssignmentOperatorType.ConcatString:
                {
                    obj = CreateSimpleAssignment(assignmentExpression, "+=", data);
                    break;
                }
                case AssignmentOperatorType.ShiftLeft:
                {
                    obj = CreateSimpleAssignment(assignmentExpression, "<<=", data);
                    break;
                }
                case AssignmentOperatorType.ShiftRight:
                {
                    obj = CreateSimpleAssignment(assignmentExpression, ">>=", data);
                    break;
                }
                case AssignmentOperatorType.BitwiseAnd:
                {
                    obj = CreateSimpleAssignment(assignmentExpression, "&=", data);
                    break;
                }
                case AssignmentOperatorType.BitwiseOr:
                {
                    obj = CreateSimpleAssignment(assignmentExpression, "|=", data);
                    break;
                }
                case AssignmentOperatorType.ExclusiveOr:
                {
                    obj = CreateSimpleAssignment(assignmentExpression, "^=", data);
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
            Append("self");
            return null;
        }

        public override object TrackedVisitBinaryOperatorExpression(BinaryOperatorExpression binaryOperatorExpression, object data)
        {
            binaryOperatorExpression.Left.AcceptVisitor(this, data);
            Append(" ");
            Append(GetBinaryOperator(binaryOperatorExpression.Op));
            Append(" ");
            binaryOperatorExpression.Right.AcceptVisitor(this, data);
            return null;
        }

        public override object TrackedVisitBlockStatement(BlockStatement blockStatement, object data)
        {
            return blockStatement.AcceptChildren(this, data);
        }

        public override object TrackedVisitBreakStatement(BreakStatement breakStatement, object data)
        {
            AppendIndentedLine("break");
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
            Append(" if ");
            conditionalExpression.Condition.AcceptVisitor(this, data);
            Append(" else ");
            conditionalExpression.FalseExpression.AcceptVisitor(this, data);
            return null;
        }

        public override object TrackedVisitConstructorDeclaration(ConstructorDeclaration constructorDeclaration, object data)
        {
            CreateConstructor(constructorInfo);
            return null;
        }

        public override object TrackedVisitConstructorInitializer(ConstructorInitializer constructorInitializer, object data)
        {
            Console.WriteLine("VisitConstructorInitializer");
            return null;
        }

        public override object TrackedVisitContinueStatement(ContinueStatement continueStatement, object data)
        {
            AppendIndentedLine("continue");
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
            AppendIndentedLine("def __del__(self):");
            IncreaseIndent();
            destructorDeclaration.Body.AcceptVisitor(this, data);
            DecreaseIndent();
            return null;
        }

        public override object TrackedVisitDirectionExpression(DirectionExpression directionExpression, object data)
        {
            Console.WriteLine("VisitDirectionExpression");
            return null;
        }

        public override object TrackedVisitDoLoopStatement(DoLoopStatement doLoopStatement, object data)
        {
            AppendIndented("while ");
            doLoopStatement.Condition.AcceptVisitor(this, data);
            Append(":");
            AppendLine();
            IncreaseIndent();
            doLoopStatement.EmbeddedStatement.AcceptVisitor(this, data);
            DecreaseIndent();
            return null;
        }

        public override object TrackedVisitElseIfSection(ElseIfSection elseIfSection, object data)
        {
            AppendIndented("elif ");
            elseIfSection.Condition.AcceptVisitor(this, data);
            Append(":");
            AppendLine();
            IncreaseIndent();
            elseIfSection.EmbeddedStatement.AcceptVisitor(this, data);
            DecreaseIndent();
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
            AppendIndented(string.Empty);
            expressionStatement.Expression.AcceptVisitor(this, data);
            AppendLine();
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
            AppendIndented(string.Empty);
            CreateInitStatement(foreachStatement);
            AppendLine();
            AppendIndentedLine("while enumerator.MoveNext():");
            IncreaseIndent();
            AppendIndentedLine(string.Concat(foreachStatement.VariableName, " = enumerator.Current"));
            foreachStatement.EmbeddedStatement.AcceptVisitor(this, data);
            DecreaseIndent();
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
            AppendIndented("while ");
            forStatement.Condition.AcceptVisitor(this, data);
            Append(":");
            AppendLine();
            IncreaseIndent();
            forStatement.EmbeddedStatement.AcceptVisitor(this, data);
            foreach (Statement iterator in forStatement.Iterator)
            {
                iterator.AcceptVisitor(this, data);
            }
            DecreaseIndent();
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
            if (IsField(identifier))
            {
                Append(string.Concat("self._", identifier));
            }
            else if (!IsProperty(identifier) || IsMethodParameter(identifier))
            {
                Append(identifier);
            }
            else
            {
                Append(string.Concat("self.", identifier));
            }
            return null;
        }

        public override object TrackedVisitIfElseStatement(IfElseStatement ifElseStatement, object data)
        {
            AppendIndented("if ");
            ifElseStatement.Condition.AcceptVisitor(this, data);
            Append(":");
            AppendLine();
            IncreaseIndent();
            foreach (Statement trueStatement in ifElseStatement.TrueStatement)
            {
                trueStatement.AcceptVisitor(this, data);
            }
            DecreaseIndent();
            if (ifElseStatement.HasElseIfSections)
            {
                foreach (ElseIfSection elseIfSection in ifElseStatement.ElseIfSections)
                {
                    elseIfSection.AcceptVisitor(this, data);
                }
            }
            if (ifElseStatement.HasElseStatements)
            {
                AppendIndentedLine("else:");
                IncreaseIndent();
                foreach (Statement falseStatement in ifElseStatement.FalseStatement)
                {
                    falseStatement.AcceptVisitor(this, data);
                }
                DecreaseIndent();
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
                Append("[");
                index.AcceptVisitor(this, data);
                Append("]");
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
                Append(string.Concat(".", targetObject.MemberName));
            }
            else if (identifierExpression != null)
            {
                if (currentMethod == null ? true : !IsStatic(currentMethod))
                {
                    Append("self.");
                }
                else
                {
                    Append(string.Concat(GetTypeName(currentMethod), "."));
                }
                Append(identifierExpression.Identifier);
            }
            Append("(");
            bool flag = true;
            foreach (Expression argument in invocationExpression.Arguments)
            {
                if (!flag)
                {
                    Append(", ");
                }
                else
                {
                    flag = false;
                }
                argument.AcceptVisitor(this, data);
            }
            Append(")");
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
                AppendIndented(string.Concat(item.Name, " = "));
                item.Initializer.AcceptVisitor(this, data);
                AppendLine();
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
            if (!(memberReferenceExpression.TargetObject is ThisReferenceExpression) ? true : IsProperty(memberReferenceExpression.MemberName))
            {
                Append(".");
            }
            else
            {
                Append("._");
            }
            Append(memberReferenceExpression.MemberName);
            return null;
        }

        public override object TrackedVisitMethodDeclaration(MethodDeclaration methodDeclaration, object data)
        {
            currentMethod = methodDeclaration;
            AppendIndented(string.Concat("def ", methodDeclaration.Name));
            AddParameters(methodDeclaration);
            methodParameters = methodDeclaration.Parameters;
            AppendLine();
            IncreaseIndent();
            AppendDocstring(xmlDocComments);
            if (methodDeclaration.Body.Children.Count <= 0)
            {
                AppendIndentedPassStatement();
            }
            else
            {
                methodDeclaration.Body.AcceptVisitor(this, data);
            }
            DecreaseIndent();
            AppendLine();
            if (IsStatic(methodDeclaration))
            {
                AppendIndentedLine(string.Concat(methodDeclaration.Name, " = staticmethod(", methodDeclaration.Name, ")"));
                AppendLine();
                SaveMethodIfMainEntryPoint(methodDeclaration);
            }
            currentMethod = null;
            return null;
        }

        public override object TrackedVisitNamedArgumentExpression(NamedArgumentExpression namedArgumentExpression, object data)
        {
            Append(namedArgumentExpression.Name);
            Append(" = ");
            namedArgumentExpression.Expression.AcceptVisitor(this, data);
            return null;
        }

        public override object TrackedVisitNamespaceDeclaration(NamespaceDeclaration namespaceDeclaration, object data)
        {
            return namespaceDeclaration.AcceptChildren(this, data);
        }

        public override object TrackedVisitObjectCreateExpression(ObjectCreateExpression objectCreateExpression, object data)
        {
            Append(objectCreateExpression.CreateType.Type);
            if (IsGenericType(objectCreateExpression))
            {
                AppendGenericTypes(objectCreateExpression);
            }
            Append("(");
            bool flag = true;
            foreach (Expression parameter in objectCreateExpression.Parameters)
            {
                if (!flag)
                {
                    Append(", ");
                }
                parameter.AcceptVisitor(this, data);
                flag = false;
            }
            bool flag1 = true;
            foreach (Expression createExpression in objectCreateExpression.ObjectInitializer.CreateExpressions)
            {
                if (!flag1)
                {
                    Append(", ");
                }
                createExpression.AcceptVisitor(this, data);
                flag1 = false;
            }
            Append(")");
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
            Append("(");
            parenthesizedExpression.Expression.AcceptVisitor(this, data);
            Append(")");
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
                Append("None");
            }
            else if (!(primitiveExpression.Value is bool))
            {
                Append(primitiveExpression.StringValue);
            }
            else
            {
                Append(primitiveExpression.Value.ToString());
            }
            return null;
        }

        public override object TrackedVisitPropertyDeclaration(PropertyDeclaration propertyDeclaration, object data)
        {
            string name = propertyDeclaration.Name;
            propertyNames.Add(name);
            if (propertyDeclaration.HasGetRegion)
            {
                AppendIndentedLine(string.Concat("def get_", name, "(self):"));
                IncreaseIndent();
                propertyDeclaration.GetRegion.Block.AcceptVisitor(this, data);
                DecreaseIndent();
                AppendLine();
            }
            if (propertyDeclaration.HasSetRegion)
            {
                AppendIndentedLine(string.Concat("def set_", name, "(self, value):"));
                IncreaseIndent();
                propertyDeclaration.SetRegion.Block.AcceptVisitor(this, data);
                DecreaseIndent();
                AppendLine();
            }
            AppendPropertyDecorator(propertyDeclaration);
            AppendLine();
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
            AppendIndented("return ");
            returnStatement.Expression.AcceptVisitor(this, data);
            AppendLine();
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
                CreateSwitchCaseCondition(switchStatement.SwitchExpression, switchSection, flag);
                IncreaseIndent();
                CreateSwitchCaseBody(switchSection);
                DecreaseIndent();
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
            Append("self");
            return null;
        }

        public override object TrackedVisitThrowStatement(ThrowStatement throwStatement, object data)
        {
            AppendIndented("raise ");
            throwStatement.Expression.AcceptVisitor(this, data);
            AppendLine();
            return null;
        }

        public override object TrackedVisitTryCatchStatement(TryCatchStatement tryCatchStatement, object data)
        {
            AppendIndentedLine("try:");
            IncreaseIndent();
            tryCatchStatement.StatementBlock.AcceptVisitor(this, data);
            DecreaseIndent();
            foreach (CatchClause catchClause in tryCatchStatement.CatchClauses)
            {
                AppendIndented("except ");
                Append(catchClause.TypeReference.Type);
                Append(string.Concat(", ", catchClause.VariableName, ":"));
                AppendLine();
                IncreaseIndent();
                catchClause.StatementBlock.AcceptVisitor(this, data);
                DecreaseIndent();
            }
            AppendIndentedLine("finally:");
            IncreaseIndent();
            tryCatchStatement.FinallyBlock.AcceptVisitor(this, data);
            DecreaseIndent();
            return null;
        }

        public override object TrackedVisitTypeDeclaration(TypeDeclaration typeDeclaration, object data)
        {
            codeBuilder.AppendLineIfPreviousLineIsCode();
            AppendIndented(string.Concat("class ", typeDeclaration.Name));
            AppendBaseTypes(typeDeclaration.BaseTypes);
            AppendLine();
            IncreaseIndent();
            AppendDocstring(xmlDocComments);
            if (typeDeclaration.Children.Count <= 0)
            {
                AppendIndentedPassStatement();
            }
            else
            {
                constructorInfo = PythonConstructorInfo.GetConstructorInfo(typeDeclaration);
                if (constructorInfo != null)
                {
                    if (constructorInfo.Constructor == null)
                    {
                        CreateConstructor(constructorInfo);
                    }
                }
                typeDeclaration.AcceptChildren(this, data);
            }
            DecreaseIndent();
            return null;
        }

        public override object TrackedVisitTypeOfExpression(TypeOfExpression typeOfExpression, object data)
        {
            codeBuilder.InsertIndentedLine("import clr\r\n");
            Append("clr.GetClrType(");
            Append(GetTypeName(typeOfExpression.TypeReference));
            Append(")");
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
            Append(GetTypeName(typeReferenceExpression.TypeReference));
            return null;
        }

        public override object TrackedVisitUnaryOperatorExpression(UnaryOperatorExpression unaryOperatorExpression, object data)
        {
            object obj;
            switch (unaryOperatorExpression.Op)
            {
                case UnaryOperatorType.Not:
                {
                    obj = CreateUnaryOperatorStatement("not ", unaryOperatorExpression.Expression);
                    break;
                }
                case UnaryOperatorType.BitNot:
                {
                    obj = CreateUnaryOperatorStatement("~", unaryOperatorExpression.Expression);
                    break;
                }
                case UnaryOperatorType.Minus:
                {
                    obj = CreateUnaryOperatorStatement(GetBinaryOperator(BinaryOperatorType.Subtract), unaryOperatorExpression.Expression);
                    break;
                }
                case UnaryOperatorType.Plus:
                {
                    obj = CreateUnaryOperatorStatement(GetBinaryOperator(BinaryOperatorType.Add), unaryOperatorExpression.Expression);
                    break;
                }
                case UnaryOperatorType.Increment:
                case UnaryOperatorType.PostIncrement:
                {
                    obj = CreateIncrementStatement(unaryOperatorExpression);
                    break;
                }
                case UnaryOperatorType.Decrement:
                case UnaryOperatorType.PostDecrement:
                {
                    obj = CreateDecrementStatement(unaryOperatorExpression);
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
                AppendIndentedLine(string.Concat("from ", @using.Name, " import *"));
            }
            return null;
        }

        public override object TrackedVisitUsingStatement(UsingStatement usingStatement, object data)
        {
            return null;
        }

        public override object TrackedVisitVariableDeclaration(VariableDeclaration variableDeclaration, object data)
        {
            AppendIndented(string.Concat(variableDeclaration.Name, " = "));
            variableDeclaration.Initializer.AcceptVisitor(this, data);
            AppendLine();
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