using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSeries {
    public class PeakSetCollection {
        public PeakSetCollection() {

        }


        public void WriteToFile(string path) {
            using (StreamWriter outputFile = new(path)) {
  /*              outputFile.WriteLine(HeaderString(LastDay()));

                foreach (TimeSeries ts in series.Values) {
                    outputFile.WriteLine(ts.ToRowString());
                }  */
            }
        }

    }
}
