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
    public partial class CurveExplorerWindow : Window {

        private readonly int startingSegments = 10;

        private TimeSeries timeSeries;
        private SegmentSet segments;
        private ScottPlot.Plottable.ScatterPlot timeSeriesCurve;
        private ScottPlot.Plottable.ScatterPlot overlayCurve;
        private int nSegments;


        public CurveExplorerWindow(TimeSeries ts) {
            timeSeries = ts;
            InitializeComponent();
            PlotSeries(ts);
        }


        private void PlotSeries(TimeSeries ts) {  
            double[] dataX = new double[ts.LastDay + 1];
            for (int i = 0; i < dataX.Length; i++) {
                dataX[i] = i;
            }
            double[] dataY = ts.GetData();

            timeSeriesCurve = wpfPlot3.Plot.AddScatter(dataX, dataY);
            timeSeriesCurve.Label = ts.Key;

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
            if (overlayCurve != null) {
                wpfPlot3.Plot.Remove(overlayCurve);
            }

            (double[] dataX, double[] dataY) = segments.BuildDataSeries();
            overlayCurve = wpfPlot3.Plot.AddScatter(dataX, dataY);
            overlayCurve.LineWidth = 3;
            overlayCurve.Color = System.Drawing.Color.Red;

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

        private void ForwardArea_Click(object sender, RoutedEventArgs e) {

            TimeSeries ts1 = timeSeries.ForwardArea();
            double[] dataX = new double[ts1.LastDay + 1];
            for (int i = 0; i < dataX.Length; i++) {
                dataX[i] = i;
            }
            double[] dataY = ts1.GetData();  

  //          if (overlayCurve != null) {
   //             wpfPlot3.Plot.Remove(overlayCurve);
  //          }

            wpfPlot3.Plot.Clear();
            PlotSeries(timeSeries);
 
            overlayCurve = wpfPlot3.Plot.AddScatter(dataX, dataY);

            overlayCurve.YAxisIndex = 1;
            wpfPlot3.Plot.YAxis2.Ticks(true);
            overlayCurve.LineWidth = 3;
            overlayCurve.Color = System.Drawing.Color.Lime;

            wpfPlot3.Refresh();
        }

        private void ClearCurve_Click(object sender, RoutedEventArgs e) {
            if (overlayCurve != null) {
                wpfPlot3.Plot.Remove(overlayCurve);
            }
            overlayCurve = null;

            wpfPlot3.Refresh();
        }

        private void Derivative_Click(object sender, RoutedEventArgs e) {

            TimeSeries ts1 = timeSeries.Derivative(7);

            double[] dataX = new double[ts1.LastDay + 1];
            for (int i = 0; i < dataX.Length; i++) {
                dataX[i] = i;
            }
            double[] dataY = ts1.GetData();  

            if (overlayCurve != null) {
                wpfPlot3.Plot.Remove(overlayCurve);
            }


            wpfPlot3.Plot.Clear();
            PlotSeries(timeSeries);


            overlayCurve = wpfPlot3.Plot.AddScatter(dataX, dataY);

            overlayCurve.YAxisIndex = 1;
            wpfPlot3.Plot.YAxis2.Ticks(true);
            overlayCurve.LineWidth = 3;
            overlayCurve.Color = System.Drawing.Color.Orange;

            wpfPlot3.Refresh();
        }

        private void Smooth_Click(object sender, RoutedEventArgs e) {
            UpdateSmoothing();
        }

        private void Smoothing_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (wpfPlot3 != null)
                UpdateSmoothing();
        }

        private void UpdateSmoothing() {
            TimeSeries ts1 = timeSeries.BlockSmooth((int) smoothingDaysSlider.Value);
            double[] dataX = new double[ts1.LastDay + 1];
            for (int i = 0; i < dataX.Length; i++) {
                dataX[i] = i;
            }
            double[] dataY = ts1.GetData();

            if (overlayCurve != null) {
                wpfPlot3.Plot.Remove(overlayCurve);
            }


            wpfPlot3.Plot.Clear();
            PlotSeries(timeSeries);


            overlayCurve = wpfPlot3.Plot.AddScatter(dataX, dataY);

            overlayCurve.YAxisIndex = 0;
            wpfPlot3.Plot.YAxis2.Ticks(true);
            overlayCurve.LineWidth = 3;
            overlayCurve.Color = System.Drawing.Color.Aqua;

            wpfPlot3.Refresh();
        }

    }
}
