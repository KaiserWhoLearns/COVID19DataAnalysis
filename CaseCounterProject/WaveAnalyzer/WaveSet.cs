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
        private readonly int waveSmoothing = 21;

        public double ZeroLevel { get { return zeroLevel; } }
        public int ZeroSmoothing { get { return zeroSmoothing; } }
        public int WaveSmoothing { get { return waveSmoothing; } }
        public WaveParameters() {

        }
    }
    public class WaveSet {
        public TimeSeries TimeSeries { get; }
        public List<Wave> Waves { get { return waves; } }

        private List<Wave> waves;

        public WaveSet(TimeSeries ts, WaveParameters wp) {
            TimeSeries = ts;
            waves = new();           
            
            List<Wave> wList = RemoveZeroLevels(ts, wp);
            foreach(Wave wave in wList) {
                waves.AddRange(MakeWaves(wave, ts, wp));
            }

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

        public static List<Wave> MakeWaves(Wave wave, TimeSeries ts, WaveParameters wp) {
            List<Wave> wList = new();
            TimeSeries smoothed = ts.BlockSmooth(wp.WaveSmoothing);
            double[] smData = smoothed.GetData();

            int waveStart = wave.Start;
            bool goingUp = true;

            for (int i = wave.Start + 1; i <= wave.End; i++) {
                if (goingUp) {
                    if (smData[i] < smData[i - 1]) {
                        goingUp = false;
                    }
                } else {
                    if (smData[i] > smData[i - 1]) {
                        wList.Add(new(ts, waveStart, i - 1));
                        waveStart = i - 1;
                        goingUp = true;
                    }
                }
            }
            wList.Add(new(ts, waveStart, wave.End));
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
