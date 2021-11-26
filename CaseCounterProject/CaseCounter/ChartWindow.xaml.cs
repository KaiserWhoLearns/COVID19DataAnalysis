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
        public ChartWindow(List<(TimeSeries,int)> tsList) : base() {
            InitializeComponent();
            PlotSeries(tsList);
        }

        void PlotSeries(List<(TimeSeries,int)> tsList) {

            foreach ((TimeSeries ts, int axisIndex) in tsList) {

                double[] dataX = new double[ts.LastDay + 1];
                for (int i = 0; i < dataX.Length; i++) {
                    dataX[i] = i;
                }
                double[] dataY = ts.GetData();


                ScottPlot.Plottable.ScatterPlot curve = wpfPlot1.Plot.AddScatter(dataX, dataY);
                curve.YAxisIndex = axisIndex;
                curve.Label = ts.Key;

            }

            wpfPlot1.Plot.XAxis.Label("Days since 1-22-20");
            wpfPlot1.Plot.YAxis.Label("Reported cases or deaths");

            wpfPlot1.Plot.YAxis2.Ticks(true);

            wpfPlot1.Plot.Legend(location: Alignment.UpperLeft);
            wpfPlot1.Refresh();

        }
    }
}
