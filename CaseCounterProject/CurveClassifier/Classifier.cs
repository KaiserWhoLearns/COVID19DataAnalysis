using System;
using System.Collections.Generic;

namespace CurveClassifier {


    public struct Datum {
        public Datum(int x, double y) {
            X = x;
            Y = y;
        }

        public int X { get; }
        public double Y { get; }
        public override string ToString() => $"({X}, {Y:F2})";
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

        public override string ToString() => $"< {LMin}, {Max}, {RMin} >";

        public static double ValleySize(Peak p1, Peak p2) {
            double x1 = p1.RMin.X - p1.Max.X;
            double y1 = p1.Max.Y - p1.RMin.Y;
            double x2 = p2.Max.X - p2.LMin.X;
            double y2 = p2.Max.Y - p2.LMin.Y;

            if (y1 < y2) {
                double z2 = y1 * x2 / y2;
                return (z2 + x1) * y1;
            }
            else {
                double z1 = y2 * x1 / y1;
                return (z1 + x2) * y2;
            }

 
        }

        public static Peak Merge(Peak p1, Peak p2) {
            return (p1.Max.Y > p2.Max.Y) ? new Peak(p1.LMin, p1.Max, p2.RMin) : new Peak(p1.LMin, p2.Max, p2.RMin);
        }
    }

    public class Classifier {
        double[] data;
        public Classifier(double[] dataArray, int len) {
            data = new double[len];
            for (int i = 0; i < len; i++)
                data[i] = dataArray[i];
        }

        public List<Peak> FindPeaks() {

            List<Peak> peaks = new();
            int index = 0;
            char state = 'R';
            int min = 0;
            int max = 0;

            while (index < data.Length - 1) {
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

        public static void RemoveSmallestValley(List<Peak> peaks) {
            if (peaks.Count <= 1)
                return;

            int minSoFar = 0;
            double minValue = Peak.ValleySize(peaks[0], peaks[1]);
            for (int i = 1; i < peaks.Count - 1; i++) {
                double val = Peak.ValleySize(peaks[i], peaks[i + 1]);
                if (val < minValue) {
                    minValue = val;
                    minSoFar = i;
                }
            }

            Peak newPeak = Peak.Merge(peaks[minSoFar], peaks[minSoFar + 1]);
            peaks[minSoFar] = newPeak;
            peaks.RemoveAt(minSoFar + 1);
        }
    }

}


