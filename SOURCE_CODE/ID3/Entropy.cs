using System.Collections.Generic;

namespace App2.ID3
{
    public abstract class Entropy
    {
        public string name;

        public abstract double m_count(int num, int count);

        public void countAndSave(int num, int count)
        {
            generalEntropies.Add(m_count(num, count));
        }

        public abstract double get();

        public List<double> generalEntropies = new List<double>();
        public List<double> attributeEntropies = new List<double>();

        public abstract double getGainOfAttribute();

        // if True, should be appended at index 1 of attributeEntropies
        public bool requiresEntropyOfSet = false;

        public double defaultFor_TemporaryEntropyBound;

        public abstract bool isBetterAttributeCandidate(double newValue,
                double currentBestValue);
    }

    enum Method
    {
        Shannon, HavrdaCharvat
    }
}