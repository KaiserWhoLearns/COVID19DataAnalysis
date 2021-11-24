﻿using System;
using System.Collections.Generic;
using System.Windows;
using DataSeries;
using Utilities;
using System.Windows.Media;
using System.Windows.Controls;






namespace CaseCounter {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private List<CaseCountTable> caseCountTables;
        private TimeSeriesSet timeSeriesSetOne;
        private TimeSeriesSet timeSeriesSetTwo;
        private List<string> filesInTSS;

        public TimeSeriesSet TimeSeriesSetOne { get { return timeSeriesSetOne; } }

        public TimeSeriesSet TimeSeriesSetTwo { get { return timeSeriesSetTwo; } }

        public MainWindow() {
            InitializeComponent();
            InitializeApplication();
        }

        private void InitializeApplication() {
            caseCountTables = new List<CaseCountTable>();
            timeSeriesSetOne = new(SeriesType.Cummulative);
            timeSeriesSetTwo = new(SeriesType.Cummulative);
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
                    cct.AddToTSS(timeSeriesSetOne);
                }
            }
            timeSeriesSetOne.ToListBox(timeSeries1ListBox);

            colorCanvas.Background = Brushes.Aqua;
        }

        private void ClearTimeSeries_Click(object sender, RoutedEventArgs e) {
            timeSeriesSetOne = new(SeriesType.Cummulative);
            filesInTSS = new List<string>();
            timeSeries1ListBox.Items.Clear();
        }


        private void ClearSourceFile_Click(object sender, RoutedEventArgs e) {
            timeSeriesSetOne = new(SeriesType.Cummulative);
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

        private void LoadTimeSeries_Click(object sender, RoutedEventArgs e) {

            TimeSeriesSet tss = LoadTimeSeries(timeSeries1ListBox, out bool result);
            if (result) {
                timeSeriesSetOne = tss;
            }
        }

        private TimeSeriesSet LoadTimeSeries( ListBox listBox, out bool result ) {  
            colorCanvas.Background = Brushes.Red;

            TimeSeriesSet tss = new(SeriesType.Cummulative);

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

        private void LoadTimeSeries2_Click(object sender, RoutedEventArgs e) {
            TimeSeriesSet tss = LoadTimeSeries(timeSeries2ListBox, out bool result);
            if (result) {
                timeSeriesSetTwo = tss;
            }
        }

        private void SelectCountries_Click(object sender, RoutedEventArgs e) {
            SelectionWindow sw = new SelectionWindow();
            sw.Show();
        }



        private void FilterTimeSeriesSet(Predicate<TimeSeries> filter) {
            SaveTimeSeries(timeSeriesSetOne.Filter(filter));
        }



        private void DailyCount_Click(object sender, RoutedEventArgs e) {
            SaveTimeSeries(timeSeriesSetOne.ToDailyCount());
        }

        private enum ChartOptions { Raw, Weekly, Gaussian };
        private enum ScaleOptions { None, Population, Count };

        private void Display_Click(object sender, RoutedEventArgs e) {
            DisplayChart(ChartOptions.Raw, ScaleOptions.None);
        }

        private void DisplayChart(ChartOptions chartOptions, ScaleOptions scaleOptions) {  
            string tsKey = (string) timeSeries1ListBox.SelectedItem;
            if (!string.IsNullOrEmpty(tsKey)) {
                TimeSeries ts = timeSeriesSetOne.GetSeries(tsKey);
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
            SaveTimeSeries(timeSeriesSetOne.WeeklySmoothing());
        }



        private void GaussSmooth_Click(object sender, RoutedEventArgs e) {
            SaveTimeSeries(timeSeriesSetOne.GaussianSmoothing());
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
            foreach (object tsKey in timeSeries1ListBox.SelectedItems) {
                tsList.Add(timeSeriesSetOne.GetSeries((string)tsKey));
            }
            foreach (object tsKey in timeSeries2ListBox.SelectedItems) {
                tsList.Add(timeSeriesSetTwo.GetSeries((string)tsKey));
            }

            ChartWindow cw = new(tsList);
            cw.Show();
        }

        private void DisplayMultiple(ScaleOptions scaleOptions) {
            List<TimeSeries> tsList = new();

            foreach (object tsKey in timeSeries1ListBox.SelectedItems) {
                TimeSeries ts = timeSeriesSetOne.GetSeries((string)tsKey);

                if (scaleOptions == ScaleOptions.Population) {
                    ts = ts.ScaleByPopulation();
                } else if (scaleOptions == ScaleOptions.Count) {
                    ts = ts.ScaleByCount();
                }
                tsList.Add(ts);
            }
            foreach (object tsKey in timeSeries2ListBox.SelectedItems) {
                TimeSeries ts = timeSeriesSetTwo.GetSeries((string)tsKey);

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

        private void DisplaySegments_Click(object sender, RoutedEventArgs e) {
            string tsKey = (string)timeSeries1ListBox.SelectedItem;
            if (!string.IsNullOrEmpty(tsKey)) {
                TimeSeries ts = timeSeriesSetOne.GetSeries(tsKey);
                SegmentWindow sw = new(ts);
                sw.Show();
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
    }
}
