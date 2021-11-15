using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSeries {

    public struct Datum {
        public Datum(int x, double y) {
            X = x;
            Y = y;
        }

        public int X { get; }
        public double Y { get; }
        public override string ToString() {
            return $"({X}, {Y:F2})";
        }
    }

    public class Peak {

        public Peak(Datum lMin, Datum max, Datum rMin) {
            LMin = lMin;
            Max = max;
            RMin = rMin;
        }

        public Peak(int i1, double y1, int i2, double y2, int i3, double y3) {
            LMin = new Datum(i1, y1);
            Max = new Datum(i2, y2);
            RMin = new Datum(i3, y3);
        }
        public Datum LMin { get; }
        public Datum RMin { get; }
        public Datum Max { get; }

        public override string ToString() => $"<{LMin}, {Max}, {RMin}>";


        public static double ValleySize(Peak p1, Peak p2) {
            double x1 = p1.RMin.X - p1.Max.X;
            double y1 = p1.Max.Y - p1.RMin.Y;
            double x2 = p2.Max.X - p2.LMin.X;
            double y2 = p2.Max.Y - p2.LMin.Y;

            if (y1 < y2) {
                double z2 = y1 * x2 / y2;
                return (z2 + x1) * y1;
            } else {
                double z1 = y2 * x1 / y1;
                return (z1 + x2) * y2;
            }


        }

        public static Peak Merge(Peak p1, Peak p2) {
            return (p1.Max.Y > p2.Max.Y) ? new Peak(p1.LMin, p1.Max, p2.RMin) : new Peak(p1.LMin, p2.Max, p2.RMin);
        }
    }
    public class PeakSet : IEnumerable<Peak> {
        public string Key { get; }
        public string Admin0 { get; }       // Country
        public string Admin1 { get; }       // State or Province
        public string Admin2 { get; }       // District or County
        public DataType DataType { get; }


        public int Count => peaks.Count;

        // Popluation comes from JHU data by Confirmed/Incidence * 100,000.  We will use the value from the latest date.
        public long Population { get; set; }
        private List<Peak> peaks;

        public PeakSet(DataType dataType, string admin0, string admin1, string admin2, long population = -1) {
            peaks = new();

            DataType = dataType;
            Admin0 = admin0;
            Admin1 = admin1;
            Admin2 = admin2;
            Population = population;

            Key = TimeSeries.BuildKey(dataType, admin0, admin1, admin2);

        }

        /*

        public PeakSet() {
            peaks = new();
        }

        public PeakSet(List<Peak> peakList) {
            peaks = new();
            foreach (Peak peak in peakList) {
                peaks.Add(new Peak(peak.LMin, peak.Max, peak.RMin));
            }
        }

        */

        public PeakSet(TimeSeries ts) {
            DataType = ts.DataType;
            Admin0 = ts.Admin0;
            Admin1 = ts.Admin1;
            Admin2 = ts.Admin2;
            Population = ts.Population;
            Key = ts.Key;

            peaks = FindPeaks(ts.GetData(), ts.LastDay + 1);
        }

        public static List<Peak> FindPeaks(double[] data, int len) {

            List<Peak> peaks = new();
            int index = 0;
            char state = 'R';
            int min = 0;
            int max = 0;

            while (index < len - 1) {
                if (state == 'R') {
                    if (data[index + 1] < data[index]) {
                        state = 'F';
                        max = index;
                    }
                } else {
                    if (data[index + 1] > data[index]) {
                        state = 'R';
                        Peak p = new(min, data[min], max, data[max], index, data[index]);
                        peaks.Add(p);
                        min = index;
                    }
                }
                index++;
            }

            if (state == 'R') {
                Peak p = new(min, data[min], index, data[index], index, data[index]);
                peaks.Add(p);
            } else {
                Peak p = new(min, data[min], max, data[max], index, data[index]);
                peaks.Add(p);
            }
            return peaks;
        }


        public IEnumerator<Peak> GetEnumerator() {
            return peaks.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return peaks.GetEnumerator();
        }   

        public void Add(Peak peak) {
            peaks.Add(peak);
        }

        public (double[], double[]) BuildDataSeries() {
            int len = 2 * peaks.Count + 1;
            double[] xData = new double[len];
            double[] yData = new double[len];

            for (int i = 0; i < peaks.Count; i++) {
                Peak p = peaks[i];
                xData[2 * i] = p.LMin.X;
                yData[2 * i] = p.LMin.Y;
                xData[2 * i + 1] = p.Max.X;
                yData[2 * i + 1] = p.Max.Y;
            }
            xData[^1] = peaks[^1].RMin.X;
            yData[^1] = peaks[^1].RMin.Y;

            return (xData, yData);

        }

        public (int, double) FindSmallestValley() {
            if (peaks.Count <= 1)
                return (-1, 0);

            int minSoFar = 0;
            double minValue = Peak.ValleySize(peaks[0], peaks[1]);
            for (int i = 1; i < peaks.Count - 1; i++) {
                double val = Peak.ValleySize(peaks[i], peaks[i + 1]);
                if (val < minValue) {
                    minValue = val;
                    minSoFar = i;
                }
            }

            return (minSoFar, minValue);
        }

        public void RemoveSmallestValley() {

            (int minSoFar, _) = FindSmallestValley();

            if (minSoFar < 0)
                return;

            Peak newPeak = Peak.Merge(peaks[minSoFar], peaks[minSoFar + 1]);
            peaks[minSoFar] = newPeak;
            peaks.RemoveAt(minSoFar + 1);
        }

        public string ToRowString() {

            StringBuilder sb = new();
            _ = sb.Append(DataType + ",\"" + Admin2 + "\",\"" + Admin1 + "\",\"" + Admin0 + "\"," + Population);

            for (int i = 0; i < peaks.Count; i++) {
                Peak pk = peaks[i];
                _ = sb.Append("," + pk.LMin.X + "," + pk.LMin.Y.ToString("F2"));
                _ = sb.Append("," + pk.Max.X + "," + pk.Max.Y.ToString("F2"));
            }
            _ = sb.Append("," + peaks[^1].RMin.X + "," + peaks[^1].RMin.Y.ToString("F2"));

            return sb.ToString();
        }

    }

}
