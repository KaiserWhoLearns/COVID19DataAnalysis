using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using CsvHelper;
using System.Linq;
using System.Globalization;
using System.Windows.Controls;
using Utilities;
using CurveClassifier;



namespace DataSeries {

    public enum DataType { Confirmed, Deaths}
    public enum SeriesType { Cummulative, Discrete}
    public class TimeSeries {

        public string Key { get; }
        public string Admin0 { get; }       // Country
        public string Admin1 { get; }       // State or Province
        public string Admin2 { get; }       // District or County

        public DataType DataType { get; }


        public int LastDay { get; set; }

        // Need to track date of population update - but this is not public info
        private int lastPopulationUpdate;

        // Popluation comes from JHU data by Confirmed/Incidence * 100,000.  We will use the value from the latest date.
        public long Population { get; set; }

        private double[] data;

        public TimeSeries(DataType dataType, string admin0, string admin1, string admin2, long population = -1) {
            DataType = dataType;
            Admin0 = admin0;
            Admin1 = admin1;
            Admin2 = admin2;
            Population = population;

            Key = BuildKey(dataType, admin0, admin1, admin2);

            data = new double[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            LastDay = -1;
            lastPopulationUpdate = -1;

        }

        // Build a time series from a row in a CSV
        public TimeSeries(IEnumerable<KeyValuePair<string, object>> row) {

            data = new double[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            LastDay = -1;

            foreach (KeyValuePair<string, object> kvp in row) {
                string key = kvp.Key;
                string val = (string)kvp.Value;

                switch (kvp.Key) {
                    case "Admin0":
                        Admin0 = val;
                        break;
                    case "Admin1":
                        Admin1 = val;
                        break;
                    case "Admin2":
                        Admin2 = val;
                        break;
                    case "DataType":
                        DataType = (DataType)Enum.Parse(typeof(DataType), val);
                        break;
                    case "Population":
                        Population = long.Parse(val);
                        break;

                    default:
                        if (key.StartsWith("Day ")) {
                            int day = int.Parse(key.Substring(4));
                            double count = (String.IsNullOrEmpty(val)) ? 0 : double.Parse(val);
                            SetValue(day, count);
                        }
                        break;
                }

            }
            Key = BuildKey(DataType, Admin0, Admin1, Admin2);

        }

        public void SetValue(int index, double? value) {

            if (index >= data.Length) {
                Resize(index);
            }

            data[index] = value ?? 0;
            LastDay = Math.Max(index, LastDay);
        }

        public void UpdatePopulation(int index, int totalCases,  double incidence) {
            if (index > lastPopulationUpdate) {      // Update only takes place on latest date,  we separate this from the actual update so as not to depend on order of updates
                lastPopulationUpdate = index;
                Population = (incidence > 0) ? (long)(totalCases * 100000.0 / incidence) : 0;
            }
        }

        private void Resize(int n) {
            double[] temp = new double[2 * n];
            for (int i = 0; i < data.Length; i++) {
                temp[i] = data[i];
            }
            for (int i = data.Length; i < 2 * n; i++) {
                temp[i] = 0;
            }

            data = temp;
        }

        public static string BuildKey(DataType dataType, string admin0, string admin1, string admin2) {
            if (!string.IsNullOrEmpty(admin0)) {
                string str = string.IsNullOrEmpty(admin1) ? admin0 : string.IsNullOrEmpty(admin2) ? admin0 + "/" + admin1 : admin0 + "/" + admin1 + "/" + admin2;
                return str + "+" + dataType;
            }
            throw new Utilities.DataException("Missing country in attempting to create time series");
        }

        public override string ToString() {

            StringBuilder sb = new();
            for (int i = 0; i < data.Length; i++) {
                _ = sb.Append(" " + data[i]);
            }

            return Key + " "  + sb.ToString();
        }

        public string ToRowString() {

            StringBuilder sb = new();
            _ = sb.Append(DataType + ",\"" + Admin2 + "\",\"" + Admin1 + "\",\"" + Admin0 + "\"," + Population);

            for (int i = 0; i <= LastDay; i++) {
                _ = sb.Append("," + data[i].ToString("F2"));
            }

            return sb.ToString();
        }

        public TimeSeries ToDailyCount() {
            TimeSeries ts = new(DataType, Admin0, Admin1, Admin2, Population);

            ts.SetValue(0, data[0]);
            for (int i = 1; i <= LastDay; i++) {
                double dailyCount = data[i] - data[i - 1];
                ts.SetValue(i, dailyCount);
            }
            return ts;
        }

        public double[] GetData() {
            double[] ddata = new double[LastDay + 1];
            for (int i = 0; i < ddata.Length; i++)
                ddata[i] = data[i];

            return ddata;

        }

        public TimeSeries WeeklySmoothing() {
            return Smooth(TimeSeriesSet.WeekFilter);
        }

        public TimeSeries GaussianSmoothing() {
            return Smooth(TimeSeriesSet.GaussianFilter);
        }
        public TimeSeries Smooth(double[] filter) {
            TimeSeries ts = new(DataType, Admin0, Admin1, Admin2, Population);
            for (int i = 0; i <= LastDay; i++)
                ts.SetValue(i, Smooth(i, filter));

            return ts;
        }

        private double Smooth(int index, double[] filter) {

            if (filter.Length % 2 == 0)
                throw new ProgrammingException("Filter length must be odd in TimeSeries.Smooth");

            int k = filter.Length / 2;
            int start = Math.Max(index - k, 0);
            int end = Math.Min(index + k, LastDay);
            double val = 0.0;
            double wt = 0.0;
            for (int i = start; i <= end; i++) {
                int j = i - (index - k);
                val += filter[j] * data[i];
                wt += filter[j];
            }
            return val / wt;

        }

        public void AddCounts(TimeSeries ts) {
            if (ts.LastDay > LastDay)           //Ensure that the array is long enough
                SetValue(ts.LastDay, 0);

            for (int i = 0; i <= ts.LastDay; i++)
                data[i] += ts.data[i];
                                            // Slightly complicated to handle -1 as the no-population value
                                            // Possibly,  using 0 as the no-population value would make things easier
            if (ts.Population != -1)
                Population = (Population == -1) ? ts.Population : Population + ts.Population;
        }

        public List<Peak> FindPeaks() {
            Classifier cc = new(data, LastDay + 1);
            return cc.FindPeaks();
        }
    }

 
}
