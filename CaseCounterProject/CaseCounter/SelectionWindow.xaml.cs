using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DataSeries;

namespace CaseCounter {
    /// <summary>
    /// Interaction logic for SelectionWindow.xaml
    /// </summary>
    public partial class SelectionWindow : Window {
        public SelectionWindow() {
            InitializeComponent();
            Initialize();
        }

        private void Initialize() {
            TimeSeriesSet ts = ((MainWindow)Application.Current.MainWindow).TimeSeriesSet;

        }
        private void SelectButtonClick(object sender, RoutedEventArgs e) {

        }
    }
}
