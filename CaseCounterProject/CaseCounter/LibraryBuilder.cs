using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.IO;
using DataSeries;


namespace CaseCounter {
    public class LibraryBuilder {
        ListBox listBox;

        public LibraryBuilder(ListBox listBox) {
            this.listBox = listBox;
        }

        public bool Build() {
            listBox.Items.Clear();
            ReportStep("Build Libary");
            bool error;

            List<CaseCountTable> tables = LoadDailyFiles(out error);

            if (error) {
                return false;
            }

            ReportStep("Created Case Count Tables, " + tables.Count + " days");

            TimeSeriesSet mainTSS = BuildTimeSeriesSet(tables);

            ReportStep("Built TimeSeriesSet,  Days: " + mainTSS.LastDay() + " Entries: " + mainTSS.Count);

            string topLevelOutputDir = CreateOutputDirectories(directoryList, out error);

            if (error) {
                return false;
            }

            ReportStep("Create Output Directories");

            mainTSS.WriteToFile(Path.Combine(topLevelOutputDir, "TS_Main.csv"));

            ReportStep("Write TS_Main");

            TimeSeriesSet dailyCount_TSS = mainTSS.ToDailyCount();
            dailyCount_TSS.WriteToFile(Path.Combine(topLevelOutputDir, "TS_Daily.csv"));

            ReportStep("Write TS_Daily");

            TimeSeriesSet unitedStates_TSS = dailyCount_TSS.Filter((TimeSeries ts) => ts.Admin0.Equals("US"));
            TimeSeriesSet world_TSS = dailyCount_TSS.Filter((TimeSeries ts) => !ts.Admin0.Equals("US"));
            TimeSeriesSet world_province_TSS = world_TSS.Filter((TimeSeries ts) => !string.IsNullOrEmpty(ts.Admin1));
            TimeSeriesSet world_country_TSS = world_TSS.Filter((TimeSeries ts) => string.IsNullOrEmpty(ts.Admin1));

            unitedStates_TSS.WriteToFile(Path.Combine(topLevelOutputDir, usbc, "USBC_Daily.csv"));
            world_country_TSS.WriteToFile(Path.Combine(topLevelOutputDir, wbc, "WBC_Daily.csv"));
            world_province_TSS.WriteToFile(Path.Combine(topLevelOutputDir, wbp, "WBP_Daily.csv"));



            ReportStep("Write USBC, WBC, WBP");

            TimeSeriesSet world_province_combined_TSS = ExtractNational(world_province_TSS, Path.Combine(topLevelOutputDir, wbp, "World_Provinces_Combined.csv"));

            ReportStep("Combine province files");

            TimeSeriesSet unitedStates_state_TSS = ExtractUSStates(unitedStates_TSS, Path.Combine(topLevelOutputDir, usbc));

            ReportStep("Write US State Data");

            TimeSeriesSet us_only_TSS = ExtractNational(unitedStates_state_TSS, Path.Combine(topLevelOutputDir, usbc, "us_national_daily.csv"));

            ReportStep("Write US National Data");

            world_province_TSS.AddTimeSeriesSet(unitedStates_state_TSS);
            world_province_TSS.WriteToFile(Path.Combine(topLevelOutputDir, wbp, "WBP_Daily_with_us.csv"));

            ReportStep("Add provice and national sets");
            world_country_TSS.AddTimeSeriesSet(world_province_combined_TSS);
            world_country_TSS.AddTimeSeriesSet(us_only_TSS);
            world_country_TSS.WriteToFile(Path.Combine(topLevelOutputDir, wbc, "WBC_Daily_all_countries.csv"));

            // Separate into confirmed and dead files.  Note that smoothing is taking place here - so subsequent selections
            // should not apply smoothing
            TimeSeriesSet us_confirmed_TSS = unitedStates_TSS.FilterAndSmooth((TimeSeries ts) => ts.DataType == DataType.Confirmed);
            us_confirmed_TSS.WriteToFile(Path.Combine(topLevelOutputDir, usbc, "US_confirmed_sm.csv"));
            TimeSeriesSet us_deaths_TSS = unitedStates_TSS.FilterAndSmooth((TimeSeries ts) => ts.DataType == DataType.Deaths);
            us_deaths_TSS.WriteToFile(Path.Combine(topLevelOutputDir, usbc, "US_deaths_sm.csv"));

            TimeSeriesSet us_state_confirmed_TSS = unitedStates_state_TSS.FilterAndSmooth((TimeSeries ts) => ts.DataType == DataType.Confirmed);
            us_state_confirmed_TSS.WriteToFile(Path.Combine(topLevelOutputDir, usbc, "US_state_confirmed_sm.csv"));
            TimeSeriesSet us_state_deaths_TSS = unitedStates_state_TSS.FilterAndSmooth((TimeSeries ts) => ts.DataType == DataType.Deaths);
            us_state_deaths_TSS.WriteToFile(Path.Combine(topLevelOutputDir, usbc, "US_state_deaths_sm.csv"));

            TimeSeriesSet world_province_confirmed_TSS = world_province_TSS.FilterAndSmooth((TimeSeries ts) => ts.DataType == DataType.Confirmed);
            world_province_confirmed_TSS.WriteToFile(Path.Combine(topLevelOutputDir, wbp, "World_province_confirmed_sm.csv"));
            TimeSeriesSet world_province_deaths_TSS = world_province_TSS.FilterAndSmooth((TimeSeries ts) => ts.DataType == DataType.Deaths);
            world_province_deaths_TSS.WriteToFile(Path.Combine(topLevelOutputDir, wbp, "World_province_deaths_sm.csv"));

            TimeSeriesSet world_confirmed_TSS = world_country_TSS.FilterAndSmooth((TimeSeries ts) => ts.DataType == DataType.Confirmed);
            world_confirmed_TSS.WriteToFile(Path.Combine(topLevelOutputDir, wbc, "World_confirmed_sm.csv"));
            TimeSeriesSet world_deaths_TSS = world_country_TSS.FilterAndSmooth((TimeSeries ts) => ts.DataType == DataType.Deaths);
            world_deaths_TSS.WriteToFile(Path.Combine(topLevelOutputDir, wbc, "World_deaths_sm.csv"));

            ReportStep("Separate cases/deaths and smooth");

            TimeSeriesSet india_state_confirmed_TSS = world_province_confirmed_TSS.Filter((TimeSeries ts) => ts.Admin0 == "India");
            india_state_confirmed_TSS.WriteToFile(Path.Combine(topLevelOutputDir, wbp, "India_confirmed_sm.csv"));
            TimeSeriesSet washington_confirmed_TSS = us_confirmed_TSS.Filter((TimeSeries ts) => ts.Admin1 == "Washington");
            washington_confirmed_TSS.WriteToFile(Path.Combine(topLevelOutputDir, usbc, "Washington_confirmed_sm.csv"));


            ReportStep("Create additional country/state data sets");


            return true;
        }

        private List<CaseCountTable> LoadDailyFiles(out bool error) {
            List<CaseCountTable> tables = new();
            _ = MessageBox.Show("Select directory for input files");

            error = false;

            System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = folderDialog.ShowDialog();


            if (result == System.Windows.Forms.DialogResult.OK) {
                string path = folderDialog.SelectedPath;

                string[] fileEntries = Directory.GetFiles(path);
                foreach (string fileName in fileEntries) {
                    if (!CaseCountTable.ValidFileName(fileName, out _, out _)) {
                        continue;
                    }
                    try {
                        CaseCountTable cct = new(fileName);
                        cct.Clean();                     // Replace nulls with zero values
                        tables.Add(cct);

                    } catch (Exception exception) {
                        _ = MessageBox.Show(exception.Message);
                    }

                }
            } else {
                error = true;
            }

            return tables;
        }

        private TimeSeriesSet BuildTimeSeriesSet(List<CaseCountTable> tables) {
            TimeSeriesSet tss = new(SeriesType.Cummulative);

            foreach (CaseCountTable cct in tables) {
                cct.AddToTSS(tss);
            }
            return tss;
        }



        private string[] directoryList = { "World by country", "World by province", "United States by county" };
        private string wbc = "World by country";
        private string wbp = "World by province";
        private string usbc = "United States by county";
        private string CreateOutputDirectories(string[] directories, out bool error) {
            error = false;

            MessageBox.Show("Select directory for output");
            System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = folderDialog.ShowDialog();
            string path = folderDialog.SelectedPath;
            if (result == System.Windows.Forms.DialogResult.OK) {


                foreach (string dir in directories) {
                    string path1 = System.IO.Path.Combine(path, dir);
                    if (Directory.Exists(path1)) {
                        MessageBox.Show("Directory " + dir + " already exists");
                        error = true;
                        return path;
                    }
                }
                foreach (string dir in directories) {
                    string path1 = System.IO.Path.Combine(path, dir);
                    Directory.CreateDirectory(path1);
                }
            } else {
                error = true;
                return "";
            }
            return path;

        }

        TimeSeriesSet ExtractUSStates(TimeSeriesSet unitedStatesTSS, string dirPath) {
            TimeSeriesSet stateSet = unitedStatesTSS.CombineAdmin1();

            string filePath = Path.Combine(dirPath, "USBC_state_daily.csv");
            stateSet.WriteToFile(filePath);
            return stateSet;

        }

        TimeSeriesSet ExtractUSNational(TimeSeriesSet unitedStatesTSS, string dirPath) {
            TimeSeriesSet nationalSet = unitedStatesTSS.CombineAdmin0();

            string filePath = Path.Combine(dirPath, "USBC_national_daily.csv");
            nationalSet.WriteToFile(filePath);

            return nationalSet;

        }

        TimeSeriesSet ExtractNational(TimeSeriesSet nationalTSS, string filePath) {
            TimeSeriesSet nationalSet = nationalTSS.CombineAdmin0();

            nationalSet.WriteToFile(filePath);

            return nationalSet;

        }
        private void ReportStep(string str) {
            _ = listBox.Items.Add(str);

        }
    }
}
