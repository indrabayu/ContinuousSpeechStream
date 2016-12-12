using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace App2.ID3
{
    public class Line : List<string>
    {
        public int no;
        public bool flag; // mark to be deleted, or for anything else
        public bool isMisclassfied;
        public bool isUnclassified;

        public Line doClone()
        {
            Line copyLine = new Line();
            copyLine.no = this.no;

            foreach (string str in this)
            {
                copyLine.Add(str);
            }

            return copyLine;
        }

        public string toString()
        {
            //return base.ToString();

            StringBuilder sb = new StringBuilder();
            sb.Append('[');
            for (int i = 0; i < this.Count; i++)
            {
                string s = this[i];
                sb.Append(s);
                if (i + 1 < Count)
                    sb.Append(",");
            }
            sb.Append(']');

            return sb.ToString();
        }

        public string toStringWithoutClassValue(int attrIndex)
        {
            Line copy = doClone();
            copy.RemoveAt(attrIndex);
            return copy.toString();
        }
    }
}