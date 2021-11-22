using System;
using System.Collections.Generic;
using System.Windows;
using DataSeries;
using Utilities;
using System.Windows.Media;






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



        private void FilterTimeSeriesSet(Predicate<TimeSeries> filter) {
            SaveTimeSeries(timeSeriesSet.Filter(filter));
        }



        private void DailyCount_Click(object sender, RoutedEventArgs e) {
            SaveTimeSeries(timeSeriesSet.ToDailyCount());
        }

        private enum ChartOptions { Raw, Weekly, Gaussian };
        private enum ScaleOptions { None, Population, Count };

        private void Display_Click(object sender, RoutedEventArgs e) {
            DisplayChart(ChartOptions.Raw, ScaleOptions.None);
        }

        private void DisplayChart(ChartOptions chartOptions, ScaleOptions scaleOptions) {  
            string tsKey = (string) timeSeriesListBox.SelectedItem;
            if (!string.IsNullOrEmpty(tsKey)) {
                TimeSeries ts = timeSeriesSet.GetSeries(tsKey);
                if (chartOptions == ChartOptions.Weekly) {
                    ts = ts.WeeklySmoothing();
                } else if (chartOptions == ChartOptions.Gaussian) {
                    ts = ts.GaussianSmoothing();
                }
                if (scaleOptions == ScaleOptions.Population) {
                    ts = ts.ScaleByPopulation();
                } else if (scaleOptions == ScaleOptions.Count) {
                    ts = ts.ScaleByCount();
                }

                List<TimeSeries> tsList = new();
                tsList.Add(ts);

                ChartWindow cw = new(tsList);
                cw.Show();
            } else {
                _ = MessageBox.Show("No time series selected");
            }
        }

        private void WeekSmooth_Click(object sender, RoutedEventArgs e) {
            SaveTimeSeries(timeSeriesSet.WeeklySmoothing());
        }



        private void GaussSmooth_Click(object sender, RoutedEventArgs e) {
            SaveTimeSeries(timeSeriesSet.GaussianSmoothing());
        }

        private void DisplayWs_Click(object sender, RoutedEventArgs e) {
            DisplayChart(ChartOptions.Weekly, ScaleOptions.None);

        }

        private void DisplayGs_Click(object sender, RoutedEventArgs e) {
            DisplayChart(ChartOptions.Gaussian, ScaleOptions.None);
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

        private void DisplayMultiple(ScaleOptions scaleOptions) {
            List<TimeSeries> tsList = new();

            foreach (object tsKey in timeSeriesListBox.SelectedItems) {
                TimeSeries ts = timeSeriesSet.GetSeries((string)tsKey);

                if (scaleOptions == ScaleOptions.Population) {
                    ts = ts.ScaleByPopulation();
                } else if (scaleOptions == ScaleOptions.Count) {
                    ts = ts.ScaleByCount();
                }
                tsList.Add(ts);
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

        private void DisplayByPop_Click(object sender, RoutedEventArgs e) {
            DisplayChart(ChartOptions.Raw, ScaleOptions.Population);
        }

        private void DisplayByCount_Click(object sender, RoutedEventArgs e) {
            DisplayChart(ChartOptions.Raw, ScaleOptions.Count);
        }

        private void Display_MultipleByPop_Click(object sender, RoutedEventArgs e) {
            DisplayMultiple(ScaleOptions.Population);
        }

        private void Display_MultipleByCount_Click(object sender, RoutedEventArgs e) {
            DisplayMultiple(ScaleOptions.Count);
        }

        private void ExportPeaks3_Click(object sender, RoutedEventArgs e) {
            PeakSetCollection peaks = timeSeriesSet.FindPeaks(3);
            SavePeakSetCollection(peaks);
        }

        private void ExportPeaks4_Click(object sender, RoutedEventArgs e) {
            PeakSetCollection peaks = timeSeriesSet.FindPeaks(4);
            SavePeakSetCollection(peaks);
        }

        private void ExportPeaks5_Click(object sender, RoutedEventArgs e) {
            PeakSetCollection peaks = timeSeriesSet.FindPeaks(5);
            SavePeakSetCollection(peaks);
        }

        private void SavePeakSetCollection(PeakSetCollection peaks) {

            Microsoft.Win32.SaveFileDialog saveFileDialog = new();


            saveFileDialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;

            bool? result = saveFileDialog.ShowDialog();


            if (result == true) {
                peaks.WriteToFile(saveFileDialog.FileName);

            }
 
        }

        private void DisplaySegments_Click(object sender, RoutedEventArgs e) {
            string tsKey = (string)timeSeriesListBox.SelectedItem;
            if (!string.IsNullOrEmpty(tsKey)) {
                TimeSeries ts = timeSeriesSet.GetSeries(tsKey);
                SegmentWindow sw = new(ts);
                sw.Show();
            } else {
                System.Windows.MessageBox.Show("No time series selected");
            }
        }

        private void ComputeDistances_Click(object sender, RoutedEventArgs e) {
            caseListBox.Items.Clear();

            List<TimeSeries> tsList = new();
            foreach (object tsKey in timeSeriesListBox.SelectedItems) {
                tsList.Add(timeSeriesSet.GetSeries((string)tsKey));
            }

            for (int i = 0; i < tsList.Count - 1; i++) {
                for (int j = i + 1; j < tsList.Count; j++) {
                    string str = $"{tsList[i].ShortName} to {tsList[j].ShortName}: {tsList[i].NormalizedDistance(tsList[j])}";
                    caseListBox.Items.Add(str);
                }
            }

        }
    }
}
