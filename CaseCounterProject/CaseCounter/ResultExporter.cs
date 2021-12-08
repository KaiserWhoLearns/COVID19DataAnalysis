using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.IO;
using DataSeries;
using WaveAnalyzer;

namespace CaseCounter {
    public class ResultExporter : TaskScript {

        private static string USBC = "United States by county";
        private static string WBC = "World by country";
        private static string WBP = "World by province";

        private string[] subDirectoryList = { "Country Summary", "Res2" };
        public static string CS = "Country Summary";

        string topLevelInputDir;
        string topLevelOutputDir;

        public ResultExporter(ListBox listBox) : base(listBox) {
        }

        public override bool Build() {
            bool error;

            topLevelInputDir = GetTopLevelInputDirectory(out error);
            if (error) {
                return false;
            }

            ReportStep("Get Input Directory");
            topLevelOutputDir = CreateOutputDirectories(subDirectoryList, out error);

            if (error) {
                return false;
            }

            ReportStep("Create Output Directories");
            CountrySummaries();
            return true;
        }

        private void CountrySummaries() {
            foreach (string continent in config.ContinentList) {
                ContinentSummary(continent);
            }

            // For each continent
            //      Summary Header
            //      Read in confirmed / deaths
            //      For each confirmed
            //          Find matching death
            //          Create summary string
            //      Make file name
            //      Write CSV
        }

        private void ContinentSummary(string continent) {
            string confFilePath = Path.Combine(topLevelInputDir, WBC, continent + "_confirmed_sm.csv");
            string deathFilePath = Path.Combine(topLevelInputDir, WBC, continent + "_deaths_sm.csv");
            string outputFilePath = Path.Combine(topLevelOutputDir, CS, continent + "_summary.csv");

            WaveSummary summary = new BasicSummary();

            TimeSeriesSet tssConf = new(confFilePath);
            TimeSeriesSet tssDeaths = new(deathFilePath);

            using (StreamWriter outputFile = new(outputFilePath)) {
                outputFile.WriteLine(summary.HeaderString());

                foreach (TimeSeries ts in tssConf) {
                    string tsKey = TimeSeries.BuildKey(DataType.Deaths, ts.Admin0, ts.Admin1, ts.Admin2);
                    TimeSeries ts2 = tssDeaths.GetSeries(tsKey);
                    outputFile.WriteLine(summary.Summarize(ts, ts2));
                }
            }
        }


    }
}
