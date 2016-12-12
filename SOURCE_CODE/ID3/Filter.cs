namespace App2.ID3
{
    public class Filter
    {
        public bool do_DeleteAdjacentDuplicateRows = true;
        public bool do_IsKeyColumn = true;
        public bool do_IsSingleValuedAttribute = true;
        public bool do_HasMajorityValue = true;
        public double majorityThreshold = 0.95; // 95%

        public Filter() : this(true, true, true, false, 0)
        {

        }

        public Filter(bool do_DeleteAdjacentDuplicateRows,
                bool do_IsKeyColumn, bool do_IsSingleValuedAttribute,
                bool do_HasMajorityValue, double majorityValuePercentage)
        {
            this.do_DeleteAdjacentDuplicateRows = do_DeleteAdjacentDuplicateRows;
            this.do_IsKeyColumn = do_IsKeyColumn;
            this.do_IsSingleValuedAttribute = do_IsSingleValuedAttribute;
            this.do_HasMajorityValue = do_HasMajorityValue;
            setMajorityValue(majorityValuePercentage);
        }

        public void setMajorityValue(double percentage)
        {
            majorityThreshold = percentage / 100;
        }
    }
}