using System.Windows;
using System.Collections.Generic;
using System.Text;
using DataSeries;


namespace CaseCounter {
    /// <summary>
    /// Interaction logic for AnalysisWindow.xaml
    /// </summary>
    public partial class AnalysisWindow : Window {

        private TimeSeries timeSeries;
        private PeakSet peaks;
        public AnalysisWindow(TimeSeries ts) {
            InitializeComponent();
            timeSeries = ts;
            peaks = new();
 
        }

        private void AnalyzeCurve(TimeSeries ts) {

            peaks = new(ts);;
            PrintPeaks();

        }

        private void PrintPeaks() {
            StringBuilder sb = new();
            sb.AppendLine(timeSeries.Key);

            int i = 1;
            foreach (Peak peak in peaks) {
                sb.Append(i + " " + peak + "\r\n");
                i++;
            }
            sb.AppendLine();

            (int peakIndex, double value) = peaks.FindSmallestValley();

            sb.Append("Smallest valley " + peakIndex + ": " + value);


            analysisTextBox.Text = sb.ToString();
        }
        private void RemovePeak_Click(object sender, RoutedEventArgs e) {
            peaks.RemoveSmallestValley();
            PrintPeaks();
        }

        private void FindPeaks_Click(object sender, RoutedEventArgs e) {
            AnalyzeCurve(timeSeries);
        }
    }
}
