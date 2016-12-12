using System;
using System.Collections.Generic;

namespace App2.ID3
{
    public class Node
    {
        public List<Node> children;
        public string path;

        // a node can be a branching (combination of attr & cond), OR a result
        public string attribute, condition;
        public string result;

        public Node parent; // if null, then this is root

        public Node()
        {
            children = new List<Node>();
        }

        public bool isLeaf()
        {
            return result != null;
        }

        public void printPath()
        {
            printPath(null);
        }

        /*
         * param printTo: Null means do direct print
         */
        public void printPath(List<string> printTo)
        {
            foreach (Node node in this.children)
            {
                if (node.isLeaf())
                {
                    if (printTo == null)
                        Console.WriteLine(node.path);
                    else
                        printTo.Add(node.path);
                }
                else
                {
                    node.printPath(printTo);
                }
            }
        }

        public int leafCount()
        {
            if (isLeaf())
                return 1;

            int count = 0;

            foreach (Node node in this.children)
            {
                count += node.leafCount();
            }

            return count;
        }

        /*
         * Result: Boolean.TRUE -> Correctly classified Boolean.FALSE -> Incorrectly
         * classified null -> Unclassified
         */
        public bool? test(DataSource testData, int lineNo)
        {
            Line line = testData.data[lineNo];

            if (this.result != null)
            {
                string classValue = line[testData.classAttribute.index];
                line.isMisclassfied = this.result.Equals(classValue);
                return line.isMisclassfied;
            }
            else
            {
                for (int i = 0; i < testData.attributes.Count; i++)
                {
                    if (testData.attributes[i].name.Equals(this.attribute))
                    {
                        string cell = line[i];

                        foreach (Node node in this.children)
                        {
                            if (node.condition.Equals(cell))
                            {
                                return node.test(testData, lineNo);
                            }
                        }
                    }
                }
            }

            line.isUnclassified = true;
            return null; // unclassified
        }
    }
}