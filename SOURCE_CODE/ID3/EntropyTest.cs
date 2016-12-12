using System;

namespace App2.ID3
{
    public class EntropyTest
    {
        /*
         * Notes for DataSet ==================
         * 
         * Vague(nonsense) rows occurs when: 1) [splice] deleteAdjacentDuplicateRows
         * = true; filterBeforeProcessing = true; (line 61 & 1022 should be deleted)
         */

        public static void Execute()
        {
            //var documentsPath = "sdcard/DataSetARFF/";
            var documentsPath = @"C:\Users\indra\Desktop\";
            //var filename = "weather.arff";
            var filename = "contact-lenses.arff";
            filename = System.IO.Path.Combine(documentsPath, filename);
            string attrSplitString = ", ";

            string classAttrName = null; //null; // NULL means using the last one
            bool replaceQuestionMark = true;

            bool deleteAdjacentDuplicate = true;

            bool isKey = true;
            bool isSingleValued = true;
            bool hasMajority = false;
            bool removeNonsense = true;
            Filter filter = // null;
                            // new Filter();
            new Filter(deleteAdjacentDuplicate, isKey, isSingleValued, hasMajority,
                    0);

            DataSource data = DataSource.getDataSource(filename, classAttrName,
                    attrSplitString, replaceQuestionMark, filter);

            if (removeNonsense)
                if (data.hasEqualValuesButDifferentClass()) // better done ONLY when
                                                            // deleteAdjacentDuplicateRows
                                                            // is set to true
                {
                    string warning = "This dataset contains adjacent duplicates with different class values!";
                    System.Console.WriteLine(warning);
                    data.deleteVagueRows();
                    //throw new System.Exception(warning);
                }

            // ArrayList<DataSource> parts = data.split(2);

            // data.printToStringBuilderOnTest = false;
            // data.testUsingTrainingSet();
            data.scrambleBeforeTesting = true;
            //data.testUsingCrossValidation(10);
            data.printToStringBuilderOnTest = false;
            data.testUsingPercentageSplit(66);
        }

        public static void Execute2()
        {
            //var documentsPath = "sdcard/DataSetARFF/";
            var documentsPath = @"C:\Users\indra\Desktop\";
            var learningFile = "makan.arff";
            learningFile = System.IO.Path.Combine(documentsPath, learningFile);
            //var testingFile = "makan.test1.arff";
            //var testingFile = "makan.test2.arff";
            var testingFile = "makan.test3.arff";
            testingFile = System.IO.Path.Combine(documentsPath, testingFile);
            string attrSplitString = ", ";

            string classAttrName = null; // use the last one
            bool replaceQuestionMark = true;

            bool deleteAdjacentDuplicate = true;

            bool isKey = true;
            bool isSingleValued = true;
            bool hasMajority = false;
            bool removeNonsense = true;
            Filter filter = // null;
                            // new Filter();
            new Filter(deleteAdjacentDuplicate, isKey, isSingleValued, hasMajority,
                    0);

            DataSource learningData = DataSource.getDataSource(learningFile, classAttrName,
                    attrSplitString, replaceQuestionMark, filter);

            DataSource testingData = DataSource.getDataSource(testingFile, classAttrName,
                    attrSplitString, replaceQuestionMark, filter);

            Action<DataSource> _ = (DataSource ds) =>
            {
                if (removeNonsense)
                    if (ds.hasEqualValuesButDifferentClass()) // better done ONLY when
                                                              // deleteAdjacentDuplicateRows
                                                              // is set to true
                        {
                        string warning = "This dataset contains adjacent duplicates with different class values!";
                        System.Console.WriteLine(warning);
                        ds.deleteVagueRows();
                        throw new System.Exception(warning);
                    }
            };

            _(learningData);
            _(testingData);

            learningData.testUsingDifferentFile(testingData);
        }
    }
}