using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.IO;
using DataSeries;
using Utilities;


namespace CaseCounter {
    public class PeakExporter : TaskScript {



        private delegate void ResultWriter(TimeSeriesSet tss, string path);

        private static string USBC = "United States by county";
        private static string WBC = "World by country";
        private static string WBP = "World by province";

        private string[] subDirectoryList = { "Peaks3", "Peaks4", "Peaks5"};
        private ResultWriter[] resultWriters = { (tss, path) => ExportPeaks(tss, path, 3), (tss, path) => ExportPeaks(tss, path, 4), (tss, path) => ExportPeaks(tss, path, 5) };
        private string[] tssFiles = { USBC + "\\Confirmed\\Oregon_confirmed_sm.csv", USBC + "\\Confirmed\\Washington_confirmed_sm.csv",
                                    USBC + "\\Confirmed\\Arizona_confirmed_sm.csv", WBC + "\\Africa_confirmed_sm.csv", WBC + "\\Europe_confirmed_sm.csv",
                                    WBC + "\\Asia_confirmed_sm.csv", WBP + "\\Canada_confirmed_sm.csv", WBP + "\\India_confirmed_sm.csv", 
                                    WBP + "\\Russia_confirmed_sm.csv", WBP + "\\US_confirmed_sm.csv"};
        private string[] outputFileNames = { "Oregon.csv", "Washington.csv", "Arizona.csv", "Africa.csv", "Europe.csv", "Asia.csv", "Canada.csv", "India.csv", "Russia.csv", "UnitedStates.csv" };

        public PeakExporter(ListBox listBox) : base(listBox) {

        }

        static void ExportPeaks(TimeSeriesSet tss, string filePath, int nPeaks) {
            PeakSetCollection psc = tss.FindPeaks(nPeaks);
            psc.WriteToFile(filePath);
        }

        public override bool Build() {
            if (subDirectoryList.Length != resultWriters.Length) {
                throw new ProgrammingException("directoryList and resultWriters need to be the same length"); 
            }
            if (tssFiles.Length != outputFileNames.Length) {
                throw new ProgrammingException("tssFiles and outputFileNames need to be the same length");
            }

            ReportStep("Build Peak Files");
            bool error;


            string topLevelInputDir = GetTopLevelInputDirectory(out error);

            if (error) {
                return false;
            }

            ReportStep("Get Input Directory");

            string topLevelOutputDir = CreateOutputDirectories(subDirectoryList, out error);

            if (error) {
                return false;
            }

            ReportStep("Create Output Directories");

            for (int i = 0; i < tssFiles.Length; i++) {
                MakePeaks(Path.Combine(topLevelInputDir, tssFiles[i]), outputFileNames[i], topLevelOutputDir);
                ReportStep("Exported " + outputFileNames[i]);
            }

            return true;
        }


        private void MakePeaks(string tssFilePath, string fileName, string topLevelOutputDir) {
            TimeSeriesSet tss = new();
            tss.LoadCsv(tssFilePath);
 
            for (int i = 0; i < resultWriters.Length; i++) {
                ResultWriter resultWriter = resultWriters[i];
                string subDirectory = subDirectoryList[i];
                string filePath = Path.Combine(topLevelOutputDir, subDirectory, fileName);
                resultWriter(tss, filePath);
            }
        }
    }
}
