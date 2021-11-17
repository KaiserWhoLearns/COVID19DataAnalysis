using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSeries {
    public class SegmentSet {

        private Datum[] points; 
        public SegmentSet(TimeSeries ts, int nSegments) {
            points = OptimalSegmentation(ts, nSegments);
        }

        private Datum[] OptimalSegmentation(TimeSeries ts, int nSegments) {
            int n = ts.LastDay + 1;

            double[,] error = BuildErrorMatrix(ts);
            double[,] opt = new double[nSegments, n];
            int[,] best = new int[nSegments, n];

            for (int i = 1; i < n; i++) {
                opt[0, i] = error[0, i];
            }
            for (int k = 0; k < nSegments; k++) {
                opt[k, 0] = 0.0;
            }
            for (int k = 1; k < nSegments; k++) {
                for (int i = 1; i < n; i++) {
                    double min = opt[k - 1, 0] + error[0, i];
                    int b = 0;
                    for (int j = 1; j < i-1; j++) {
                        double v = opt[k - 1, j] + error[j, i];
                        if (v < min) {
                            min = v;
                            b = j;
                        }
                    }
                    opt[k, i] = min;
                    best[k, i] = b;
                }
            }

            Datum[] result = new Datum[nSegments + 1];
            double[] data = ts.GetData();

            int p = n - 1;
            for (int k = nSegments - 1; k >= 0; k--) {
                result[k+1] = new Datum(p, data[p]);
                p = best[k, p];
            }
            result[0] = new Datum(0, data[0]);

            return result;
        }

        private double [,] BuildErrorMatrix(TimeSeries ts) {
            int n = ts.LastDay +1;
            double[,] err = new double[n - 1, n];

            for (int i = 0; i < n-1; i++) {
                for (int j = i + 1; j < n; j++)
                    err[i, j] = FindError(i, j, ts);
            }

            return err;
        }

        private double FindError(int i, int j, TimeSeries ts) {
            double[] data = ts.GetData();

            double m = (data[j] - data[i]) / (j - i);
            double b = data[i] - m * i;

            double sum = 0.0;
            for (int k = i + 1; k < j; k++) {
                 double v = Math.Abs(m * k + b - data[k]);
                 sum += v * v;
            }


            return sum;
        }
        public (double[], double[]) BuildDataSeries() {
            int len = points.Length;
            double[] xData = new double[len];
            double[] yData = new double[len];

            for (int i = 0; i < points.Length; i++) {
                xData[i] = points[i].X;
                yData[i] = points[i].Y;
            }

            return (xData, yData);

        }
    }
}
