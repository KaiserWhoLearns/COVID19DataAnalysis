using System.Windows;
using System.Collections.Generic;
using System.Text;
using DataSeries;
using CurveClassifier;

namespace CaseCounter {
    /// <summary>
    /// Interaction logic for AnalysisWindow.xaml
    /// </summary>
    public partial class AnalysisWindow : Window {

        private TimeSeries timeSeries;
        private List<Peak> peaks;
        public AnalysisWindow(TimeSeries ts) {
            InitializeComponent();
            timeSeries = ts;
            peaks = new();
 
        }

        private void AnalyzeCurve(TimeSeries ts) {
 

            peaks = ts.FindPeaks();
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


            analysisTextBox.Text = sb.ToString();
        }
        private void RemovePeak_Click(object sender, RoutedEventArgs e) {
            Classifier.RemoveSmallestValley(peaks);
            PrintPeaks();
        }

        private void FindPeaks_Click(object sender, RoutedEventArgs e) {
            AnalyzeCurve(timeSeries);
        }
    }
}
