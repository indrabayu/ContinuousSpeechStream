using System;
using System.Collections.Generic;

namespace App2.ID3
{
    public class Attribute
    {
        public string name;

        public int index;

        public List<string> values;

        public Attribute()
        {
            values = new List<string>();
        }

        public object clone()
        {
            Attribute attr = new Attribute();
            attr.name = this.name;

            attr.index = this.index;
            try
            {

                attr.values = (List<string>)values.Clone();

            }
            catch (Exception err)
            {
            }

            return attr;
        }

        public string toString()
        {
            return name + " " + values.ToString();
        }
    }
}