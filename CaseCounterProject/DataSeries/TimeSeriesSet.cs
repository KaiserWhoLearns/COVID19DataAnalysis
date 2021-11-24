using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CsvHelper;
using System.Globalization;
using System.Windows.Controls;
using System.IO;
using Utilities;

namespace DataSeries {
    public class TimeSeriesSet {


        private Dictionary<string, TimeSeries> series;
        public SeriesType SeriesType { get; }

        public TimeSeriesSet(SeriesType seriesType) {
            SeriesType = seriesType;
            series = new Dictionary<string, TimeSeries>();
        }

        public int Count {
            get { return series.Count; }
        }
        public void AddConfirmed(string admin0, string admin1, string admin2, int fips, int? confirmed, int index) {
            AddCase(admin0, admin1, admin2, fips, confirmed, DataType.Confirmed, index);
        }

        public void AddDeaths(string admin0, string admin1, string admin2, int fips, int? deaths, int index) {
            AddCase(admin0, admin1, admin2, fips, deaths, DataType.Deaths, index);
        }

        public void UpdatePopulation(string admin0, string admin1, string admin2, int fips, int? confirmed, double? incidence, int index) {
            double ir = incidence ?? 0.0;                                   // Annoying, clean this up
            int totalCases = confirmed ?? 0;

            if (ir == 0.0)
                return;

            UpdatePopulation(admin0, admin1, admin2, fips, DataType.Confirmed, totalCases, ir, index);
            UpdatePopulation(admin0, admin1, admin2, fips, DataType.Deaths, totalCases, ir, index);

        }

        public void UpdatePopulation(string admin0, string admin1, string admin2, int fips, DataType dataType, int totalCases, double incidence, int index) {
            string key = TimeSeries.BuildKey(dataType, admin0, admin1, admin2);

            // This probably never occurs due assigning counts prior to updating population
            if (!series.ContainsKey(key)) {
                series.Add(key, new TimeSeries(dataType, admin0, admin1, admin2, fips));
            }

            series[key].UpdatePopulation(index, totalCases, incidence);
        }



        public void AddCase(string admin0, string admin1, string admin2, int fips, int? count, DataType dataType, int index) {
            string key = TimeSeries.BuildKey(dataType, admin0, admin1, admin2);

            if (!series.ContainsKey(key)) {
                series.Add(key, new TimeSeries(dataType, admin0, admin1, admin2, fips));
            }

            series[key].SetValue(index, count);

        }

        public void AddSeries(TimeSeries ts) {
            if (series.ContainsKey(ts.Key)) {
                TimeSeries ts1 = series[ts.Key];
                ts1.AddCounts(ts);

            } else {
                series.Add(ts.Key, ts);
            }
        }

        public void AddTimeSeriesSet(TimeSeriesSet tss) {
            foreach (TimeSeries ts in tss.series.Values) {
                AddSeries(ts);
            }
        }


        public TimeSeries GetSeries(string key) {
            return series[key];
        }

        public override string ToString() {
            StringBuilder sb = new();
            foreach (TimeSeries ts in series.Values) {
                _ = sb.Append(ts.ToString() + "\r\n");
            }
            return sb.ToString();
        }



        public void WriteToFile(string path) {
            using (StreamWriter outputFile = new(path)) {
                outputFile.WriteLine(HeaderString(LastDay()));

                foreach (TimeSeries ts in series.Values) {
                    outputFile.WriteLine(ts.ToRowString());
                }
            }
        }

        public int LastDay() {
            int last = -1;

            foreach (TimeSeries ts in series.Values) {
                last = Math.Max(last, ts.LastDay);
            }

            return last;
        }

        private static string HeaderString(int lastDay) {
            StringBuilder sb = new();
            _ = sb.Append("DataType,Admin2,Admin1,Admin0,Fips,Population,CaseCount");

            for (int i = 0; i <= lastDay; i++) {
                _ = sb.Append(",Day " + i);
            }
            return sb.ToString();

        }

        public void LoadCsv(string filePath) {


            using (StreamReader reader = new(filePath))
            using (CsvReader csvReader = new(reader, CultureInfo.InvariantCulture)) {
                List<dynamic> dataRecords = csvReader.GetRecords<dynamic>().ToList();
                foreach (IEnumerable<KeyValuePair<string, object>> row in dataRecords) {
                    AddSeries(new(row));
                }
            }

        }

        public TimeSeriesSet Filter(Predicate<TimeSeries> filter) {
            TimeSeriesSet tss = new(SeriesType);

            foreach (TimeSeries ts in series.Values) {
                if (filter(ts)) {
                    tss.AddSeries(ts);
                }
            }
            return tss;
        }

        public TimeSeriesSet FilterAndSmooth(Predicate<TimeSeries> filter) {
            return Filter(filter).GaussianSmoothing();
        }


        public TimeSeriesSet ToDailyCount() {
            TimeSeriesSet tss = new(SeriesType.Discrete);
            foreach (TimeSeries ts in series.Values) {
                tss.AddSeries(ts.ToDailyCount());
            }

            return tss;
        }

        public static readonly double[] WeekFilter = { 0.14286, 0.14286, 0.14286, 0.14286, 0.14286, 0.14286, 0.14286 };

        public TimeSeriesSet WeeklySmoothing() {
            return SmoothWithFilter(WeekFilter);
        }

        public static readonly double[] GaussianFilter = { 0.002, 0.004, 0.008, 0.016, 0.027, 0.042, 0.06, 0.079, 0.097, 0.109, 0.112, 0.109, 0.097, 0.079, 0.06, 0.042, 0.027, 0.016, 0.008, 0.004, 0.002 };
        public TimeSeriesSet GaussianSmoothing() {
            return SmoothWithFilter(GaussianFilter);
        }

        public TimeSeriesSet SmoothWithFilter(double[] filter) {
            TimeSeriesSet tss = new(SeriesType);
            foreach (TimeSeries ts in series.Values) {
                tss.AddSeries(ts.Smooth(filter));
            }

            return tss;
        }

        public void ToListBox(ListBox listBox) {

            List<string> itemList = series.Keys.ToList();
            itemList.Sort();

            listBox.Items.Clear();
            foreach (string str in itemList) {
                _ = listBox.Items.Add(str);
            }
        }

        public TimeSeriesSet CombineAdmin1() {
            Dictionary<string, TimeSeries> stateSeries = new();

            foreach (TimeSeries ts in series.Values) {
                string seriesKey = ts.Admin0 + "/" + ts.Admin1 + ts.DataType;

                TimeSeries tsState;

                if (stateSeries.ContainsKey(seriesKey)) {
                    tsState = stateSeries[seriesKey];
                } else {
                    tsState = new(ts.DataType, ts.Admin0, ts.Admin1, "", ts.Fips, ts.Population);
                    stateSeries.Add(seriesKey, tsState);
                }
                tsState.AddCounts(ts);
            }

            TimeSeriesSet tss = new(SeriesType);
            foreach (TimeSeries ts in stateSeries.Values) {
                tss.AddSeries(ts);
            }
            return tss;
        }

        public TimeSeriesSet CombineAdmin0() {
            Dictionary<string, TimeSeries> nationalSeries = new();

            foreach (TimeSeries ts in series.Values) {
                string seriesKey = ts.Admin0 + ts.DataType;

                TimeSeries tsNational;

                if (nationalSeries.ContainsKey(seriesKey)) {
                    tsNational = nationalSeries[seriesKey];
                } else {
                    tsNational = new(ts.DataType, ts.Admin0, "", "", ts.Fips, ts.Population);
                    nationalSeries.Add(seriesKey, tsNational);
                }
                tsNational.AddCounts(ts);
            }

            TimeSeriesSet tss = new(SeriesType);
            foreach (TimeSeries ts in nationalSeries.Values) {
                tss.AddSeries(ts);
            }
            return tss;
        }

        public PeakSetCollection FindPeaks(int nPeaks) {
            PeakSetCollection psc = new();
            foreach (TimeSeries ts in series.Values) {
                PeakSet ps = new(ts);
                while (ps.Count > nPeaks) {
                    ps.RemoveSmallestValley();
                }

                psc.AddPeakSet(ps);
            }
            return psc;
        }

        private readonly bool LogDataUpdates = true;

        public void DetectNegativeCounts(string path) {

            StringBuilder sb = new();

            foreach (TimeSeries ts1 in series.Values) {
                ts1.DetectNegativeCounts(sb);
            }

            if (LogDataUpdates) {
                Util.WriteToFile(sb, Path.Combine(path, "NegativeCounts.txt"));
            }

        }

        public void DetectOutliers(string path) {

            StringBuilder sb = new();

            foreach (TimeSeries ts1 in series.Values) {
                ts1.DetectOutliers(sb);
            }

            if (LogDataUpdates) {
                Util.WriteToFile(sb, Path.Combine(path, "Outliers.txt"));
            }
        }

        public TimeSeriesSet RemoveAnomalies(List<(string, int)> admin0List, List<(string, int)> admin0StarList, List<(string, string, int)> admin1StarList, 
                                        List<(string, string, string, int)> admin2List, string path) {
            TimeSeriesSet tss = new(SeriesType);
            StringBuilder sb = new();

            foreach (TimeSeries ts1 in series.Values) {
                List<int> days = GetAnomalies(ts1, admin0List, admin0StarList, admin1StarList, admin2List);
                if (days != null) {
                    TimeSeries ts2 = ts1.RemoveAnomalies(days, sb);
                    tss.AddSeries(ts2);
                } else {
                    tss.AddSeries(ts1);
                }

            }

            if (LogDataUpdates) {
                Util.WriteToFile(sb, Path.Combine(path, "AnomaliesRemoved.txt"));
            }

            return tss;
        }

        private static List<int> GetAnomalies(TimeSeries ts, List<(string, int)> admin0List, List<(string, int)> admin0StarList, 
                                            List<(string, string, int)> admin1StarList, List<(string, string, string, int)> admin2List) {
            List<int> days = new();
            if (string.IsNullOrEmpty(ts.Admin1)) {
                foreach ((string admin0, int day) in admin0List) {
                    if (ts.Admin0 == admin0) {
                        days.Add(day);
                    }
                }

            } else if (string.IsNullOrEmpty(ts.Admin2)) {
                foreach ((string admin0, int day) in admin0StarList) {
                    if (ts.Admin0 == admin0) {
                        days.Add(day);
                    }
                }
            } else {
                foreach ((string admin0, string admin1, int day) in admin1StarList) {
                    if (ts.Admin0 == admin0 && ts.Admin1 == admin1) {
                        days.Add(day);
                    }
                }
                foreach ((string admin0, string admin1, string admin2, int day) in admin2List) {
                    if (ts.Admin0 == admin0 && ts.Admin1 == admin1 && ts.Admin2 == admin2) {
                        days.Add(day);
                    }
                }
            }

            return (days.Count > 0) ? days : null;
        }

        private delegate double ComputeDistance(TimeSeries ts1, TimeSeries ts2);
        private delegate double ComputeDistance2(TimeSeries ts1, TimeSeries ts2, int start, int end);

        public List<(TimeSeries, double)> GetDistanceList(string tsKey) {

            return GetValueList((ts1, ts2) => ts1.NormalizedDistance(ts2), tsKey);
        }

        public List<(TimeSeries, double)> GetDistanceList(string tsKey, int start, int end) {

            return GetValueList2((ts1, ts2, start, end) => ts1.NormalizedDistance(ts2, start, end), tsKey, start, end);
        }

        public List<(TimeSeries, double)> GetCosineList(string tsKey) {
            return GetValueList((ts1, ts2) => ts1.CosineDistance(ts2), tsKey);
        }

        private List<(TimeSeries, double)> GetValueList(ComputeDistance distance, string tsKey) {
            List<(TimeSeries, double)> tList = new();
            TimeSeries ts = GetSeries(tsKey);

            foreach (TimeSeries ts1 in series.Values) {
                tList.Add((ts1, distance(ts, ts1)));
            }

            tList.Sort((x, y) => (x.Item2).CompareTo(y.Item2));

            return tList;
        }

        private List<(TimeSeries, double)> GetValueList2(ComputeDistance2 distance, string tsKey, int start, int end) {
            List<(TimeSeries, double)> tList = new();
            TimeSeries ts = GetSeries(tsKey);

            foreach (TimeSeries ts1 in series.Values) {
                tList.Add((ts1, distance(ts, ts1, start, end)));
            }

            tList.Sort((x, y) => (x.Item2).CompareTo(y.Item2));

            return tList;
        }

    }

}
