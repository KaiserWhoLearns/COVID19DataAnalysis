using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataSeries;

namespace WaveAnalyzer {
    public class Wave {
        public TimeSeries timeSeries { get; }
        public int Start { get; }
        public int End { get; }

        private double caseCount;
        public double CaseCount { get { return caseCount; } }

        private double weight;
        public double Weight { get { return weight; } }

        private int median;
        public int Median {  get { return median; } }

        private double mean;
        public double Mean { get { return mean; } }

        private double sigma;
        public double Sigma { get { return sigma; } }

        private int maximum;
        public int Maximum { get { return maximum; } }

        private double maxValue;
        public double MaxValue { get { return maxValue; } }



        public Wave(TimeSeries ts) {
            timeSeries = ts;
        }

        private double[] data;

        public Wave(TimeSeries ts, int start, int end) {
            timeSeries = ts;
            Start = start;
            End = end;
            data = timeSeries.GetData();

            Initialize();
        }

        public void Initialize() {

            double normalization = timeSeries.Population / 100000.0;

            double count = 0.0;
            for (int i = Start; i <= End; i++) {
                count += data[i];
            }
            caseCount = count;

            weight = caseCount / timeSeries.CaseCount();
            
            int j = Start;
            count = data[j]; 

            while (count < caseCount / 2) {
                j++;
                count += data[j];
            }
            median = j;

            double maxSoFar = data[Start];
            int maxPos = Start;
            for (int i = Start + 1; i <= End; i++) {
                if (data[i] > maxSoFar) {
                    maxSoFar = data[i];
                    maxPos = i;
                }
            }
            maximum = maxPos;
            maxValue = (normalization > 0) ? maxSoFar / normalization : 0.0;

            double sum = 0.0;
            for (int i = Start; i <= End; i++) {
                sum += i * data[i];
            }
            mean = sum / caseCount;

            sum = 0.0;
            for (int i = Start; i <= End; i++) {
                double val = i - mean;
                sum += val * val * data[i];
            }
            sigma = Math.Sqrt(sum / caseCount);

        }

        public string ToLongString() {
            StringBuilder sb = new();
            sb.Append($"<{Start}, {End}>\r\n");
            sb.Append($"  Cases: {CaseCount:F2}\r\n");
            sb.Append($"  Weight: {Weight:F2}\r\n");
            sb.Append($"  Maximum: {Maximum}\r\n");
            sb.Append($"  MaxValue: {MaxValue:F2}\r\n");
            sb.Append($"  Median: {Median}\r\n");
            sb.Append($"  Mean: {Mean:F2}\r\n");
            sb.Append($"  Sigma: {Sigma:F2}\r\n");
            return sb.ToString();
        }
    }
}
