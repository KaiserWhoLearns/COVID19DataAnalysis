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
using System.Windows.Shapes;
using DrawingManager;
using DataSeries; 

namespace CaseCounter {
    /// <summary>
    /// Interaction logic for DrawingWindow.xaml
    /// </summary>
    public partial class DrawingWindow : Window {

        private ScatterPlot scatterPlot;

        public DrawingWindow(List<TimeSeries> tsList) {
            InitializeComponent();
            PlotSeries(tsList);
        }

        // meaningless comment
        private static List<Point> LatLongList(List<TimeSeries> tsList) {
            List<Point> pointList = new();

            foreach (TimeSeries ts in tsList) {
                pointList.Add(new(ts.Longitude, ts.Latitude));
            }
            return pointList;
        }

        public void PlotSeries(List<TimeSeries> tsList) {
            scatterPlot = new(drawingCanvas);
            drawingCanvas.Background = Brushes.BlanchedAlmond;

            List<Point> pointList = LatLongList(tsList);
            scatterPlot.PlotPoints(pointList, 10);
        }
    }
}
