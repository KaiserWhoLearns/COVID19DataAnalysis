using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using CsvHelper;
using System.Linq;
using System.Globalization;
using System.Windows.Controls;
using Utilities;




namespace DataSeries {

    public enum DataType { Confirmed, Deaths}
    public enum SeriesType { Cummulative, Discrete}
    public class TimeSeries {

        public string Key { get; }
        public string Admin0 { get; }       // Country
        public string Admin1 { get; }       // State or Province
        public string Admin2 { get; }       // District or County

        public int Fips { get; }            // 5 digit  FIPS (Federal Information Processing System) codes for US Counties.  0 otherwise

        public double Latitude { get; }
        public double Longitude { get; }

        public DataType DataType { get; }


        public int LastDay { get; set; }

        // Need to track date of population update - but this is not public info
        private int lastPopulationUpdate;

        // Popluation comes from JHU data by Confirmed/Incidence * 100,000.  We will use the value from the latest date.
        public long Population { get; set; }

        public bool ValidGIS {
            get {
                return Latitude >= -90 && Latitude <= 90 && Longitude >= -180 && Longitude <= 180;
            }
        }
        public int CaseCount() {
            return CaseCount(0, LastDay);
        }

        public int CaseCount(int start, int end) {
            if (data == null || LastDay < 0) {
                return 0;
            }
            double count = 0.0;
            for (int i = start; i <= end; i++) {
                count += data[i];
            }
            return (int)count;
        }

        public double L2Norm {
            get {
                if (data == null || LastDay < 0) {
                    return 0;
                }
                double count = 0.0;
                for (int i = 0; i <= LastDay; i++) {
                    count += data[i]*data[i];
                }

                return Math.Sqrt(count);
            }
        }

        private double[] data;

        public TimeSeries(DataType dataType, string admin0, string admin1, string admin2, int fips, double latitude, double longitude, long population = -1) {
            DataType = dataType;
            Admin0 = admin0;
            Admin1 = admin1;
            Admin2 = admin2;
            Fips = fips;
            Latitude = latitude;
            Longitude = longitude;

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
                    case "Fips":
                        Fips = int.Parse(val);
                        break;
                    case "Latitude":
                        Latitude = double.Parse(val);
                        break;
                    case "Longitude":
                        Longitude = double.Parse(val);
                        break;
                    case "DataType":
                        DataType = (DataType)Enum.Parse(typeof(DataType), val);
                        break;
                    case "Population":
                        Population = long.Parse(val);
                        break;
                    case "CaseCount":           // CaseCount is a field for export, but generated when needed, so not read back in
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

        public TimeSeries(TimeSeries ts) {
            DataType = ts.DataType;
            Admin0 = ts.Admin0;
            Admin1 = ts.Admin1;
            Admin2 = ts.Admin2;
            Fips = ts.Fips;
            Latitude = ts.Latitude;
            Longitude = ts.Longitude;
            Population = ts.Population;
            Key = ts.Key;
            LastDay = ts.LastDay;
            lastPopulationUpdate = ts.lastPopulationUpdate;
            data = new double[ts.data.Length];
            for (int i = 0; i < data.Length; i++) {
                data[i] = ts.data[i];
            }
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
            _ = sb.Append(DataType + ",\"" + Admin2 + "\",\"" + Admin1 + "\",\"" + Admin0 + "\"," + Fips + "," + Latitude.ToString("F2") 
                +"," + Longitude.ToString("F2") +"," + Population + "," + CaseCount());

         

            for (int i = 0; i <= LastDay; i++) {
                _ = sb.Append("," + data[i].ToString("F2"));
            }

            return sb.ToString();
        }

        public TimeSeries ToDailyCount() {
            TimeSeries ts = new(DataType, Admin0, Admin1, Admin2, Fips, Latitude, Longitude, Population);

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
            TimeSeries ts = new(DataType, Admin0, Admin1, Admin2, Fips, Latitude, Longitude, Population);
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

        // Average over +-d days.  This is done separately from smooth to make it more efficient.
        public TimeSeries BlockSmooth(int d) {
            TimeSeries ts = new(DataType, Admin0, Admin1, Admin2, Fips, Latitude, Longitude, Population);

            if (LastDay <= d) {
                double val = CaseCount() / (d + 1);
                for (int i = 0; i <= LastDay; i++) {
                    ts.SetValue(i, val);
                }
                return ts;
            }

            double sum = 0;
            for (int i = 0; i < d+1; i++) {
                sum += data[i];
            }
            int len = d + 1;
            ts.SetValue(0, sum / len);

            for (int i = 1; i <= LastDay; i++) {
                if (i > d) {
                    sum -= data[i - d - 1];
                }
                else {
                    len += 1;
                }
                if (i <= LastDay - d) {
                    sum += data[i + d];
                }
                else {
                    len -= 1;
                }
                ts.SetValue(i, sum / len);
            }


            return ts;
        }

        public TimeSeries DoubleSmooth(int d) {
            return BlockSmooth(d).BlockSmooth(d);
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
/*
        public PeakSet FindPeaks() {
            Classifier cc = new(data, LastDay + 1);
            return cc.FindPeaks();
        }
*/

        public TimeSeries ScaleByPopulation() {
            TimeSeries ts = new(DataType, Admin0, Admin1, Admin2, Fips, Latitude, Longitude, Population);

                            // Cases per 100K or deaths per 10M
            double scaleFactor = (DataType == DataType.Confirmed) ? 100000 : 10000000;
            for (int i = 0; i <= LastDay; i++) {
                double val = (Population >= 1) ? data[i] * scaleFactor / Population : 0;
                ts.SetValue(i, val);
            }

            return ts;
        }

        public TimeSeries ScaleByCount() {
            TimeSeries ts = new(DataType, Admin0, Admin1, Admin2, Fips, Latitude, Longitude, Population);

            double total = 0.0;
            for (int i = 0; i <= LastDay; i++)
                total += data[i];

            for (int i = 0; i <= LastDay; i++) {
                double val = (total > 0) ? data[i] / total : 0;
                ts.SetValue(i, val);
            }

            return ts;

        }

        public TimeSeries TruncateNegative() {
            TimeSeries ts = new(DataType, Admin0, Admin1, Admin2, Fips, Latitude, Longitude, Population);

            for (int i = 0; i <= LastDay; i++) {
                ts.SetValue(i, Math.Max(data[i], 0.0));
            }

            return ts;

        }

        public PeakSet FindPeaks(int nPeaks) {
            return null;
        }

        public void DetectNegativeCounts(StringBuilder sb) {

            bool negativeFound = false;
            for (int i = 0; i <= LastDay; i++) {
                if (data[i] < -999) {
                    if (! negativeFound) {
                        negativeFound = true;
                        sb.Append($"{Key}: ");
                    }
                    sb.Append($"<{i},{data[i]}>");
                }
            }
            if (negativeFound) {
                sb.Append("\r\n");
            }


        }

        public void DetectOutliers(StringBuilder sb) {

            bool outlierFound = false;
            for (int i = 0; i <= LastDay; i++) {
                if (OutlierTest(i)) {
                    if (!outlierFound) {
                        outlierFound = true;
                        sb.Append($"{Key}: ");
                    }
                    sb.Append($"<{i},{data[i]}>");
                }
            }
            if (outlierFound) {
                sb.Append("\r\n");
            }

        }

        // An outlier is a value that is at least 100 and at least K times the daily average of three days before and after

        private readonly int OutlierConstant = 1000;
        private readonly int OutlierFactor = 10;
        private bool OutlierTest(int i) {
            if (data[i] < OutlierConstant) {
                return false;
            }

            return data[i] > OutlierFactor * LocalAverage(i);
        }

        // average of 3 days before and 3 days after.  A pain because of boundary conditions
        private  double LocalAverage(int i) {
            double val = 0.0; 
            double days = 0;

            if (i >= 3) {
                val = data[i - 1] + data[i - 2] + data[i - 3];
                days = 3;
            } else if (i == 2) {
                val = data[1] + data[0];
                days = 2;
            } else if (i == 1) {
                val = data[0];
                days = 1;
            }

            if (i <= LastDay - 3) {
                val += data[i + 1] + data[i + 2] + data[i + 3];
                days += 3;
            } else if (i == LastDay - 2) {
                val += data[i + 1] + data[i + 2];
                days += 2;
            } else if (i == LastDay - 1) {
                val += data[i + 1];
                days += 1;
            }

            // Need to avoid the divide by zero case where there is only one data point
            return days > 0 ? val / days : data[0];
        }

        public TimeSeries RemoveAnomalies(List<int> days, StringBuilder sb) {
            TimeSeries ts = new(this);
            sb.Append(Key);
            foreach (int day in days) {
                ts.data[day] = LocalAverage(day);
                sb.Append($" {day}: {data[day]} -> {ts.data[day]}  ");
            }
            sb.Append("\r\n");
            return ts;
        }

        // Compute the distance between normalized time series - using euclidean distance on the daily counts
        public double NormalizedDistance(TimeSeries ts) {
            if (LastDay != ts.LastDay)
                return -1;

            double count1 = CaseCount();
            double count2 = ts.CaseCount();

            double sum = 0.0;

            for (int i = 0; i <= LastDay; i++) {
                double diff = data[i] / count1 - ts.data[i] / count2;
                sum += diff * diff;
            }
            return Math.Sqrt(sum);
        }

        public double NormalizedDistance(TimeSeries ts, int start, int end) {
            if (LastDay != ts.LastDay)
                return -1;
            if (start < 0 || end > LastDay || start > end) {
                throw new ProgrammingException("Invalid date range in Normalized Distance");
            }
                 

            double count1 = CaseCount(start, end);
            double count2 = ts.CaseCount(start, end);

            double sum = 0.0;

            for (int i = start; i <= end; i++) {
                double diff = data[i] / count1 - ts.data[i] / count2;
                sum += diff * diff;
            }
            return Math.Sqrt(sum);
        }

        public double CosineDistance(TimeSeries ts) {
            if (LastDay != ts.LastDay)
                return -1;

 

            double sum = 0.0;

            for (int i = 0; i <= LastDay; i++) {
                sum += data[i] * ts.data[i];
            }

            return sum / (L2Norm * ts.L2Norm);
        }

        public string ShortName {
            get {
                return string.IsNullOrEmpty(Admin2) ? (string.IsNullOrEmpty(Admin1) ? Admin0 : Admin1) : Admin2;
            }
        }

        public TimeSeries ForwardArea() {
            TimeSeries ts = new(DataType, Admin0, Admin1, Admin2, Fips, Latitude, Longitude, Population);

            for (int i = 0; i <= LastDay; i++) {
                ts.SetValue(i, FindForwardArea(i));
            }

            return ts;
        }

        private double FindForwardArea(int i) {
            double val = data[i];
            int k = i;
            while (k < LastDay && data[k + 1] >= val) {
                k++;
            }
            return val * (k - i);
        }

        // Compute a discrete derivative.  Our derivative is two sided,  so we are returning f'(x) = (f(x+d) - f(x-d))/2d
        public TimeSeries Derivative(int d) {

            TimeSeries ts = new(DataType, Admin0, Admin1, Admin2, Fips, Latitude, Longitude, Population);

            for (int i = 0; i <= LastDay; i++) {
                ts.SetValue(i, FindDerivative(i, d));
            }

            return ts;
        }

        private double FindDerivative(int i, int d) {
            if (LastDay == 0) {
                return 0;                   // Avoid the divide by zero on a one day data set
            }

            int startDay = Math.Max(0, i - d);
            int endDay = Math.Min(LastDay, i + d);

            return (data[endDay] - data[startDay]) / (endDay - startDay);

        }

        public (int, double) MaximumDerivative(int d, int start, int end) {
            if (end - start < 2 * d) {
                return ((start + end) / 2, 0.0);
            }

            int maxPos = start + d;
            double maxSoFar = FindDerivative(maxPos, d);
            for (int i = start + d; i <= end - d; i++) {
                double deriv = FindDerivative(i, d);
                if (deriv > maxSoFar) {
                    maxSoFar = deriv;
                    maxPos = i;
                }
            }
            return (maxPos, maxSoFar);  
        }

        public (int, double) MinimumDerivative(int d, int start, int end) {
            if (end - start < 2 * d) {
                return ((start + end) / 2, 0.0);
            }

            int minPos = start + d;
            double minSoFar = FindDerivative(minPos, d);
            for (int i = start + d; i <= end - d; i++) {
                double deriv = FindDerivative(i, d);
                if (deriv < minSoFar) {
                    minSoFar = deriv;
                    minPos = i;
                }
            }
            return (minPos, minSoFar);
        }


        public (double[], double[]) FindCriticalPoints() {
            List<int> xList = new();

            xList.Add(0);
            if (LastDay > 0) {
                bool goingUp = (data[1] >= data[0]);

                for (int i = 1; i < LastDay; i++) {
                    if (goingUp) {
                        if (data[i] > data[i + 1]) {
                            goingUp = false;
                            xList.Add(i);
                        }
                    } else {
                        if (data[i] < data[i + 1]) {
                            goingUp = true;
                            xList.Add(i);
                        }
                    }
                }
                xList.Add(LastDay);
            }

            double[] yArray = new double[xList.Count];
            double[] xArray = new double[xList.Count];
            for (int i = 0; i < xList.Count; i++) {
                yArray[i] = data[xList[i]];
                xArray[i] = xList[i];
            }

            return (xArray, yArray);
        }

        public (double[], double[]) FindInflectionPoints(int d) {
            TimeSeries derivTS = Derivative(d);
            (double[] xArray, double[] yArray) =  derivTS.FindCriticalPoints();

            for (int i = 0; i < xArray.Length; i++) {           // Move the inflection points to the original curve
                yArray[i] = data[(int)xArray[i]];
            }

            return (xArray, yArray);
        }

        public double NormalizedCaseCount(int startDay, int endDay) {
            if (Population <= 0) {
                return 0.0;
            }

            double  count = 0;
            for (int i = startDay; i <= endDay; i++) {
                count += data[i];
            }

            return count / Population;
        }

    }

}
