using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Web.WebView2.Core;
using System.IO;
using System.Reflection;
using DataSeries;
using Utilities;

namespace CaseCounter {
    /// <summary>
    /// Interaction logic for MapView.xaml
    /// </summary>
    public partial class MapView : Window {

        // Application loads content from a served directory.  For now, the URLS are just hardcoded into the application.  Two options are under consideration,
        // using a local server and local host, (For now "python -m SimpleHTTPServer 8888"),  and using a directory in my department home webspace.


        // Local server
       // private readonly string webDirectory = "C:\\Users\\anderson\\Documents\\CC-web-home";
       //private readonly string localURL = "http://localhost:8888";

        // CSE Home ddirectory

           private readonly string webDirectory = "O:\\cse\\web\\homes\\anderson\\b";     // Hard coded to a fixed directory
           private readonly string localURL = "https://homes.cs.washington.edu/~anderson/b/";
/*
        private readonly string[] US_states = {"Alabama", "Alaska", "Arizona", "Arkansas", "California", "Colorado", "Connecticut", "Delaware", "District of Columbia",
                                                "Florida", "Georgia","Hawaii","Idaho","Illinois","Indiana","Iowa","Kansas","Kentucky","Louisiana","Maine","Maryland",
                                                "Massachusetts", "Michigan", "Minnesota", "Mississippi", "Missouri", "Montana", "Nebraska", "Nevada", "New Hampshire",
                                                "New Jersey", "New Mexico","New York","North Carolina","North Dakota","Ohio","Oklahoma","Oregon","Pennsylvania",
                                                "Puerto Rico", "Rhode Island", "South Carolina", "South Dakota", "Tennessee", "Texas", "Utah", "Vermont", "Virginia",
                                                "Washington", "West Virginia","Wisconsin","Wyoming" };

        // Excluding island countries
        private readonly string[] Continental_African_countries = { "Algeria", "Angola", "Benin", "Botswana", "Burkina Faso", "Burundi", 
                                                "Cameroon", "Central African Republic", "Chad",  "Congo (Brazzaville)",
                                                 "Congo (Kinshasa)", "Cote d'Ivoire", "Djibouti", "Egypt", "Equatorial Guinea", "Eritrea",
                                                "Eswatini", "Ethiopia", "Gabon", "Gambia", "Ghana", "Guinea", "Guinea-Bissau",
                                                "Kenya", "Lesotho", "Liberia", "Libya", "Madagascar", "Malawi", "Mali", "Mauritania", 
                                                "Morocco", "Mozambique", "Namibia", "Niger", "Nigeria", "Rwanda",
                                                "Senegal",  "Sierra Leone", "Somalia", "South Africa", "South Sudan",
                                                "Sudan", "Tanzania", "Togo", "Tunisia", "Uganda", "Zambia", "Zimbabwe" };
 */ 

        TimeSeriesSet timeSeriesSet;
        string mapName;
        string csvFileName;
        string d3FileName;
        List<string> regionNames;
        TimeSeriesSet.RegionString regionSelector;
        Config config;
        Random random;

        public MapView(string mapName, TimeSeriesSet tss) {
            InitializeComponent();
            webView.CoreWebView2InitializationCompleted += Webview_InitializationComplete;

            random = new();
            config = new();
            timeSeriesSet = tss;
            this.mapName = mapName;

            int lastDay = tss.LastDay();
            startDaySlider.Maximum = lastDay;
            spanSlider.Maximum = lastDay;
            spanSlider.Value = lastDay;
            mapNameLabel.Content = mapName;

            switch (mapName) {
                case "Africa":
                    csvFileName = "data/africa_confirmed.csv";
                    d3FileName = "d3_africa.html";
                    regionNames = config.ContinentalAfricaList;
                    regionSelector = Admin0Selector;
                    break;
                case "Africa2":
                    csvFileName = "data/africa_confirmed.csv";
                    d3FileName = "d3_africa2.html";
                    regionNames = config.ContinentalAfricaList;
                    regionSelector = Admin0Selector;
                    break;
                case "UnitedStates":
                    csvFileName = "data/us_confirmed.csv";
                    d3FileName = "d3_us_states.html";
                    regionNames = config.UsStatesList;
                    regionSelector = Admin1Selector;
                    break;
                case "UnitedStates2":
                    csvFileName = "data/us_confirmed.csv";
                    d3FileName = "d3_us2.html";
                    regionNames = config.UsStatesList;
                    regionSelector = Admin1Selector;
                    break;
                case "India":
                    csvFileName = "data/india_confirmed.csv";
                    d3FileName = "d3_india_states.html";
                    regionNames = config.IndiaStatesList;
                    regionSelector = Admin1Selector;
                    break;
                default:
                    throw new ProgrammingException("Unexpected map type in MapView");
            }


        }

        private void Webview_InitializationComplete(object sender, EventArgs e) {
            CopyHtmlToWebDirectory("index.html");
            LoadHtml("index.html");
        }


        // Copy a file to the local directory.  Files are assumed to be Embedded Resources
        private void CopyHtmlToWebDirectory(string fileName) {
            Assembly assembly = Assembly.GetExecutingAssembly();
            StreamReader reader = new StreamReader(assembly.GetManifestResourceStream($"CaseCounter.D3Files.{fileName}"));
            string fileString = reader.ReadToEnd();

            using (StreamWriter outputFile = new(Path.Combine(webDirectory, fileName))) {
                outputFile.Write(fileString);
            }
        }

        private void LoadHtml(string fileName) {
            if (webView != null && webView.CoreWebView2 != null) {
                webView.CoreWebView2.Navigate(Path.Combine(localURL, fileName));
            }
        }



        private void Refresh_Click(object sender, RoutedEventArgs e) {
            WriteRandom();
        }


        private void WriteRandom() {

            StringBuilder sb = new();
            sb.AppendLine("region,value");
            foreach (string str in regionNames) {
                sb.AppendLine($"{str},{random.NextDouble():F5}");
            }

            WriteCsv(sb.ToString());
        }

        public string Admin0Selector(TimeSeries ts) {
            return ts.Admin0;
        }

        public string Admin1Selector(TimeSeries ts) {
            return ts.Admin1;
        }


        private void Random_Click(object sender, RoutedEventArgs e) {
            WriteRandom();
        }

        private void Update_Click(object sender, RoutedEventArgs e) {
            int startDay = (int)startDaySlider.Value;
            int endDay = Math.Min(startDay + (int)spanSlider.Value, timeSeriesSet.LastDay());
            string fileString = timeSeriesSet.MapValues(regionSelector, startDay, endDay);
            WriteCsv(fileString);
        }

        private void WriteCsv(string fileString) {
            string fileName = csvFileName;
            using (StreamWriter outputFile = new(Path.Combine(webDirectory, fileName))) {
                outputFile.Write(fileString);
            }
        }

        private void LoadMap_Click(object sender, RoutedEventArgs e) {
            CopyHtmlToWebDirectory(d3FileName);
            LoadHtml(d3FileName);
        }

        private void Foo_Click(object sender, RoutedEventArgs e) {
            webView.ExecuteScriptAsync("foo()");
        }
    }
}
