#region usings

using System.ComponentModel;
using System.Text;

#endregion

namespace BesiegeScriptingMod.Util
{
    public class PythonCodeBuilder
    {
        private readonly StringBuilder codeBuilder = new StringBuilder();

        private bool insertedCreateComponentsContainer;

        public PythonCodeBuilder()
        {
        }

        public PythonCodeBuilder(int initialIndent)
        {
            Indent = initialIndent;
        }

        public int Indent { get; private set; }

        public string IndentString { get; set; } = "\t";

        public int Length
        {
            get { return codeBuilder.Length; }
        }

        public bool PreviousLineIsCode
        {
            get
            {
                bool flag;
                string str = ToString();
                int previousLineEnd = MoveToPreviousLineEnd(str, str.Length - 1);
                if (previousLineEnd <= 0)
                {
                    flag = false;
                }
                else
                {
                    int num = MoveToPreviousLineEnd(str, previousLineEnd);
                    string str1 = str.Substring(num + 1, previousLineEnd - num).Trim();
                    flag = str1.Length <= 0 ? false : !str1.Trim().StartsWith("#");
                }
                return flag;
            }
        }

        public void Append(string text)
        {
            codeBuilder.Append(text);
        }

        public void AppendIndented(string text)
        {
            codeBuilder.Append(GetIndentString());
            codeBuilder.Append(text);
        }

        public void AppendIndentedLine(string text)
        {
            AppendIndented(string.Concat(text, "\r\n"));
        }

        public void AppendLine()
        {
            Append("\r\n");
        }

        public void AppendLineIfPreviousLineIsCode()
        {
            if (PreviousLineIsCode)
            {
                codeBuilder.AppendLine();
            }
        }

        public void AppendToPreviousLine(string text)
        {
            string str = ToString();
            int previousLineEnd = MoveToPreviousLineEnd(str, str.Length - 1);
            if (previousLineEnd > 0)
            {
                codeBuilder.Insert(previousLineEnd + 1, text);
            }
        }

        public void DecreaseIndent()
        {
            PythonCodeBuilder pythonCodeBuilder = this;
            pythonCodeBuilder.Indent = pythonCodeBuilder.Indent - 1;
        }

        private string GetIndentString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < Indent; i++)
            {
                stringBuilder.Append(IndentString);
            }
            return stringBuilder.ToString();
        }

        public void IncreaseIndent()
        {
            PythonCodeBuilder pythonCodeBuilder = this;
            pythonCodeBuilder.Indent = pythonCodeBuilder.Indent + 1;
        }

        public void InsertCreateComponentsContainer()
        {
            if (!insertedCreateComponentsContainer)
            {
                InsertIndentedLine(string.Concat("self._components = ", typeof (Container).FullName, "()"));
                insertedCreateComponentsContainer = true;
            }
        }

        public void InsertIndentedLine(string text)
        {
            text = string.Concat(GetIndentString(), text, "\r\n");
            codeBuilder.Insert(0, text, 1);
        }

        private int MoveToPreviousLineEnd(string code, int index)
        {
            int num;
            while (true)
            {
                if (index < 0)
                {
                    num = -1;
                    break;
                }
                if (code[index] != '\r')
                {
                    index--;
                }
                else
                {
                    num = index - 1;
                    break;
                }
            }
            return num;
        }

        public override string ToString()
        {
            return codeBuilder.ToString();
        }
    }
}