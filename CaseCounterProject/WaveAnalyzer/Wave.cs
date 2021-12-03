using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataSeries;

namespace WaveAnalyzer {
    public class Wave {

        private readonly int derivSpan = 7;

        public TimeSeries timeSeries { get; }
        public int Start { get; }
        public int End { get; }

        private double caseCount;
        public double CaseCount { get { return caseCount; } }

        private double deaths;
        public double Deaths { get { return deaths; } }

        private double normalizedCount;
        public double NormalizedCount { get { return normalizedCount; } }

        private double weight;
        public double Weight { get { return weight; } }

        private double deathWeight;
        public double DeathWeight { get { return deathWeight;  } }

        private double fatalityRate;
        public double FatalityRate { get { return fatalityRate; } }

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

        private double maxDeriv;
        public double MaxDeriv { get { return maxDeriv; } }

        private int maxDerivPos;
        public double MaxDerivPos { get { return maxDerivPos; } }

        private double minDeriv;
        public double MinDeriv { get { return minDeriv; } }

        private int minDerivPos;
        public double MinDerivPos { get { return minDerivPos; } }

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

            normalizedCount = count / timeSeries.Population;

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

  
            (int pos, double deriv) = timeSeries.MaximumDerivative(derivSpan, Start, End);
            maxDerivPos = pos;
            maxDeriv = (normalization > 0) ? deriv / normalization : 0.0;

            (pos, deriv) = timeSeries.MinimumDerivative(derivSpan, Start, End);
            minDerivPos = pos;
            minDeriv = (normalization > 0) ? deriv / normalization : 0.0;
        }

        public string ToLongString() {
            StringBuilder sb = new();
            sb.Append($"<{Start}, {End}>\r\n");
            sb.Append($"  Cases: {CaseCount:F2} ");
            sb.Append($"  Normalized Cases: {NormalizedCount:F2}\r\n");
            sb.Append($"  Percentage: {100 * Weight:F2}%\r\n");
            sb.Append($"  Deaths: {Deaths:F2} Pecentatge {100 * DeathWeight:F2}%\r\n");
            sb.Append($"  Fatality rate {FatalityRate:F4} \r\n");
            sb.Append($"  Maximum: {Maximum}");
            sb.Append($"  MaxValue: {MaxValue:F2}\r\n");
            sb.Append($"  Median: {Median}");
            sb.Append($"  Mean: {Mean:F2}");
            sb.Append($"  Sigma: {Sigma:F2}\r\n");
            sb.Append($"  Max Derivative {MaxDeriv:F2} at {MaxDerivPos}\r\n");
            sb.Append($"  Min Derivative {MinDeriv:F2} at {MinDerivPos}\r\n");
            return sb.ToString();
        }

        public void AddDeath(TimeSeries ts) {
            double deathCount = 0.0;
            double[] ddata = ts.GetData();

            for (int i = Start; i <= End; i++) {
                deathCount += ddata[i];
            }
            deaths = deathCount;

            double totalDeaths = ts.CaseCount();
            deathWeight = (totalDeaths > 0) ? deathCount / totalDeaths : 0.0;

            fatalityRate = (caseCount > 0) ? deathCount / caseCount : 0.0;
        }
    }
}
