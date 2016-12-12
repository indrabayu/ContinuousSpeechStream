using System;

namespace App2.ID3
{
    public class ShannonEntropy : Entropy
    {
        public ShannonEntropy()
        {
            name = "Shannon";
            requiresEntropyOfSet = true;
            defaultFor_TemporaryEntropyBound = 0;
        }

        public override double m_count(int num, int count)
        {
            if (num == 0)
                return 0;

            double left = (double)num / (double)count;
            double right = Math.Log10((double)left) / Math.Log10((double)2);
            double result = -1 * left * right;
            return result;
        }

        public override double get()
        {
            double result = 0;

            foreach (double d in generalEntropies)
            {
                result += d;
            }

            generalEntropies.Clear();

            return result;
        }

        public override double getGainOfAttribute()
        {
            double result = attributeEntropies[0]; // take the EntOfSet out
            attributeEntropies.RemoveAt(0); // take the EntOfSet out

            for (int i = 0; i < attributeEntropies.Count; i++)
            {
                double d = attributeEntropies[i];
                result -= d;
            }

            attributeEntropies.Clear();

            return result;
        }

        public override bool isBetterAttributeCandidate(double newValue,
                double currentBestValue)
        {
            return newValue > currentBestValue;
        }

        public string toString()
        {
            return name;
        }
    }

}