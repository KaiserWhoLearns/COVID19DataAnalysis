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

        private static string USBC_C = "United States by county\\Confirmed";
        private static string USBC_D = "United States by county\\Deaths";
        private static string WBC = "World by country";
        private static string WBP = "World by province";

        private string[] subDirectoryList = { "Summaries\\Continents","Summaries\\Countries", "Summaries\\States",
                                              "TopWaves\\Continents","TopWaves\\Countries", "TopWaves\\States"};

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

            BasicSummaries();

            ReportStep("Basic Summaries");

            WaveSummaries();

            ReportStep("Wave Summaries");
            return true;
        }

        private void BasicSummaries() {
            WaveSummary summary = new BasicSummary();
            ContinentSummaries("Summaries", summary);
            CountrySummaries("Summaries", summary);
            StateSummaries("Summaries", summary);
        }

        private void WaveSummaries() {
            WaveSummary summary = new TopWavesSummary();
            ContinentSummaries("TopWaves", summary);
            CountrySummaries("TopWaves", summary);
            StateSummaries("TopWaves", summary);
        }

        private void ContinentSummaries(string subdirectory, WaveSummary summary) {
            string inputCasePath = Path.Combine(topLevelInputDir, WBC);
            string inputDeathPath = Path.Combine(topLevelInputDir, WBC);
            string outputPath = Path.Combine(topLevelOutputDir, subdirectory, "Continents");
            Summaries(config.ContinentList, inputCasePath, inputDeathPath, outputPath, summary);
        }

        private void CountrySummaries(string subdirectory, WaveSummary summary) {
            string inputCasePath = Path.Combine(topLevelInputDir, WBP);
            string inputDeathPath = Path.Combine(topLevelInputDir, WBP);
            string outputPath = Path.Combine(topLevelOutputDir, subdirectory, "Countries");
            Summaries(config.CountriesWithProvincesList, inputCasePath, inputDeathPath, outputPath, summary);

        }

        private void StateSummaries(string subdirectory, WaveSummary summary) {
            string inputCasePath = Path.Combine(topLevelInputDir, USBC_C);
            string inputDeathPath = Path.Combine(topLevelInputDir, USBC_D);
            string outputPath = Path.Combine(topLevelOutputDir, subdirectory, "States");
            Summaries(config.UsStatesList, inputCasePath, inputDeathPath, outputPath, summary);

        }

        private void Summaries(List<string> regionList, string inputCasePath, string inputDeathPath, string outputPath, WaveSummary summary) {
            foreach (string region in regionList) {
                Summary(region, inputCasePath, inputDeathPath, outputPath, summary);
            }
        }

        private void Summary(string region, string inputCasePath, string inputDeathPath, string outputPath, WaveSummary summary) {
            string confFilePath = Path.Combine(inputCasePath, region + "_confirmed_sm.csv");
            string deathFilePath = Path.Combine(inputDeathPath, region + "_deaths_sm.csv");
            string outputFilePath = Path.Combine(outputPath,region + "_summary.csv");

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
