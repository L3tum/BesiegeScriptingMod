using System.ComponentModel;
using System.Text;

namespace BesiegeScriptingMod.Util
{
    public class PythonCodeBuilder
    {
        private StringBuilder codeBuilder = new StringBuilder();

        private string indentString = "\t";

        private int indent;

        private bool insertedCreateComponentsContainer;

        public int Indent
        {
            get
            {
                return this.indent;
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

        public int Length
        {
            get
            {
                return this.codeBuilder.Length;
            }
        }

        public bool PreviousLineIsCode
        {
            get
            {
                bool flag;
                string str = this.ToString();
                int previousLineEnd = this.MoveToPreviousLineEnd(str, str.Length - 1);
                if (previousLineEnd <= 0)
                {
                    flag = false;
                }
                else
                {
                    int num = this.MoveToPreviousLineEnd(str, previousLineEnd);
                    string str1 = str.Substring(num + 1, previousLineEnd - num).Trim();
                    flag = (str1.Length <= 0 ? false : !str1.Trim().StartsWith("#"));
                }
                return flag;
            }
        }

        public PythonCodeBuilder()
        {
        }

        public PythonCodeBuilder(int initialIndent)
        {
            this.indent = initialIndent;
        }

        public void Append(string text)
        {
            this.codeBuilder.Append(text);
        }

        public void AppendIndented(string text)
        {
            this.codeBuilder.Append(this.GetIndentString());
            this.codeBuilder.Append(text);
        }

        public void AppendIndentedLine(string text)
        {
            this.AppendIndented(string.Concat(text, "\r\n"));
        }

        public void AppendLine()
        {
            this.Append("\r\n");
        }

        public void AppendLineIfPreviousLineIsCode()
        {
            if (this.PreviousLineIsCode)
            {
                this.codeBuilder.AppendLine();
            }
        }

        public void AppendToPreviousLine(string text)
        {
            string str = this.ToString();
            int previousLineEnd = this.MoveToPreviousLineEnd(str, str.Length - 1);
            if (previousLineEnd > 0)
            {
                this.codeBuilder.Insert(previousLineEnd + 1, text);
            }
        }

        public void DecreaseIndent()
        {
            PythonCodeBuilder pythonCodeBuilder = this;
            pythonCodeBuilder.indent = pythonCodeBuilder.indent - 1;
        }

        private string GetIndentString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < this.indent; i++)
            {
                stringBuilder.Append(this.indentString);
            }
            return stringBuilder.ToString();
        }

        public void IncreaseIndent()
        {
            PythonCodeBuilder pythonCodeBuilder = this;
            pythonCodeBuilder.indent = pythonCodeBuilder.indent + 1;
        }

        public void InsertCreateComponentsContainer()
        {
            if (!this.insertedCreateComponentsContainer)
            {
                this.InsertIndentedLine(string.Concat("self._components = ", typeof(Container).FullName, "()"));
                this.insertedCreateComponentsContainer = true;
            }
        }

        public void InsertIndentedLine(string text)
        {
            text = string.Concat(this.GetIndentString(), text, "\r\n");
            this.codeBuilder.Insert(0, text, 1);
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
                else if (code[index] != '\r')
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
            return this.codeBuilder.ToString();
        }
    }
}