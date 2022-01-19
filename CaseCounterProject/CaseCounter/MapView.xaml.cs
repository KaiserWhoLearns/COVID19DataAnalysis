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

namespace CaseCounter {
    /// <summary>
    /// Interaction logic for MapView.xaml
    /// </summary>
    public partial class MapView : Window {

        // Application loads content from a served directory.  For now, the URLS are just hardcoded into the application.  Two options are under consideration,
        // using a local server and local host, (For now "python -m SimpleHTTPServer 8888"),  and using a directory in my department home webspace.


        // Local server
        private readonly string webDirectory = "C:\\Users\\anderson\\Documents\\CC-web-home";
       private readonly string localURL = "http://localhost:8888";

        // CSE Home ddirectory

        //   private readonly string webDirectory = "O:\\cse\\web\\homes\\anderson\\b";     // Hard coded to a fixed directory
        //   private readonly string localURL = "https://homes.cs.washington.edu/~anderson/b/";

        TimeSeriesSet timeSeriesSet;

        public MapView(TimeSeriesSet tss) {
            InitializeComponent();
            webView.CoreWebView2InitializationCompleted += Webview_InitializationComplete;

            timeSeriesSet = tss;
            int lastDay = tss.LastDay();
            startDaySlider.Maximum = lastDay;
            endDaySlider.Maximum = lastDay;
            endDaySlider.Value = lastDay;
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
            LoadHtml("d3_us_states.html");
        }

        private readonly string[] US_states = {"Alabama", "Alaska", "Arizona", "Arkansas", "California", "Colorado", "Connecticut", "Delaware", "District of Columbia",
                                                "Florida", "Georgia","Hawaii","Idaho","Illinois","Indiana","Iowa","Kansas","Kentucky","Louisiana","Maine","Maryland",
                                                "Massachusetts", "Michigan", "Minnesota", "Mississippi", "Missouri", "Montana", "Nebraska", "Nevada", "New Hampshire",
                                                "New Jersey", "New Mexico","New York","North Carolina","North Dakota","Ohio","Oklahoma","Oregon","Pennsylvania",
                                                "Puerto Rico", "Rhode Island", "South Carolina", "South Dakota", "Tennessee", "Texas", "Utah", "Vermont", "Virginia",
                                                "Washington", "West Virginia","Wisconsin","Wyoming" };
        private void WriteRandom() {
            Random rand = new();
            StringBuilder sb = new();
            sb.AppendLine("state,value");
            foreach (string str in US_states) {
                sb.AppendLine($"{str},{rand.NextDouble()}");
            }

            string fileString = sb.ToString();
            string fileName = "data/random.csv";
            using (StreamWriter outputFile = new(Path.Combine(webDirectory, fileName))) {
                outputFile.Write(fileString);
            }


        }

        private void LoadUS_Click(object sender, RoutedEventArgs e) {
            CopyHtmlToWebDirectory("d3_us_states.html");
            LoadHtml("d3_us_states.html");
        }

        private void Random_Click(object sender, RoutedEventArgs e) {
            WriteRandom();
        }

        private void Update_Click(object sender, RoutedEventArgs e) {
            string fileString = timeSeriesSet.NormalizedCaseCount((int)startDaySlider.Value, (int)endDaySlider.Value);
            string fileName = "data/us_confirmed.csv";
            using (StreamWriter outputFile = new(Path.Combine(webDirectory, fileName))) {
                outputFile.Write(fileString);
            }
        }
    }
}
