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
using ScottPlot;
using DataSeries;



namespace CaseCounter {
    /// <summary>
    /// Interaction logic for ChartWindow.xaml
    /// </summary>
    public partial class ChartWindow : Window {
        public ChartWindow(List<TimeSeries> tsList) : base() {
            InitializeComponent();
            PlotSeries(tsList);
        }

        void PlotSeries(List<TimeSeries> tsList) {


            foreach (TimeSeries ts in tsList) {

                double[] dataX = new double[ts.LastDay + 1];
                for (int i = 0; i < dataX.Length; i++) {
                    dataX[i] = i;
                }
                double[] dataY = ts.GetData();


                ScottPlot.Plottable.ScatterPlot curve = wpfPlot1.Plot.AddScatter(dataX, dataY);
                curve.Label = ts.Key;     


            }

            wpfPlot1.Plot.XAxis.Label("Days since 1-22-20");
            wpfPlot1.Plot.YAxis.Label("Reported cases or deaths");

            wpfPlot1.Plot.Legend(location: Alignment.UpperLeft);
            wpfPlot1.Refresh();

        }
    }
}
