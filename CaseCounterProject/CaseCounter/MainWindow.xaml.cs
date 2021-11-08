using System;
using System.Collections.Generic;
using System.Windows;

using DataSeries;
using Utilities;
using System.Text;
using System.Windows.Media;
using ScottPlot;





namespace CaseCounter {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private List<CaseCountTable> caseCountTables;
        private TimeSeriesSet timeSeriesSet;
        private List<string> filesInTSS;

        public TimeSeriesSet TimeSeriesSet { get { return timeSeriesSet; } }
        public MainWindow() {
            InitializeComponent();
            InitializeApplication();
        }

        private void InitializeApplication() {
            caseCountTables = new List<CaseCountTable>();
            timeSeriesSet = new TimeSeriesSet(SeriesType.Cummulative);
            filesInTSS = new List<string>();
        }



        private void LoadSourceFile_Click(object sender, RoutedEventArgs e) {
            colorCanvas.Background = Brushes.Red;
            Microsoft.Win32.OpenFileDialog openFileDialog = new();
            openFileDialog.Multiselect = true;

            CaseCountTable caseCountTable;

            openFileDialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            bool? result = openFileDialog.ShowDialog();

            if (result == true) {
                foreach (string filePath in openFileDialog.FileNames) {

                    if (!CaseCountTable.ValidFileName(filePath, out _, out _)) {
                        _ = System.Windows.MessageBox.Show(filePath + " is not a valid filename");
                        continue;
                    }
                    try {
                        caseCountTable = new CaseCountTable(filePath);
                        caseCountTable.Clean();                     // Replace nulls with zero values
                        caseCountTables.Add(caseCountTable);
                        caseCountTable.ToListBox(caseListBox);
                        _ = fileListBox.Items.Add(Util.ExtractFilename(filePath));

                    } catch (Exception exception) {
                        _ = System.Windows.MessageBox.Show(exception.Message);
                    };
                }
            }
            colorCanvas.Background = Brushes.Blue;
        }


        private void BuildTimeSeries_Click(object sender, RoutedEventArgs e) {
            colorCanvas.Background = Brushes.Red;
            foreach (CaseCountTable cct in caseCountTables) {
                if (!filesInTSS.Contains(cct.FileName)) {
                    filesInTSS.Add(cct.FileName);
                    cct.AddToTSS(timeSeriesSet);
                }
            }
            timeSeriesSet.ToListBox(timeSeriesListBox);

            colorCanvas.Background = Brushes.Aqua;
        }

        private void ClearTimeSeries_Click(object sender, RoutedEventArgs e) {
            timeSeriesSet = new TimeSeriesSet(SeriesType.Cummulative);
            filesInTSS = new List<string>();
            timeSeriesListBox.Items.Clear();
        }

        private void ViewAllTimeSeries_Click(object sender, RoutedEventArgs e) {
            SecondWindow sw = new SecondWindow();
            sw.Show();

            sw.SecondTB.Text = timeSeriesSet.ToString();
        }

        private void ClearSourceFile_Click(object sender, RoutedEventArgs e) {
            timeSeriesSet = new TimeSeriesSet(SeriesType.Cummulative);
            filesInTSS = new List<string>();
            caseCountTables = new List<CaseCountTable>();
            fileListBox.Items.Clear();
            caseListBox.Items.Clear();
        }

        private void SaveTimeSeries_Click(object sender, RoutedEventArgs e) {
            SaveTimeSeries(timeSeriesSet);
        }

        private void SaveTimeSeries(TimeSeriesSet tss) {
            colorCanvas.Background = Brushes.Red;
            Microsoft.Win32.SaveFileDialog saveFileDialog = new();


            saveFileDialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;

            bool? result = saveFileDialog.ShowDialog();


            if (result == true) {
                tss.WriteToFile(saveFileDialog.FileName);
    //            string[] outputTable = tss.CsvOutput();
    //            System.IO.File.WriteAllLines(saveFileDialog.FileName, outputTable);
            }
            colorCanvas.Background = Brushes.Green;
        }

        private void LoadTimeSeries_Click(object sender, RoutedEventArgs e) {
            colorCanvas.Background = Brushes.Red;

            Microsoft.Win32.OpenFileDialog openFileDialog = new();



            openFileDialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            bool? result = openFileDialog.ShowDialog();

            if (result == true) {

                try {
                    TimeSeriesSet timeSeriesSet1 = new(SeriesType.Cummulative);
                    timeSeriesSet1.LoadCsv(openFileDialog.FileName);
                    timeSeriesSet = timeSeriesSet1;

                } catch (Exception exception) {
                    _ = System.Windows.MessageBox.Show(exception.Message);
                };
            }
            timeSeriesSet.ToListBox(timeSeriesListBox);
            colorCanvas.Background = Brushes.Green;
        }

        private void SelectCountries_Click(object sender, RoutedEventArgs e) {
            SelectionWindow sw = new SelectionWindow();
            sw.Show();
        }

        private void OnFilterUSStates_Click(object sender, RoutedEventArgs e) {
            static bool filter(TimeSeries ts) => ts.Admin0.Equals("US");
            FilterTimeSeriesSet(filter);
        }

        private void FilterTimeSeriesSet(Predicate<TimeSeries> filter) {
            SaveTimeSeries(timeSeriesSet.Filter(filter));
        }

        private void OnFilterWashington_Click(object sender, RoutedEventArgs e) {
            static bool filter(TimeSeries ts) => ts.Admin1.Equals("Washington");
            FilterTimeSeriesSet(filter);
        }

        private void OnFilterIndia_Click(object sender, RoutedEventArgs e) {
            static bool filter(TimeSeries ts) => ts.Admin0.Equals("India");
            FilterTimeSeriesSet(filter);
        }

        private void OnFilterConfirmed_Click(object sender, RoutedEventArgs e) {
            static bool filter(TimeSeries ts) => ts.DataType == DataType.Confirmed;
            FilterTimeSeriesSet(filter);
        }

        private void OnFilterDeath_Click(object sender, RoutedEventArgs e) {
            static bool filter(TimeSeries ts) => ts.DataType == DataType.Deaths;
            FilterTimeSeriesSet(filter);
        }

        private void DailyCount_Click(object sender, RoutedEventArgs e) {
            SaveTimeSeries(timeSeriesSet.ToDailyCount());
        }

        private enum ChartOptions { Raw, Weekly, Gaussian };

        private void Display_Click(object sender, RoutedEventArgs e) {
            DisplayChart(ChartOptions.Raw);
        }

        private void DisplayChart(ChartOptions options) {  
            string tsKey = (string) timeSeriesListBox.SelectedItem;
            if (!string.IsNullOrEmpty(tsKey)) {
                TimeSeries ts = timeSeriesSet.GetSeries(tsKey);
                if (options == ChartOptions.Weekly) {
                    ts = ts.WeeklySmoothing();
                } else if (options == ChartOptions.Gaussian) {
                    ts = ts.GaussianSmoothing();
                }

                List<TimeSeries> tsList = new();
                tsList.Add(ts);

                ChartWindow cw = new(tsList);
                cw.Show();
            } else {
                System.Windows.MessageBox.Show("No time series selected");
            }
        }

        private void WeekSmooth_Click(object sender, RoutedEventArgs e) {
            SaveTimeSeries(timeSeriesSet.WeeklySmoothing());
        }

        private void GaussSmooth_Click(object sender, RoutedEventArgs e) {
            SaveTimeSeries(timeSeriesSet.GaussianSmoothing());
        }

        private void DisplayWs_Click(object sender, RoutedEventArgs e) {
            DisplayChart(ChartOptions.Weekly);

        }

        private void DisplayGs_Click(object sender, RoutedEventArgs e) {
            DisplayChart(ChartOptions.Gaussian);
        }

        private void BuildLibrary_Click(object sender, RoutedEventArgs e) {

            colorCanvas.Background = Brushes.Yellow;

            LibraryBuilder lb = new(caseListBox);
            bool result = lb.Build();

            if (result) {
                colorCanvas.Background = Brushes.Green;
            } else {
                colorCanvas.Background = Brushes.Red; ;
            }
 

        }

        private void Display_Multiple_Click(object sender, RoutedEventArgs e) {
            List<TimeSeries> tsList = new();
            foreach (object tsKey in timeSeriesListBox.SelectedItems) {
                tsList.Add(timeSeriesSet.GetSeries((string)tsKey));
            }

            ChartWindow cw = new(tsList);
            cw.Show();
        }

        private void CountPeaks_Click(object sender, RoutedEventArgs e) {
            string tsKey = (string)timeSeriesListBox.SelectedItem;
            if (!string.IsNullOrEmpty(tsKey)) {
                TimeSeries ts = timeSeriesSet.GetSeries(tsKey);
                AnalysisWindow aw = new(ts);
                aw.Show();
            } else {
                System.Windows.MessageBox.Show("No time series selected");
            }
        }

        private void DisplayPeaks_Click(object sender, RoutedEventArgs e) {
            string tsKey = (string)timeSeriesListBox.SelectedItem;
            if (!string.IsNullOrEmpty(tsKey)) {
                TimeSeries ts = timeSeriesSet.GetSeries(tsKey);
                MountainWindow mw = new(ts);
                mw.Show();
            } else {
                System.Windows.MessageBox.Show("No time series selected");
            }
        }
    }
}
