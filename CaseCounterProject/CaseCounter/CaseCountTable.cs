using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Windows.Controls;
using Utilities;
using DataSeries;


namespace CaseCounter {
    // Case Count table is storing information for a single day across a set of locations
    public class CaseCountTable {

        List<JhuCaseCountRecord> cases;
        public DateTime Date { get; }           // Date extracted from filename
        public int Days { get; }                 // Number of days since 01-22-2020,  start date of JHU data


        public string FileName { get; }

        public CaseCountTable() {
            cases = new List<JhuCaseCountRecord>();
        }

        // Load data from file - we only open the file if its name is a valid date and a csv file
        public CaseCountTable(string filePath) : this() {

            if (ValidFileName(filePath, out DateTime dateTime, out int days) == false) {
                return;
            }

            FileName = Util.ExtractFilename(filePath);

            Date = dateTime;
            Days = days;

            StreamReader reader = new(filePath);
            CsvReader csvReader = new(reader, CultureInfo.InvariantCulture);
 
            _ = csvReader.Context.RegisterClassMap<JhuClassMap>();
            cases = csvReader.GetRecords<JhuCaseCountRecord>().ToList();

            for (int i = 0; i < cases.Count; i++) {     // Remove any null values
                cases[i] = cases[i].Clean();
            }

        }


        public void ToListBox(ListBox listBox) {
            listBox.Items.Clear();
            foreach (JhuCaseCountRecord ccr in cases) {
                _ = listBox.Items.Add(ccr.ToString());
            }
        }

        // File names are of the form YYYY-MM-DD.csv starting from 2020-1-22
        public static bool ValidFileName(string filePath, out DateTime dateTime, out int days) {

            dateTime = new();
            days = 0;

            string[] path = filePath.Split('\\', '.');
            if (path.Length < 2) {
                return false;
            }

            string fileName = path[^2];

            if (DateTime.TryParse(fileName, out dateTime)) {
                DateTime first = new(2020, 1, 22);
                days = (int)dateTime.Subtract(first).TotalDays;
                return days >= 0;       // Need to watch out for the case of a date before the start
            } else {
                return false;
            }
        }

        public void AddToTSS(TimeSeriesSet tss) {
            /*
             * for each location
             *     AddToCount TS
             *     AddToDeath TS
             */

            foreach (JhuCaseCountRecord jccr in cases) {
                tss.AddConfirmed(jccr.Country, jccr.Province, jccr.District, jccr.Confirmed, Days);
                tss.AddDeaths(jccr.Country, jccr.Province, jccr.District, jccr.Deaths, Days);
            }

        }

        public void Cleanup(Config config) {
            List<JhuCaseCountRecord> jList = new();
            foreach (JhuCaseCountRecord jccr in cases) {
                if (! RemoveRecord(jccr, config)) {
                    UpdateRecord(jccr, config);
                    jList.Add(jccr);
                }
            }

            cases = jList;
        }

        private bool RemoveRecord(JhuCaseCountRecord jccr, Config config) {
            if (config.Admin0RemoveList.Contains(jccr.Country))
                return true;
            return false;
        }

        private void UpdateRecord(JhuCaseCountRecord jccr, Config config) {
            if (config.Admin0Substitutions.ContainsKey(jccr.Country)) {
                jccr.Country = config.Admin0Substitutions[jccr.Country];
            }
        }
    }

    public class JhuCaseCountRecord {

        public int? FIPS { get; set; }
        public string District { get; set; }
        public string Province { get; set; }
        public string Country { get; set; }
        public string LastUpdate { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int? Confirmed { get; set; }
        public int? Deaths { get; set; }
        public string Recovered { get; set; }               // Changed to string type as there are data errors on reading  (values given with decimal point, e.g, 7.0 insted of 7
        public int? Active { get; set; }
        public string FullName { get; set; }
        public double? IncidentRate { get; set; }
        public string CaseFatalityRatio { get; set; }   // Changed to string type as there are data errors on reading  (cells with divide by zero text)
                                                            

        public override string ToString() {
            string str = "[" + District + ", " + Province + ", " + Country + ", " + LastUpdate + ", " + Confirmed + ", " + Deaths +  "]";
            return str;
        }

        public JhuCaseCountRecord Clean() {
            JhuCaseCountRecord ccr = new();

            ccr.FIPS = FIPS ?? 0;
            ccr.District = District;
            ccr.Province = Province;
            ccr.Country = Country;
            ccr.LastUpdate = LastUpdate;
            ccr.Latitude = Latitude ?? 0.0;
            ccr.Longitude = Longitude ?? 0.0;
            ccr.Confirmed = Confirmed ?? 0;
            ccr.Deaths = Deaths ?? 0;
            ccr.Recovered = Recovered;
            ccr.Active = Active ?? 0;
            ccr.FullName = FullName;
            ccr.IncidentRate = IncidentRate ?? 0.0;
            ccr.CaseFatalityRatio = CaseFatalityRatio;
            return ccr;
        }

    }

    public class JhuClassMap : ClassMap<JhuCaseCountRecord> {
        public JhuClassMap() {
            Map(m => m.FIPS).Name("FIPS").Optional();
            Map(m => m.District).Name("Admin2").Optional();
            Map(m => m.Province).Name("Province/State", "Province_State").Optional();
            Map(m => m.Country).Name("Country/Region", "Country_Region").Optional();
            Map(m => m.LastUpdate).Name("Last Update", "Last_Update").Optional();
            Map(m => m.Latitude).Name("Latitude", "Lat").Optional();
            Map(m => m.Longitude).Name("Longitude", "Long_").Optional();
            Map(m => m.Confirmed).Name("Confirmed").Optional();
            Map(m => m.Deaths).Name("Deaths").Optional();
            Map(m => m.Recovered).Name("Recovered").Optional();
            Map(m => m.Active).Name("Active").Optional();
            Map(m => m.FullName).Name("Combined_Key").Optional();
            Map(m => m.IncidentRate).Name("Incident_Rate").Optional();
  //          Map(m => m.CaseFatalityRatio).Name("Case_Fatality_Ratio").Optional();
        }
    }
}
