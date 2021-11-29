using System;
using DataSeries;


namespace WaveAnalyzer {
    public class WaveParameters {
        private readonly double zeroLevel = 0.5;

        public double ZeroLevel { get; }

        public WaveParameters() {

        }
    }
    public class WaveSet {
        public TimeSeries TimeSeries { get; }
        public WaveSet(TimeSeries ts, WaveParameters wp) {
            TimeSeries = ts;
        }

        public string SummaryString() {
            return TimeSeries.ShortName;
        }
    }
}
