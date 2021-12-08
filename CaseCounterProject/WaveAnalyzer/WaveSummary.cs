using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataSeries;

namespace WaveAnalyzer {


    public abstract class WaveSummary {
        public WaveSummary() {

        }

        public abstract string HeaderString(int waves = 0);

        public abstract string Summarize(TimeSeries caseSeries, TimeSeries deathSeries, int waves = 0);


        protected string SummaryHeader() {
            return "Admin0,Admin1,Admin2,FIPS,Population,CaseCount,DeathCount,CasesPer100K,DeathsPerM,CFR";
        }
        protected string SummaryData(TimeSeries caseSeries, TimeSeries deathSeries) {
            StringBuilder sb = new();
            sb.Append($"\"{caseSeries.Admin0}\",\"{caseSeries.Admin1}\",\"{caseSeries.Admin2}\",{caseSeries.Fips},");

            double population = caseSeries.Population;
            double caseCount = caseSeries.CaseCount();
            double deathCount = (deathSeries != null) ? deathSeries.CaseCount() : 0;
            double casesPer100K = (population > 0) ? caseCount * 100000 / population : 0;
            double deathsPer1M = (population > 0) ? deathCount * 1000000 / population : 0;
            double caseFatalityRate = (caseCount > 0) ? deathCount / caseCount : 0;

            sb.Append($"{population:F0},{caseCount:F0},{deathCount:F0},");
            sb.Append($"{casesPer100K:F2},{deathsPer1M:F2},{caseFatalityRate:F4}");

            return sb.ToString();
        }
    }

    public class BasicSummary : WaveSummary {
        public BasicSummary() {

        }

        public override string HeaderString(int waves = 0) {
            return SummaryHeader();
        }

        public override string Summarize(TimeSeries caseSeries, TimeSeries deathSeries, int waves = 0) {
            return SummaryData(caseSeries, deathSeries);
        }
    }

    public class TopWavesSummary : WaveSummary {
        public TopWavesSummary() {

        }

        public override string HeaderString(int waves = 0) {
            StringBuilder sb = new();
            sb.Append(SummaryHeader());

            return sb.ToString();
        }

        public override string Summarize(TimeSeries caseSeries, TimeSeries deathSeries, int waves = 0) {
            StringBuilder sb = new();
            sb.Append(SummaryData(caseSeries, deathSeries));



            return sb.ToString();
        }
    }
}
