using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataSeries;

namespace WaveAnalyzer {
    public class Wave {
        public TimeSeries timeSeries { get; }


        public Wave(TimeSeries ts) {
            timeSeries = ts;
        }
    }
}
