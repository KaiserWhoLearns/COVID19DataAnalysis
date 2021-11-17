using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSeries {
    public class PeakSetCollection {

        private Dictionary<string, PeakSet> peakSets;
        public PeakSetCollection() {
            peakSets = new Dictionary<string, PeakSet>();
        }

        public void AddPeakSet(PeakSet ps) {
            peakSets.Add(ps.Key, ps);
        }

        private int MaximumPeaks() {
            int p = 0;

            foreach (PeakSet ps in peakSets.Values) {
                p = Math.Max(p, ps.Count);
            }
            return p;
        }


        public void WriteToFile(string path) {
            using (StreamWriter outputFile = new(path)) {
;               int mPeaks = MaximumPeaks();
                outputFile.WriteLine(HeaderString(mPeaks));
                 foreach (PeakSet ps in peakSets.Values) {
                    outputFile.WriteLine(ps.ToRowString(mPeaks));
                }   
            }
        }

        private static string HeaderString(int maxPeaks) {
            StringBuilder sb = new();
            _ = sb.Append("DataType,Admin2,Admin1,Admin0,Population,CaseCount");

            for (int i = 1; i <= 2*maxPeaks + 1; i++) {
                _ = sb.Append(",X" + i + ",Y" + i);
            }
            return sb.ToString();

        }

    }
}
