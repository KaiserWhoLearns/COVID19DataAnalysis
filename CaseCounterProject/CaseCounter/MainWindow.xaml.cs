using System;
using System.Collections.Generic;
using System.Windows;
using DataSeries;
using Utilities;
using WaveAnalyzer;
using System.Windows.Media;
using System.Windows.Controls;
using System.IO;






namespace CaseCounter {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private List<CaseCountTable> caseCountTables;
        private TimeSeriesSet timeSeriesSetOne;
        private TimeSeriesSet timeSeriesSetTwo;
        private List<string> filesInTSS;

        private string topLevelDirectory;

        public TimeSeriesSet TimeSeriesSetOne { get { return timeSeriesSetOne; } }

        public TimeSeriesSet TimeSeriesSetTwo { get { return timeSeriesSetTwo; } }

        public MainWindow() {
            InitializeComponent();
            InitializeApplication();
        }

        private void InitializeApplication() {
            caseCountTables = new List<CaseCountTable>();
            timeSeriesSetOne = new();
            timeSeriesSetTwo = new();
            filesInTSS = new List<string>();
            topLevelDirectory = "C:\\Users\\anderson\\Documents\\GitHub\\COVID19DataAnalysis\\data\\UW time series\\Global";
        }


        /* Loading time series - the current usage model for the application is loading time series and then exploring the data,  so these routines
         * are just loading a TimeSeriesSet from a file.   Currently,  two separate time series sets are maintained.
         * */


        private void ClearTimeSeries_Click(object sender, RoutedEventArgs e) {
            timeSeriesSetOne = new();
            filesInTSS = new List<string>();
            timeSeries1ListBox.Items.Clear();
        }

        private void LoadTimeSeries_Click(object sender, RoutedEventArgs e) {

            TimeSeriesSet tss = LoadTimeSeries(timeSeries1ListBox, out bool result);
            if (result) {
                timeSeriesSetOne = tss;
            }
        }

        private void LoadTimeSeries2_Click(object sender, RoutedEventArgs e) {
            TimeSeriesSet tss = LoadTimeSeries(timeSeries2ListBox, out bool result);
            if (result) {
                timeSeriesSetTwo = tss;
            }
        }


        private TimeSeriesSet LoadTimeSeries( ListBox listBox, out bool result ) {  
            colorCanvas.Background = Brushes.Red;

            TimeSeriesSet tss = new();

            Microsoft.Win32.OpenFileDialog openFileDialog = new();

            openFileDialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            result = openFileDialog.ShowDialog() ?? false;


            if (result) {

                try {
                    tss.LoadCsv(openFileDialog.FileName);
                    tss.ToListBox(listBox);

                    int lastDay = tss.LastDay();
                    startDaySlider.Maximum = lastDay;
                    endDaySlider.Maximum = lastDay;
                    endDaySlider.Value = lastDay;

                } catch (Exception exception) {
                    _ = System.Windows.MessageBox.Show(exception.Message);
                };
            }

            colorCanvas.Background = Brushes.Green;
            return tss;
        }

        /* Call a routine that is essentially a long script that goes through the pipeline of converting source
         * data files to a directory of processed time series sets.  
         */
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

        /* Call a routine that is a script to print out summary information
         */

        private void ExportSummaries_Click(object sender, RoutedEventArgs e) {
            colorCanvas.Background = Brushes.Yellow;

            ResultExporter re = new(caseListBox);
            bool result = re.Build();

            if (result) {
                colorCanvas.Background = Brushes.Green;
            } else {
                colorCanvas.Background = Brushes.Red; ;
            }
        }

        private void ClearSelection_Click(object sender, RoutedEventArgs e) {
            timeSeries1ListBox.SelectedItems.Clear();
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e) {
            timeSeries1ListBox.SelectAll();
        }

        private enum ScaleOptions { None, Population, Count };

        private void Display_Multiple_Click(object sender, RoutedEventArgs e) {
            DisplayMultiple(ScaleOptions.None);
        }

        private void Display_MultipleByPop_Click(object sender, RoutedEventArgs e) {
            DisplayMultiple(ScaleOptions.Population);
        }

        private void Display_MultipleByCount_Click(object sender, RoutedEventArgs e) {
            DisplayMultiple(ScaleOptions.Count);
        }

        private readonly bool TruncateNegative = true;

        private void DisplayMultiple(ScaleOptions scaleOptions) {
            List<(TimeSeries, int)> displayList = new();

            AddToDisplayList(displayList, timeSeriesSetOne, timeSeries1ListBox, 0, scaleOptions, TruncateNegative);
            AddToDisplayList(displayList, timeSeriesSetTwo, timeSeries2ListBox, 1, scaleOptions, TruncateNegative);

            ChartWindow cw = new(displayList);
            cw.Show();

        }

        private void AddToDisplayList(List<(TimeSeries, int)> displayList, TimeSeriesSet tss, ListBox listBox, int axisIndex, ScaleOptions scaleOptions, bool truncateNegative) {
            foreach (object tsKey in listBox.SelectedItems) { 
                TimeSeries ts = tss.GetSeries((string)tsKey);
                if (truncateNegative) {
                    ts = ts.TruncateNegative();
                }
                if (scaleOptions == ScaleOptions.Population) {
                    ts = ts.ScaleByPopulation();
                } else if (scaleOptions == ScaleOptions.Count) {
                    ts = ts.ScaleByCount();
                }
                displayList.Add((ts, axisIndex));
            }
        }

        private void CountPeaks_Click(object sender, RoutedEventArgs e) {
            string tsKey = (string)timeSeries1ListBox.SelectedItem;
            if (!string.IsNullOrEmpty(tsKey)) {
                TimeSeries ts = timeSeriesSetOne.GetSeries(tsKey);
                AnalysisWindow aw = new(ts);
                aw.Show();
            } else {
                System.Windows.MessageBox.Show("No time series selected");
            }
        }

        private void DisplayPeaks_Click(object sender, RoutedEventArgs e) {
            string tsKey = (string)timeSeries1ListBox.SelectedItem;
            if (!string.IsNullOrEmpty(tsKey)) {
                TimeSeries ts = timeSeriesSetOne.GetSeries(tsKey);
                MountainWindow mw = new(ts);
                mw.Show();
            } else {
                System.Windows.MessageBox.Show("No time series selected");
            }
        }


        private void ExportPeaks3_Click(object sender, RoutedEventArgs e) {
            PeakSetCollection peaks = timeSeriesSetOne.FindPeaks(3);
            SavePeakSetCollection(peaks);
        }

        private void ExportPeaks4_Click(object sender, RoutedEventArgs e) {
            PeakSetCollection peaks = timeSeriesSetOne.FindPeaks(4);
            SavePeakSetCollection(peaks);
        }

        private void ExportPeaks5_Click(object sender, RoutedEventArgs e) {
            PeakSetCollection peaks = timeSeriesSetOne.FindPeaks(5);
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

        private void CurveExplorer_Click(object sender, RoutedEventArgs e) {
            string tsKey = (string)timeSeries1ListBox.SelectedItem;
            if (!string.IsNullOrEmpty(tsKey)) {
                TimeSeries ts = timeSeriesSetOne.GetSeries(tsKey);
                CurveExplorerWindow cw = new(ts);
                cw.Show();
            } else {
                System.Windows.MessageBox.Show("No time series selected");
            }
        }


        private void ComputeDistances_Click(object sender, RoutedEventArgs e) {
            caseListBox.Items.Clear();

            List<TimeSeries> tsList = new();
            foreach (object tsKey in timeSeries1ListBox.SelectedItems) {
                tsList.Add(timeSeriesSetOne.GetSeries((string)tsKey));
            }

            for (int i = 0; i < tsList.Count - 1; i++) {
                for (int j = i + 1; j < tsList.Count; j++) {
                    string str = $"{tsList[i].ShortName} to {tsList[j].ShortName}: {tsList[i].NormalizedDistance(tsList[j]):f4}";
                    caseListBox.Items.Add(str);
                }
            }

        }

        private void ExportAllPeaks_Click(object sender, RoutedEventArgs e) {
            colorCanvas.Background = Brushes.Yellow;

            PeakExporter pe = new(caseListBox);
            bool result = pe.Build();

            if (result) {
                colorCanvas.Background = Brushes.Green;
            } else {
                colorCanvas.Background = Brushes.Red; ;
            }

        }

        private void ComputeCosines_Click(object sender, RoutedEventArgs e) {
            caseListBox.Items.Clear();

            List<TimeSeries> tsList = new();
            foreach (object tsKey in timeSeries1ListBox.SelectedItems) {
                tsList.Add(timeSeriesSetOne.GetSeries((string)tsKey));
            }

            for (int i = 0; i < tsList.Count - 1; i++) {
                for (int j = i + 1; j < tsList.Count; j++) {
                    string str = $"{tsList[i].ShortName} to {tsList[j].ShortName}: {tsList[i].CosineDistance(tsList[j]):f4}";
                    caseListBox.Items.Add(str);
                }
            }
        }

        private delegate List<(TimeSeries, double)> GetValueList(TimeSeriesSet tss, string key);
        private void AllDistances_Click(object sender, RoutedEventArgs e) {

            AllDistances((tss, key) => tss.GetDistanceList(key));
        }

        private void AllCosines_Click(object sender, RoutedEventArgs e) {
            AllDistances((tss, key) => tss.GetCosineList(key));
        }

        private void AllDistances(GetValueList valueList) {
            string tsKey = (string)timeSeries1ListBox.SelectedItem;

            if (!string.IsNullOrEmpty(tsKey)) {
                List<(TimeSeries, double)> tsList = valueList(timeSeriesSetOne, tsKey); 

                caseListBox.Items.Clear();
                TimeSeries ts = timeSeriesSetOne.GetSeries(tsKey);
                foreach ((TimeSeries ts1, double val) in tsList) {
                    string str = $"{ts.ShortName} to {ts1.ShortName}: {val:f4}";
                    caseListBox.Items.Add(str);
                }
            }

        }

        private void WindowDistances_Click(object sender, RoutedEventArgs e) {
            int startDay = (int)startDaySlider.Value;
            int endDay = (int)endDaySlider.Value;
            AllDistances((tss, key) => tss.GetDistanceList(key, startDay, endDay));
        }

        private void MakeWaves_Click(object sender, RoutedEventArgs e) {
            fileListBox.Items.Clear();
            WaveParameters wp = new();

            wp.WaveSmoothing = int.Parse(waveSmoothTextBox.Text);

            foreach (string tsKey in timeSeries1ListBox.SelectedItems) {
                TimeSeries ts = timeSeriesSetOne.GetSeries(tsKey);
                WaveSet ws = new(ts, wp);

                fileListBox.Items.Add(ws.SummaryString());
            }

        }

        private void WaveDetails_Click(object sender, RoutedEventArgs e) {
            if (timeSeries1ListBox.SelectedItems.Count == 0) {
                _ = MessageBox.Show("Item not selected");
            }
            else {
                string tsKey = (string) timeSeries1ListBox.SelectedItem;
                TimeSeries ts = timeSeriesSetOne.GetSeries(tsKey);
                WaveSet ws = new(ts, new());
                if (timeSeriesSetTwo != null) {         // If there is a matching file of deaths,  add deaths to waveset
                    TimeSeries ts1 = timeSeriesSetTwo.LookupCorrespondingSeries(ts);
                    if (ts1 != null) {
                        ws.AddDeath(ts1);
                    }
                }
                caseListBox.Items.Clear();
                ws.AddToListBox(caseListBox);
            }
        }

        private void Summarize_Click(object sender, RoutedEventArgs e) {
            fileListBox.Items.Clear();
            foreach (string tsKey in timeSeries1ListBox.SelectedItems) {
                TimeSeries ts = timeSeriesSetOne.GetSeries(tsKey);
                WaveSet ws = new(ts, new());
                fileListBox.Items.Add(ws.MainPeaks(0.10));
            } 
        }

        DrawingWindow drawingWindow;

        private void Drawing_Click(object sender, RoutedEventArgs e) {

            List<TimeSeries> tsList = new();

            foreach (object tsKey in timeSeries1ListBox.SelectedItems) {
                TimeSeries ts = timeSeriesSetOne.GetSeries((string)tsKey);
                if (ts.ValidGIS) {
                    tsList.Add(ts);
                }
            }

            if (tsList.Count > 0) {
                drawingWindow = new(tsList);
                drawingWindow.Show();
            }
        }

        private void MapInfo_Click(object sender, RoutedEventArgs e) {

            Random random = new();

            List<TimeSeries> tsList = new();

            foreach (object tsKey in timeSeries1ListBox.SelectedItems) {
                TimeSeries ts = timeSeriesSetOne.GetSeries((string)tsKey);
                if (ts.ValidGIS) {
                    tsList.Add(ts);
                }
            }

            if (tsList.Count > 0) {
                drawingWindow = new(tsList);
                drawingWindow.Show();
            }
        }


        private void LaunchMapView(string mapName, string fileName) {
            if (string.IsNullOrEmpty(topLevelDirectory)) {
                SetDirectory();
            }

            TimeSeriesSet tss = new();
            string filePath = Path.Combine(topLevelDirectory, fileName);
            tss.LoadCsv(filePath);

            MapView mv = new(mapName, tss);
            mv.Show();

        }
        private void USMapView_Click(object sender, RoutedEventArgs e) {
            LaunchMapView("UnitedStates", "World by province/US_confirmed_sm.csv");
        }

        private void AfricaMapView_Click(object sender, RoutedEventArgs e) {
            LaunchMapView("Africa", "World by country/ContinentalAfrica_confirmed_sm.csv");
        }

        private void IndiaMapView_Click(object sender, RoutedEventArgs e) {
            LaunchMapView("India", "World by province/India_confirmed_sm.csv");
        }

        // We need to set the top level directory for looking up files
        private void SetDirectory_Click(object sender, RoutedEventArgs e) {
            SetDirectory();
        }

        private void SetDirectory() {  
            _ = MessageBox.Show("Select top level directory");
            System.Windows.Forms.FolderBrowserDialog folderDialog = new ();
            System.Windows.Forms.DialogResult result = folderDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK) {
                topLevelDirectory = folderDialog.SelectedPath;
            }  
        }


        /* Data processing and cleaning routines - these have been replaced by the single script, Build Library which converts the input data sources into
         * a set of cleaned files.  These were used during initial development - but no longer are needed.
         */
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
                    cct.AddToTSS(timeSeriesSetOne);
                }
            }
            timeSeriesSetOne.ToListBox(timeSeries1ListBox);

            colorCanvas.Background = Brushes.Aqua;
        }

        private void ClearSourceFile_Click(object sender, RoutedEventArgs e) {
            timeSeriesSetOne = new();
            filesInTSS = new List<string>();
            caseCountTables = new List<CaseCountTable>();
            fileListBox.Items.Clear();
            caseListBox.Items.Clear();
        }

        private void SaveTimeSeries_Click(object sender, RoutedEventArgs e) {
            SaveTimeSeries(timeSeriesSetOne);
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
            }
            colorCanvas.Background = Brushes.Green;
        }

        /* Time series conversion routines - this are now all done by calls from a script - so strictly useful for testing or debugging now */ 

        private void DailyCount_Click(object sender, RoutedEventArgs e) {
            SaveTimeSeries(timeSeriesSetOne.ToDailyCount());
        }

        private void FilterTimeSeriesSet(Predicate<TimeSeries> filter) {
            SaveTimeSeries(timeSeriesSetOne.Filter(filter));
        }

        private void WeekSmooth_Click(object sender, RoutedEventArgs e) {
            SaveTimeSeries(timeSeriesSetOne.WeeklySmoothing());
        }



        private void GaussSmooth_Click(object sender, RoutedEventArgs e) {
            SaveTimeSeries(timeSeriesSetOne.GaussianSmoothing());
        }


    }
}
