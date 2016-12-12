using System;

namespace App2.ID3
{
    public class HavrdaCharvatEntropy : Entropy
    {
        public double alpha;

        public const double Alpha_25Percent = 0.25, Alpha_50Percent = 0.5,
                Alpha_75Percent = 0.75;

        public HavrdaCharvatEntropy(double alpha)
        {
            name = "Havrda & Charvat";

            if (alpha < 0 || alpha > 1)
            {
                this.alpha = Alpha_25Percent; // default
            }
            else
            {
                this.alpha = alpha;
            }

            defaultFor_TemporaryEntropyBound = 1;
        }

        public override double m_count(int num, int count)
        {
            if (num == 0)
                return 0;

            double result = Math.Pow((double)num / (double)count, alpha);

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

            return Math.Abs(result - 1) / (1 - alpha);
        }

        public override double getGainOfAttribute()
        {
            double result = 0;

            foreach (double d in attributeEntropies)
            {
                result += d;
            }

            attributeEntropies.Clear();

            return result;
        }

        public override bool isBetterAttributeCandidate(double newValue,
                double currentBestValue)
        {
            return newValue < currentBestValue;
        }

        public string toString()
        {
            return name + " with alpha = " + alpha;
        }
    }
}