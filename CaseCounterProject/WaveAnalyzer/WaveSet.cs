using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using DataSeries;
using Utilities;


namespace WaveAnalyzer {
    public class WaveParameters {
        private readonly double zeroLevel = 0.5;
        private readonly int zeroSmoothing = 14;

        public double ZeroLevel { get { return zeroLevel; } }
        public int ZeroSmoothing { get { return zeroSmoothing; } }

        public WaveParameters() {

        }
    }
    public class WaveSet {
        public TimeSeries TimeSeries { get; }
        public List<Wave> Waves { get { return waves; } }

        private List<Wave> waves;

        public WaveSet(TimeSeries ts, WaveParameters wp) {
            TimeSeries = ts;
            waves = RemoveZeroLevels(ts, wp);
        }

        private static List<Wave> RemoveZeroLevels(TimeSeries ts, WaveParameters wp) {
            TimeSeries smoothed = ts.BlockSmooth(wp.ZeroSmoothing);
            List<Wave> wList = new();

            if (ts.Population < 0)
                throw new DataException("Unexpected zero population in RemoveZeroLevels");

            double denormalizedZeroLevel = wp.ZeroLevel * ts.Population / 100000.0;
            bool inWave = false;
            int waveStart = -1;
            double[] smData = ts.GetData();

            for(int i = 0; i <= smoothed.LastDay; i++) {
                if (inWave) {
                    if (smData[i] < denormalizedZeroLevel) {
                        wList.Add(new(ts, waveStart, i - 1));
                        inWave = false;
                    }
                } else {
                    if (smData[i] >= denormalizedZeroLevel) {
                        waveStart = i;
                        inWave = true;
                    }

                }
            }
            if (inWave) {
                wList.Add(new(ts, waveStart, smoothed.LastDay));
            }

            return wList;
        }

        public string SummaryString() {
            StringBuilder sb = new();
            sb.Append(TimeSeries.ShortName);
            foreach (Wave w in waves) {
                sb.Append($"({w.Start}, {w.End})");
            }

            return sb.ToString();
        }
    }
}
