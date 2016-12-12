using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace App2.ID3
{
    public class DataSource
    {
        public string relation;

        public Attribute classAttribute;

        public List<Attribute> attributes;

        public List<Line> data;

        public int size = 0;

        public bool should_print = false;
        public bool scrambleBeforeTesting = true;
        public bool printToDefaultStreamOnTest = true;
        public bool printToStringBuilderOnTest = true;

        public Entropy entropy = new ShannonEntropy();

        // new HavrdaCharvatEntropy(HavrdaCharvatEntropy.Alpha_75Percent);

        /*
         * Private modifier of constructor makes sure that this class will never be
         * manually instantiated.
         */
        private DataSource()
        {
            attributes = new List<Attribute>();
            data = new List<Line>();
        }

        public object clone()
        {
            return doClone();
        }

        public DataSource doClone()
        {
            DataSource ds = new DataSource();
            cloneExceptData(ds);

            ds.data = new List<Line>();
            foreach (Line line in this.data)
            {
                ds.data.Add(line.doClone());
            }

            ds.size = ds.data.Count;

            return ds;
        }

        void cloneExceptData(DataSource ds)
        {
            ds.relation = this.relation;
            ds.should_print = this.should_print;
            ds.entropy = this.entropy;

            try
            {
                ds.attributes = new List<Attribute>(this.attributes);
            }
            catch (Exception err)
            {
            }

            foreach (Attribute attr in ds.attributes)
            {
                if (attr.name.Equals(classAttribute.name))
                {
                    ds.classAttribute = attr;
                    break;
                }
            }
        }

        public string toString()
        {
            return data.Count + ""; // Actual size is important in debugging
        }

        /*
         * Read a dataset from an ARFF file
         */
        public static DataSource getDataSource(string filename,
                string classAttributeName, string attrSplitString,
                bool replaceQuestionMark, Filter filter)
        {
            DataSource dataSource = new DataSource();

            if (attrSplitString == null)
                attrSplitString = ", ";

            try
            {
                //FileInputStream fstream = new FileInputStream(filename);
                //DataInputStream ins = new DataInputStream(fstream);
                //BufferedReader br = new BufferedReader(new InputStreamReader(ins));
                string[] lines = System.IO.File.ReadAllLines(filename);
                string strLine;

                int attrCount = 0, originalLineNo = 0;
                //while ((strLine = br.ReadLine()) != null)
                for (int i = 0; i < lines.Length; i++)
                {
                    strLine = lines[i];

                    if (strLine == null)
                        break;

                    if (strLine.Length > 0)
                    {
                        if (strLine[0] == '%') // skip dataset comments
                            continue;
                    }

                    string firstSentence = strLine.Split(new string[] { " " }, StringSplitOptions.None)[0];

                    switch (firstSentence.ToLower())
                    {
                        case "@relation":
                            dataSource.relation = strLine.ReplaceFirst("@RELATION", "")
                                    .ReplaceFirst("@relation", "").Trim();
                            break;
                        case "@attribute":
                            strLine = strLine.ReplaceFirst("@ATTRIBUTE", "")
                                    .ReplaceFirst("@attribute", "").Trim();
                            Attribute attr = new Attribute();
                            attr.name = strLine.Split(new string[] { " " }, StringSplitOptions.None)[0];
                            strLine = strLine.ReplaceFirst(attr.name, "").Trim();
                            string[] values = strLine.Replace("{", "").Replace("}", "")
                                    .Split(new string[] { attrSplitString }, StringSplitOptions.None);

                            foreach (string value in values)
                            {
                                attr.values.Add(value.Trim());
                            }
                            attr.index = attrCount;
                            attrCount++;
                            dataSource.attributes.Add(attr);

                            if (classAttributeName == null)
                                dataSource.classAttribute = attr;// keep setting until
                                                                 // reaching the last
                            else if (attr.name.ToLower().Equals(classAttributeName.ToLower()))
                                dataSource.classAttribute = attr;

                            break;
                        case "@data":
                            //while ((strLine = br.ReadLine()) != null)
                            for (int j = i + 1; j < lines.Length; i++, j++)
                            {
                                strLine = lines[j];

                                // Continue when not real data
                                if (strLine.Equals(string.Empty))
                                    continue;
                                if (strLine[0] == '%')
                                    continue;

                                string[] data = strLine.Split(new string[] { "," }, StringSplitOptions.None);
                                if (data.Length != dataSource.attributes.Count)
                                    break; // end of data

                                Line line = new Line();
                                foreach (string value in data)
                                {
                                    line.Add(value.Trim());
                                }
                                line.no = ++originalLineNo;

                                dataSource.data.Add(line); // allow duplicates
                            }
                            break;
                        default:
                            // not recognized yet
                            break;
                    }
                }

                if (classAttributeName != null && dataSource.classAttribute == null)
                {
                    string warning = "There's no such attribute: "
                            + classAttributeName;
                    System.Console.Error.WriteLine(warning);
                    throw new Exception(warning);
                }

                if (!dataSource.hasConsistentColumnsInData()) // can be used to
                                                              // detect missing
                                                              // value..
                {
                    string warning = "Dataset has inconsistent column number!";
                    System.Console.Error.WriteLine(warning);
                    throw new Exception(warning);
                }

                if (replaceQuestionMark)
                    dataSource.replaceQuestionMarkWithTheMostPopularValueInColumn();

                dataSource.filterBeforeProcessing(filter);

                dataSource.size = dataSource.data.Count; // also applied in clone
                                                         // method

                //ins.Close();
            }
            catch (Exception err)
            {
                System.Console.Error.WriteLine(err.ToString());
            }

            return dataSource;
        }

        /*
         * Check whether all rows has equal number of columns
         */
        private bool hasConsistentColumnsInData()
        {
            HashSet<string> hash = new HashSet<string>();
            foreach (List<string> line in data)
                hash.Add(line.Count.ToString());
            int countHash = hash.Count;
            return countHash == 1;
        }

        private void replaceQuestionMarkWithTheMostPopularValueInColumn()
        {
            foreach (Attribute attr in attributes)
            {
                string bestValue = "";
                int highestOccurance = 0;

                foreach (string value in attr.values)
                {
                    int occurance = getCountOfValueInAttribute(attr.index, value);
                    if (occurance > highestOccurance)
                    {
                        highestOccurance = occurance;
                        bestValue = value;
                    }
                }

                foreach (Line line in data)
                {
                    string cell = line.ElementAt(attr.index);
                    switch (cell)
                    {
                        case "?":
                        case "'?'":
                            // Swap question-mark with the most popular value
                            line[attr.index] = bestValue;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public void filterBeforeProcessing(Filter filter)
        {
            if (filter == null)
                return;

            if (filter.do_DeleteAdjacentDuplicateRows) // Attribute-dependant filter
                deleteAdjacentDuplicateRows();

            for (int i = attributes.Count - 1; i >= 0; i--)
            {
                Attribute attr = attributes[i];

                if (attr == classAttribute)
                    continue;

                bool isKey = false;
                bool isSingleValued = false;
                bool hasMajorityValue = false;

                if (filter.do_IsKeyColumn)
                    isKey = isKeyColumn(attr.index);

                if (filter.do_IsSingleValuedAttribute)
                    isSingleValued = 1 == attr.values.Count;

                if (filter.do_HasMajorityValue)
                {
                    foreach (string value in attr.values)
                    {
                        int occurance = getCountOfValueInAttribute(attr.index,
                                value);
                        double population = occurance / (double)data.Count;
                        if (population >= filter.majorityThreshold)
                        {
                            hasMajorityValue = true;
                            break;
                        }
                    }
                }

                if (isKey || isSingleValued || hasMajorityValue)
                {
                    removeAttribute(attr.index);
                }
            }

            reIndexAttributes();
        }

        private void deleteAdjacentDuplicateRows()
        {
            this.data = new List<Line>(new HashSet<Line>(this.data));
        }

        /*
         * Check whether all rows for this particular column has a unique value
         * (like a primary key in database concept)
         */
        private bool isKeyColumn(int columnIndex)
        {
            HashSet<string> rows = new HashSet<string>();

            foreach (Line line in data)
            {
                rows.Add(line[columnIndex]);
            }

            // everything in this Column unique?
            bool result = rows.Count == this.data.Count;

            return result;
        }

        /*
         * Suggested to be called when the indeces are unstable
         */
        public void reIndexAttributes()
        {
            for (int i = 0; i < attributes.Count; i++)
            {
                attributes[i].index = i;
            }
        }

        /*
         * Initial process
         */
        public void process(Node parent)
        {
            process("", parent, null);
            reIndexAttributes();
            GC.Collect();
        }

        private void process(string path, Node parent, string condition) // never
                                                                         // manually
                                                                         // call!
        {
            if (attributes.Count <= 1)
                return;

            Node currNode = new Node();

            if (classCategoryVariants() == 1) // are class values are uniformed?
            {
                string result = data[0][classAttribute.index];

                path = path + " : " + result;
                currNode.attribute = null;
                currNode.condition = condition;
                currNode.result = result;
                currNode.path = path;
                parent.children.Add(currNode);
                currNode.parent = parent;
                currNode.path = path;

                if (should_print)
                {
                    System.Console.WriteLine("Path " + path + " results in " + result);
                }

                return;
            }

            Attribute attr = getHighestGain();

            if (attr == null)
            {
                if (should_print)
                    System.Console.WriteLine("Path " + path + " has unknown result");
                return;
            }

            if (path == "" && parent.parent == null /* top-most node */)
            {
                path = attr.name;
                parent.attribute = attr.name;
                parent.condition = null; // and it needs no condition
                parent.result = null;
                currNode = parent;
            }
            else
            {
                path = path + " -> " + attr.name;
                currNode.attribute = attr.name;
                currNode.condition = condition;
                currNode.result = null;
                parent.children.Add(currNode);
                currNode.parent = parent;
            }
            currNode.path = path;

            for (int i = 0; i < attr.values.Count; i++)
            {
                DataSource dsPerCategory;

                try
                {
                    dsPerCategory = (DataSource)this.clone();

                    //List<Line>.Enumerator iter = dsPerCategory.data.GetEnumerator();

                    string iif = attr.values[i];

                    //while (iter.hasNext())
                    //{
                    //    Line line = (Line)iter.next();
                    //    string inTable = line[attr.index];
                    //    if (inTable.Equals(iif) == false)
                    //    {
                    //        iter.remove();
                    //    }
                    //}

                    dsPerCategory.data.RemoveAll(line =>
                    {
                        string inTable = line[attr.index];
                        return inTable.Equals(iif) == false;
                    });

                    dsPerCategory.data.TrimExcess();

                    dsPerCategory.size = dsPerCategory.data.Count;

                    dsPerCategory.relation += "." + attr.name;

                    dsPerCategory.removeAttribute(attr.index);

                    // dsPerCategory.printAll();

                    dsPerCategory.process(path + "(" + iif + ")", currNode, iif);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.ToString());
                }
            }
        }

        // This field is here so 3 methods below won't rescan the dataset twice
        private List<Line> m_vagueRows;

        public bool hasEqualValuesButDifferentClass()
        {
            /*
             * (gender) - (age) - (smoke/not) - (CLASS: has/no cancer) MALE - YOUNG
             * - SMOKE - HAS_CANCER MALE - YOUNG - SMOKE - NO_CANCER
             * 
             * --> Such ambiguous data is not acceptable!
             */

            if (m_vagueRows == null)
            {
                // first call to this method
                m_vagueRows = getVagueRows();
            }

            return m_vagueRows.Count > 0;
        }

        public void deleteVagueRows()
        {
            if (m_vagueRows == null)
            {
                // hasEqualValuesButDifferentClass has not yet been called
                m_vagueRows = getVagueRows();
            }

            if (m_vagueRows.Count > 0)
            {
                //List<Line>.Enumerator iter = m_vagueRows.GetEnumerator();
                //while (iter.hasNext())
                //{
                //    data.remove(iter.next());
                //}

                m_vagueRows.ForEach(line => data.Remove(line));
            }
        }

        public List<Line> getVagueRows()
        {
            DataSource copy = this.doClone();
            List<Line> hasDifferentClassValue = new List<Line>();

            reIndexAttributes();

            for (int i = 0; i < copy.data.Count - 1; i++)
            {
                Line outter = copy.data[i];

                if (outter.flag)
                    continue;

                for (int j = copy.data.Count - 1; j > i; j--)
                {
                    Line inner = copy.data[j];

                    if (inner.flag)
                        continue;

                    if (outter
                            .toStringWithoutClassValue(copy.classAttribute.index)
                            .Equals(inner
                                    .toStringWithoutClassValue(copy.classAttribute.index)))
                    {
                        if (!outter[copy.classAttribute.index].Equals(
                                inner[copy.classAttribute.index]))
                        {
                            outter.flag = true;
                            inner.flag = true;
                            hasDifferentClassValue.Add(inner);
                            break;
                        }
                    }
                }

                if (outter.flag)
                    hasDifferentClassValue.Add(outter);
            }

            return hasDifferentClassValue;
        }

        public void setClassAttribute(string attrName)
        {
            for (int i = 0; i < attributes.Count; i++)
            {
                if (attributes[i].name.Equals(attrName))
                {
                    setClassAttribute(i);
                    return;
                }
            }

            System.Console.Error.WriteLine("There's no such attribute: " + attrName);
        }

        public void setClassAttribute(int attrIndex)
        {
            if (attrIndex < 0 || attrIndex >= attributes.Count)
            {
                System.Console.Error.WriteLine("Wrong index assignment for class attribute.");
                return;
            }

            classAttribute = attributes[attrIndex];
        }

        public void printData()
        {
            if (should_print)
                foreach (Line line in data)
                {
                    System.Console.WriteLine(line);
                }
        }

        public void printAttributes()
        {
            if (should_print)
                foreach (Attribute attr in attributes)
                {
                    System.Console.WriteLine("Name = " + attr.name);
                    System.Console.WriteLine("Index = " + attr.index);
                    foreach (string value in attr.values)
                    {
                        System.Console.WriteLine(value);
                    }
                }
        }

        public void printClassValues()
        {
            if (should_print)
                foreach (string classValue in classAttribute.values)
                {
                    System.Console.WriteLine(classValue);
                }
        }

        public void printAll()
        {
            if (should_print)
            {
                System.Console.WriteLine("Attributes:");
                printAttributes();
                System.Console.WriteLine("------------------------------");
                System.Console.WriteLine("Data: (" + data.Count + ")");
                printData();
                System.Console.WriteLine("==========================================");
            }
        }

        public void removeAttribute(string attrName)
        {
            for (int i = 0; i < attributes.Count; i++)
            {
                if (attributes[i].name.Equals(attrName))
                {
                    removeAttribute(i);
                    return;
                }
            }

            System.Console.WriteLine("There's no such attribute: " + attrName);
        }

        /*
         * Removes an attribute, along with its corresponding column in the dataset
         */
        public void removeAttribute(int attrIndex)
        {
            foreach (Line line in data)
            {
                line.RemoveAt(attrIndex); // remove that attr from each line in data
            }

            attributes.RemoveAt(attrIndex); // drop the attribute itself
            reIndexAttributes(); // necessary after attribute deletion
        }

        public int getCountOfData()
        {
            return data.Count;
        }

        private int getCountOfValueInAttribute(string attrName, string value)
        {
            for (int i = 0; i < attributes.Count; i++)
            {
                if (attributes[i].name.Equals(attrName))
                {
                    return getCountOfValueInAttribute(i, value);
                }
            }

            // shouldn't reach this point
            return -1;
        }

        private int getCountOfValueInAttribute(int attrIndex, string value)
        {
            int total = 0;

            foreach (Line line in data)
            {
                if (line[attrIndex].Equals(value))
                    total++;
            }

            return total;
        }

        private int getCountOfValueInAttributeBasedOnClassValue(int attrIndex,
                string value, string classValue)
        {
            int total = 0;

            foreach (Line line in data)
            {
                string currValue = line[attrIndex];
                string currClassValue = line[classAttribute.index];

                if (currValue.Equals(value) && currClassValue.Equals(classValue))
                    total++;
            }

            return total;
        }

        private double getEntropyOfSet()
        {
            double result = 0;

            int possibleValuesOfClass = classAttribute.values.Count;

            int[] countOfValueInAttribute = new int[possibleValuesOfClass];
            double[] partialEntropies = new double[possibleValuesOfClass];

            for (int i = 0; i < possibleValuesOfClass; i++)
            {
                string valueInEnum = classAttribute.values[i];

                countOfValueInAttribute[i] = getCountOfValueInAttribute(
                        classAttribute.name, valueInEnum);

                /*
                 * --> Old way partialEntropies[i] =
                 * Entropy.entropy(countOfValueInAttribute[i], getCountOfData());
                 * 
                 * result += partialEntropies[i];
                 */

                entropy.countAndSave(countOfValueInAttribute[i], getCountOfData()); // New
                                                                                    // way
            }

            result = entropy.get();

            return result;
        }

        private double getGainOfAttribute(string attrName)
        {
            for (int i = 0; i < attributes.Count; i++)
            {
                if (attributes[i].name.Equals(attrName))
                {
                    return getGainOfAttribute(i);
                }
            }

            // shouldn't reach this point
            return -1;
        }

        private double getGainOfAttribute(int attrIndex)
        {
            double result = 0;

            /*
             * --> Old way result = getEntropyOfSet();
             */

            if (entropy.requiresEntropyOfSet)
                entropy.attributeEntropies.Add(getEntropyOfSet()); // New : only for
                                                                   // Shannon

            foreach (string value in attributes[attrIndex].values)
            {
                double partialEntropy = getEntropyOfValue(attrIndex, value);
                /*
                 * --> Old way result -= partialEntropy;
                 */

                entropy.attributeEntropies.Add(partialEntropy);
            }

            result = entropy.getGainOfAttribute(); // New way

            return result;
        }

        private double getEntropyOfValue(int attrIndex, string value)
        {
            int right = getCountOfValueInAttribute(attrIndex, value);

            double entropyOfValue = 0;

            foreach (string classValue in classAttribute.values)
            {
                int left = getCountOfValueInAttributeBasedOnClassValue(attrIndex,
                        value, classValue);

                /*
                 * --> Old way double perClassVal_Entropy = Entropy.entropy(left,
                 * right);
                 * 
                 * entropyOfValue += perClassVal_Entropy;
                 */

                entropy.countAndSave(left, right); // New way
            }

            // Now, adjust it to the whole set

            entropyOfValue = entropy.get();// New way

            entropyOfValue = right * entropyOfValue / size;

            return entropyOfValue;
        }

        private Attribute getHighestGain()
        {
            Attribute max = null;
            double bestSoFar = 0;

            bestSoFar = entropy.defaultFor_TemporaryEntropyBound; // New way

            for (int i = 0; i < attributes.Count; i++)
            {
                Attribute attr = attributes[i];

                if (attr == classAttribute)
                    continue; // we exclude Class Attribute

                double attrGain = getGainOfAttribute(attr.index);

                /*
                 * --> Old way if(localGain > temp) { temp = localGain; max = attr;
                 * }
                 */

                if (entropy.isBetterAttributeCandidate(attrGain, bestSoFar)) // New
                                                                             // way
                {
                    bestSoFar = attrGain;
                    max = attr;
                }
            }

            return max;
        }

        /*
         * How many class values does this dataset has? (actual in the dataset, not
         * in the attribute definition)
         */
        private int classCategoryVariants()
        {
            HashSet<string> set = new HashSet<string>();

            foreach (Line line in data)
            {
                set.Add(line[classAttribute.index]);
            }

            return set.Count;
        }

        /*
         * Remove lines from current dataset from the n-th part (param: part)
         * Example-> Dataset size: 1000 - part: 1 - of: 10, will remove line index 0
         * to 99, and return those 100 rows as a separate dataset
         */
        private DataSource remove(int part, int of)
        {
            if (part <= 0 || of <= 0 || part > of)
                return null; // or throw ArgumentException

            double d_part = part;
            int parts = of; // easily read
            int from = (int)(data.Count * (part - 1) / parts);
            int to = (int)Math.Floor(data.Count * part / (double)parts);

            if (from == to)
                to++;

            DataSource dataForTest = new DataSource();
            cloneExceptData(dataForTest);
            dataForTest.data = new List<Line>();

            for (int j = to - 1; j >= from; j--)
            {
                dataForTest.data.Add(this.data[j]);
                this.data.RemoveAt(j);
            }

            this.size = this.data.Count;
            dataForTest.size = dataForTest.data.Count;

            return dataForTest;

            /*
             * After this: 1) The master dataset will be processed, resulting a tree
             * 2) The return value (dataForTest), will be fed as test argument to
             * that tree
             */
        }

        private DataSource remove(double percentage)
        {
            if (percentage <= 0 || percentage >= 100)
                return null; // or throw ArgumentException

            int toTakeOut = (int)Math.Floor(data.Count * percentage / 100);

            DataSource dataForTest = new DataSource();
            cloneExceptData(dataForTest);
            dataForTest.data = new List<Line>();

            while (toTakeOut > 0)
            {
                dataForTest.data.Add(this.data[0]);
                this.data.RemoveAt(0);
                toTakeOut--;
            }

            this.size = this.data.Count;
            dataForTest.size = dataForTest.data.Count;

            return dataForTest;
        }

        void removeMisclassified()
        {
            size = data.Count;
            for (int i = size - 1; i > 0; i--)
            {
                Line line = data[i];
                if (line.isMisclassfied)
                    data.RemoveAt(i);
            }
            size = data.Count;
        }

        void removeUnclassified()
        {
            size = data.Count;
            for (int i = size - 1; i > 0; i--)
            {
                Line line = data[i];
                if (line.isUnclassified)
                    data.RemoveAt(i);
            }
            size = data.Count;
        }

        StringBuilder output; // large output in GUI textbox

        private void print(string str)
        {
            if (printToDefaultStreamOnTest)
                System.Console.WriteLine(str);

            if (printToStringBuilderOnTest)
            {
                output.Append(str);
                output.Append("\n");
            }
        }

        private TestResult test(DataSource masterData, DataSource testData)
        {
            return test(masterData, testData, false);
        }

        private TestResult test(DataSource masterData, DataSource testData,
                bool printMisclassifiedAndUnclassified)
        {
            string emptyString = "";
            return test(masterData, testData, printMisclassifiedAndUnclassified,
                    emptyString);
        }

        public TestResult test(DataSource masterData, DataSource testData,
                bool printMisclassifiedAndUnclassified, string printArgument)
        {
            TestResult result = new TestResult();
            int unclassified = 0;
            int misclassified = 0;
            int classified = 0;
            int recordsTested = testData.data.Count;

            // Learn from "masterData" dataset, and generate a tree
            Node tree = new Node();
            masterData.process(tree);

            // Fed all records from "testData" dataset to that tree for
            // classification
            for (int j = 0; j < recordsTested; j++)
            {
                bool? hasil = tree.test(testData, j);
                if (hasil == null)
                    unclassified++;
                else if (hasil.Equals(false))
                    misclassified++;
                else if (hasil.Equals(true))
                    classified++;
            }

            if (printMisclassifiedAndUnclassified)
            {
                if (misclassified > 0 || unclassified > 0)
                {
                    if (!(printArgument != null))
                        throw new Exception("printArgument can't be null");

                    string print = printArgument + "Size " + recordsTested + ", "
                            + "Misclassified = " + misclassified + ", "
                            + "Unclassified = " + unclassified;

                    this.print(print);
                }
                else
                {
                    // if everything goes fine, then there's nothing to print out
                }
            }

            result.unclassified = unclassified;
            result.misclassified = misclassified;
            result.classified = classified;
            result.recordsTested = recordsTested;

            return result;
        }

        public void testUsingTrainingSet()
        {
            output = new StringBuilder();

            printHeaderOfTest("evaluate on training data");

            DataSource masterData = null;
            DataSource testData = null;
            int total_unclassified = 0;
            int total_misclassified = 0;
            int total_classified = 0;
            int total_test_records = 0;
            int total_learning_records = 0;

            try
            {
                masterData = (DataSource)this.clone();
                testData = (DataSource)this.clone();

                TestResult result = test(masterData, testData, true);

                total_unclassified += result.unclassified;
                total_misclassified += result.misclassified;
                total_classified += result.classified;
                total_test_records += result.recordsTested;
                total_learning_records += masterData.data.Count;
            }
            catch (Exception err)
            {
                System.Console.WriteLine(err.ToString());
            }

            printFooterOfTest(total_classified, total_misclassified,
                    total_unclassified, total_test_records, total_learning_records);
        }

        public void testUsingCrossValidation(int folds)
        {
            output = new StringBuilder();

            printHeaderOfTest(folds + "-fold cross-validation");

            int parts = folds;
            if (parts < 1)
                parts = 10; // default folds

            DataSource copy = this.doClone();

            if (scrambleBeforeTesting)
                Collections.Shuffle(copy.data); // Not shuffling the original
                                                // dataset

            DataSource masterData = null;
            DataSource testData = null;
            int total_unclassified = 0;
            int total_misclassified = 0;
            int total_classified = 0;
            int total_test_records = 0;
            int total_learning_records = 0;

            for (int i = 1; i <= parts; i++)
            {
                try
                {
                    masterData = (DataSource)copy.clone();
                    testData = masterData.remove(i, parts);

                    string loopArgument = "Loop # " + i + ", ";

                    TestResult result = test(masterData, testData, true,
                            loopArgument);

                    total_unclassified += result.unclassified;
                    total_misclassified += result.misclassified;
                    total_classified += result.classified;
                    total_test_records += result.recordsTested;
                    total_learning_records += masterData.data.Count;
                }
                catch (Exception err)
                {
                    System.Console.WriteLine(err.ToString());
                }
            }

            printFooterOfTest(total_classified, total_misclassified,
                    total_unclassified, total_test_records, total_learning_records);
        }

        public void testUsingPercentageSplit(double percentage)
        {
            output = new StringBuilder();

            printHeaderOfTest("split " + percentage + "% train, remainder test");

            DataSource copy = this.doClone();

            if (scrambleBeforeTesting)
                Collections.Shuffle(copy.data); // Not shuffling the original
                                                // dataset

            DataSource masterData = null;
            DataSource testData = null;
            int total_unclassified = 0;
            int total_misclassified = 0;
            int total_classified = 0;
            int total_test_records = 0;
            int total_learning_records = 0;

            try
            {
                masterData = (DataSource)copy.clone();
                testData = masterData.remove(100 - percentage);

                TestResult result = test(masterData, testData, true);

                total_unclassified += result.unclassified;
                total_misclassified += result.misclassified;
                total_classified += result.classified;
                total_test_records += result.recordsTested;
                total_learning_records += masterData.data.Count;
            }
            catch (Exception err)
            {
                System.Console.WriteLine(err.ToString());
            }

            printFooterOfTest(total_classified, total_misclassified,
                    total_unclassified, total_test_records, total_learning_records);
        }

        public void testUsingDifferentFile(DataSource testData)
        {
            output = new StringBuilder();

            printHeaderOfTest("evaluate on training data");

            DataSource masterData = null;
            //DataSource testData = null;
            int total_unclassified = 0;
            int total_misclassified = 0;
            int total_classified = 0;
            int total_test_records = 0;
            int total_learning_records = 0;

            try
            {
                masterData = (DataSource)this.clone();
                //testData = (DataSource)this.clone();

                TestResult result = test(masterData, testData, true);

                total_unclassified += result.unclassified;
                total_misclassified += result.misclassified;
                total_classified += result.classified;
                total_test_records += result.recordsTested;
                total_learning_records += masterData.data.Count;
            }
            catch (Exception err)
            {
                System.Console.WriteLine(err.ToString());
            }

            printFooterOfTest(total_classified, total_misclassified,
                    total_unclassified, total_test_records, total_learning_records);
        }

        private void printHeaderOfTest(string testMode)
        {
            print("=== Run information ===");
            print("");
            print("Scheme:\tId3");
            print("Relation:\t" + relation);
            print("Attributes:\t" + attributes.Count);
            foreach (Attribute attr in attributes)
            {
                print("           \t" + attr.name);
            }
            print("Entropy:\t" + entropy.ToString());
            print("Test mode:\t" + testMode);
            print("\n\n");

            // Process a copy of current dataset, and get the whole path
            DataSource copy = this.doClone();
            Node tree = new Node();
            copy.process(tree);

            // Print each path line by line
            List<string> pathList = new List<string>();
            tree.printPath(pathList);
            object[] pathArray = pathList.ToArray();
            Array.Sort(pathArray); // Sort is applied to improve readability
            foreach (object line in pathArray)
            {
                print(line.ToString());
            }
            print("Leaf count : " + tree.leafCount());
            print("\n\n");
        }

        private void printFooterOfTest(int total_classified,
                int total_misclassified, int total_unclassified,
                int total_test_records, int total_learning_records)
        {
            string print = null;

            this.print("\n=== Summary ===\n");
            print = "Total classified = \t" + total_classified + " ("
                    + percent(total_classified, total_test_records) + ")";
            this.print(print);
            print = "Total misclassified = \t" + total_misclassified + " ("
                    + percent(total_misclassified, total_test_records) + ")";
            this.print(print);
            print = "Total unclassified = \t" + total_unclassified + " ("
                    + percent(total_unclassified, total_test_records) + ")";
            this.print(print);
            print = "Total test records = \t" + total_test_records;
            this.print(print);
            print = "Total learning records = \t" + total_learning_records;
            this.print(print);
            print = "Total dataset records = \t" + data.Count;
            this.print(print);
        }

        private static string percent(double quantity, double total)
        {
            //NumberFormat nbFmt = NumberFormat.getPercentInstance();
            //nbFmt.setMaximumFractionDigits(4);
            //return nbFmt.format(quantity / total);

            Func<long, int, string> DecimalToArbitrarySystem = (long decimalNumber, int radix) =>
              {
                  const int BitsInLong = 64;
                  const string Digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

                  if (radix < 2 || radix > Digits.Length)
                      throw new ArgumentException("The radix must be >= 2 and <= " + Digits.Length.ToString());

                  if (decimalNumber == 0)
                      return "0";

                  int index = BitsInLong - 1;
                  long currentNumber = Math.Abs(decimalNumber);
                  char[] charArray = new char[BitsInLong];

                  while (currentNumber != 0)
                  {
                      int remainder = (int)(currentNumber % radix);
                      charArray[index--] = Digits[remainder];
                      currentNumber = currentNumber / radix;
                  }

                  string result = new String(charArray, index + 1, BitsInLong - index - 1);
                  if (decimalNumber < 0)
                  {
                      result = "-" + result;
                  }

                  return result;
              };

            //return DecimalToArbitrarySystem((long)quantity / (long)total, 4) + '%';
            return (100 * quantity / total) + "%";
        }
    }
}