using System;
using System.Collections.Generic;
using System.Drawing;
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
    /// Interaction logic for MountainWindow.xaml
    /// </summary>
    public partial class MountainWindow : Window {

        private TimeSeries timeSeries;
        private PeakSet peaks;
        private ScottPlot.Plottable.ScatterPlot peakCurve;

        public MountainWindow(TimeSeries ts) {
            InitializeComponent();
            timeSeries = ts;
            PeakSet peaks1 = new(ts);
            numberOfPeaksSlider.Maximum = peaks1.Count;
            peaks = peaks1;

            PlotSeries(ts);
        }

        void PlotSeries(TimeSeries ts) {




            double[] dataX = new double[ts.LastDay + 1];
            for (int i = 0; i < dataX.Length; i++) {
                dataX[i] = i;
            }
            double[] dataY = ts.GetData();


            ScottPlot.Plottable.ScatterPlot curve = wpfPlot2.Plot.AddScatter(dataX, dataY);
            curve.Label = ts.Key;




            wpfPlot2.Plot.XAxis.Label("Days since 1-22-20");
            wpfPlot2.Plot.YAxis.Label("Reported cases or deaths");
            wpfPlot2.Plot.Legend(location: ScottPlot.Alignment.UpperLeft);


            wpfPlot2.Refresh();

        }
        private void FindPeaks_MW_Click(object sender, RoutedEventArgs e) {
            peaks = new(timeSeries);
            UpdatePeaks();

        }

        private void UpdatePeaks() {
            if (peakCurve != null) {
                wpfPlot2.Plot.Remove(peakCurve);
            }

            (double[] dataX, double[] dataY) = peaks.BuildDataSeries();
            peakCurve = wpfPlot2.Plot.AddScatter(dataX, dataY);
            peakCurve.LineWidth = 3;
            peakCurve.Color = System.Drawing.Color.Red;

            wpfPlot2.Refresh();
        }

        private void RemovePeak_MW_Click(object sender, RoutedEventArgs e) {

            peaks.RemoveSmallestValley();
            UpdatePeaks();

        }

        private void NPeaks_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (timeSeries == null) {
                return;
            }

            int n = (int)numberOfPeaksSlider.Value;
            peaks = new(timeSeries, n);
            UpdatePeaks();
        }
    }
}
