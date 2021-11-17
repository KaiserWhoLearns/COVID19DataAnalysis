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
using DataSeries;

namespace CaseCounter {
    /// <summary>
    /// Interaction logic for SegmentWindow.xaml
    /// </summary>
    public partial class SegmentWindow : Window {

        private readonly int startingSegments = 10;

        private TimeSeries timeSeries;
        private SegmentSet segments;
        private ScottPlot.Plottable.ScatterPlot segmentCurve;
        private int nSegments;


        public SegmentWindow(TimeSeries ts) {
            InitializeComponent();
            timeSeries = ts;
            PlotSeries(ts);
        }

        void PlotSeries(TimeSeries ts) {

            double[] dataX = new double[ts.LastDay + 1];
            for (int i = 0; i < dataX.Length; i++) {
                dataX[i] = i;
            }
            double[] dataY = ts.GetData();

            ScottPlot.Plottable.ScatterPlot curve = wpfPlot3.Plot.AddScatter(dataX, dataY);
            curve.Label = ts.Key;

            wpfPlot3.Plot.XAxis.Label("Days since 1-22-20");
            wpfPlot3.Plot.YAxis.Label("Reported cases or deaths");
            wpfPlot3.Plot.Legend(location: ScottPlot.Alignment.UpperLeft);

            wpfPlot3.Refresh();

        }

        private void FindSegments_Click(object sender, RoutedEventArgs e) {
            nSegments = startingSegments;
            segments = new(timeSeries, nSegments);
            UpdateSegments();
        }


        private void UpdateSegments() {
            if (segmentCurve != null) {
                wpfPlot3.Plot.Remove(segmentCurve);
            }

            (double[] dataX, double[] dataY) = segments.BuildDataSeries();
            segmentCurve = wpfPlot3.Plot.AddScatter(dataX, dataY);
            segmentCurve.LineWidth = 3;
            segmentCurve.Color = System.Drawing.Color.Red;

            wpfPlot3.Refresh();
        }
        private void AddSegment_Click(object sender, RoutedEventArgs e) {
            nSegments++;
            segments = new(timeSeries, nSegments);
            UpdateSegments();
        }

        private void RemoveSegment_Click(object sender, RoutedEventArgs e) {
            if (nSegments > 1) {
                nSegments--;
                segments = new(timeSeries, nSegments);
                UpdateSegments();
            }
        }
    }
}
