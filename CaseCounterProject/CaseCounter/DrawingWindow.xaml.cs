using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DrawingManager;
using DataSeries;
using WaveAnalyzer;
using Utilities;

namespace CaseCounter {
    /// <summary>
    /// Interaction logic for DrawingWindow.xaml
    /// </summary>
    public partial class DrawingWindow : Window {

        private ScatterPlot scatterPlot;
        private List<TimeSeries> timeSeriesList;
        private List<Point> coordList;
        private List<WaveSet> waveSetList;

        public delegate double? WaveQuery(WaveSet waves, int date);
        private WaveQuery waveQuery;

        public DrawingWindow(List<TimeSeries> tsList) {
            InitializeComponent();

            timeSeriesList = tsList;
            coordList = LatLongList(tsList);
            WaveParameters waveParameters = new();

            PlotSeries(tsList, coordList,  null);
            waveSetList = new();
            foreach (TimeSeries ts in tsList) {
                waveSetList.Add(new(ts, waveParameters));
            }

        }

       
        private static List<Point> LatLongList(List<TimeSeries> tsList) {
            List<Point> pointList = new();

            foreach ( TimeSeries ts  in tsList) {
                double x = ts.Longitude;
                double y = Util.Mercator(ts.Latitude);
                pointList.Add(new(x, y));
            }
            return pointList;
        }

        public void PlotSeries(List<TimeSeries> tsList, List<Point> pointList,  List<double?> valueList) {
            scatterPlot = new(drawingCanvas);
            drawingCanvas.Background = Brushes.BlanchedAlmond;

            scatterPlot.PlotPoints(pointList, valueList, 20);
        }
/*
        private void Query_Click(object sender, RoutedEventArgs e) {
            List<double?> valueList = new();

            foreach (TimeSeries ts in timeSeriesList) {
                WaveSet waves = new(ts, waveParameters);
                double? val = waves.Query(343);
                valueList.Add(val);
            }

            PlotSeries(timeSeriesList, coordList, valueList);
        }

*/
        private void ExecuteQuery(WaveQuery waveQuery, int date) {
            if (waveQuery == null) {
                return;
            }

            List<double?> valueList = new();

            foreach (WaveSet waves in waveSetList) {

                double? val = waveQuery(waves, date);
                valueList.Add(val);
            }

            List<double?> normalized = Normalize(valueList);

            PlotSeries(timeSeriesList, coordList, normalized);
        }

        private List<double?> Normalize(List<double?> vList) {
            double maxValue = double.MinValue;
            double minValue = double.MaxValue;

            foreach (double? val in vList) {
                if (val != null) {
                    maxValue = Math.Max(maxValue, (double) val);
                    minValue = Math.Min(minValue, (double) val);
                }
            }

            // This catches the case where there is at most one distinct value
            if (maxValue <= minValue) {
                return vList;
            }

            double diff = maxValue - minValue;

            List<double?> nList = new();

            foreach (double? val in vList) {
                if (val != null) {
                    double nVal = ((double)val - minValue) / diff;
                    nList.Add(nVal);
                }
                else {
                    nList.Add(val);
                }
            }

            return nList;
        }

        private void Query_Click(object sender, RoutedEventArgs e) {
            ExecuteQuery(waveQuery, (int)dateSlider.Value);

        }


        private void QueryChanged(object sender, SelectionChangedEventArgs e) {

            string query = queryTypeComboBox.SelectedValue.ToString();

            switch (query) {
                case "Weight":
                    waveQuery = (waves, d) => waves.Query(d);
                    break;
                case "Max Date":
                    waveQuery = (waves, d) => waves.Query2(d);
                    break;
                case "Max Value":
                    waveQuery = (waves, d) => waves.Query3(d);
                    break;

            }

            ExecuteQuery(waveQuery, (int)dateSlider.Value);

        }

        private void DateValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            ExecuteQuery(waveQuery, (int)dateSlider.Value);
        }
    }
}
