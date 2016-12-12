using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App2
{
    public class TextMatching
    {
        /*
         * Explanation for POSSIBILITIES:
         * This is actually a dictionary where the Key is Tuple<Entry.keys, Entry.next>
         * ,while the the Value is composed of the other fields in the Entry class 
         * (occurences, match, mismatch).
         */
        public List<Entry> possibilities = new List<Entry>();

        public List<string> stream = new List<string>();

        const int MaxNumberOfWordsInEntries_ConsideredNotTooLong = int.MaxValue;

        public /*List<Entry>*/ Entry NextToLearn(string query)
        {
            var results = new List<Entry>();

            if (possibilities.Count == 0)
            {
                var empty = new Entry();
                empty.next = query;
                possibilities.Add(empty);
                stream.Add(query);
                return null;
            }

            foreach (Entry entry in possibilities.Where(_ => _.keys.Count > 0).OrderBy(_ => _.keys.Count))
            {
                if (entry.next.Equals(query))
                {
                    entry.occurences++;
                    results.Add(entry);
                }
            }

            foreach (Entry entry in results)
            {
                if (entry.keys.SequenceEqual(stream.Skip(stream.Count - entry.keys.Count)))
                {
                    entry.match++;
                }
                else
                {
                    entry.mismatch++;
                }
            }

            for (int i = 0; i < stream.Count; i++)
            {
                if ((stream.Count - i) >= MaxNumberOfWordsInEntries_ConsideredNotTooLong)
                {
                    //filter for entries which can be too long,
                    //which can mean irrelevant
                    continue;
                }

                var newEntry = new Entry();
                newEntry.keys = stream.Skip(i).ToList();
                newEntry.next = query;
                if (possibilities
                        .Where(_ => _.keys.SequenceEqual(newEntry.keys) && _.next.Equals(query))
                        .Count() == 0) //make sure such combination doesn't exist already
                {
                    possibilities.Add(newEntry);
                }
            }

            stream.Add(query);

            results.Sort((left, right) =>
            {
                if (left.match != right.match)
                {
                    return right.match.CompareTo(left.match);
                }
                else if (left.occurences != right.occurences)
                {
                    return right.occurences.CompareTo(left.occurences);
                }
                else if (left.mismatch != right.mismatch)
                {
                    return left.mismatch.CompareTo(right.mismatch);
                }
                else
                {
                    return left.keys.Count.CompareTo(right.keys.Count);
                }
            });

            if (results.Count == 0)
                return null;

            return results[0];// results.Take(1).ToList();
        }

        public /*List<Entry>*/ Entry PredictNext()
        {
            if (stream.Count == 0)
                return null;

            var latest = stream.Last();
            List<Entry> suggestions = new List<Entry>();

            try
            {
                foreach (var entry in possibilities)
                {
                    if (entry.keys.Count != 0 && entry.keys.Last().Equals(latest))
                        suggestions.Add(entry);
                }
            }
            catch (Exception)
            {
                return null;
            }

            if (suggestions.Count == 0)
                return null;

            suggestions.Sort((left, right) =>
            {
                if (left.match != right.match)
                {
                    return right.match.CompareTo(left.match);
                }
                else if (left.occurences != right.occurences)
                {
                    return right.occurences.CompareTo(left.occurences);
                }
                else if (left.mismatch != right.mismatch)
                {
                    return left.mismatch.CompareTo(right.mismatch);
                }
                else
                {
                    return left.keys.Count.CompareTo(right.keys.Count);
                }
            });

            return suggestions[0]; // suggestions.Take(1).ToList();
        }
    }

    public class Entry
    {
        public List<string> keys = new List<string>();
        public string next;
        public uint occurences = 0;
        public uint match = 0;
        public uint mismatch = 0;
    }
}
