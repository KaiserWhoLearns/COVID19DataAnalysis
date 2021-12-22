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
        private WaveParameters waveParameters;


        public DrawingWindow(List<TimeSeries> tsList) {
            InitializeComponent();

            timeSeriesList = tsList;
            coordList = LatLongList(tsList);
            waveParameters = new();

            PlotSeries(tsList, coordList,  null);
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

        private void Query_Click(object sender, RoutedEventArgs e) {
            List<double?> valueList = new();

            foreach (TimeSeries ts in timeSeriesList) {
                WaveSet waves = new(ts, waveParameters);
                double? val = waves.Query(343);
                valueList.Add(val);
            }

            PlotSeries(timeSeriesList, coordList, valueList);
        }

        public delegate double? WaveQuery(WaveSet waves);
        private void ExecuteQuery(WaveQuery waveQuery) {
            List<double?> valueList = new();

            foreach (TimeSeries ts in timeSeriesList) {
                WaveSet waves = new(ts, waveParameters);
                double? val = waveQuery(waves);
 //               double? val = waves.Query(343);
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

        private void Query1_Click(object sender, RoutedEventArgs e) {
            int d = (int)dateSlider.Value;
            ExecuteQuery(waves => waves.Query(d));

        }


        private void Query4_Click(object sender, RoutedEventArgs e) {
            int d = (int)dateSlider.Value;
            ExecuteQuery(waves => waves.Query2(d));
        }

 

        private void Query7_Click(object sender, RoutedEventArgs e) {
            int d = (int)dateSlider.Value;
            ExecuteQuery(waves => waves.Query3(d));
        }

    }
}
