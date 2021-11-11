using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;
using System.Text;
using System.Threading.Tasks;

namespace CaseCounter {

    // Static tables the could be configuration files
    public class Config {

        public List<string> Admin0RemoveList { get; }

        public List<(string, string)> Admin1RemoveList { get; }

        public Dictionary<string, string> Admin0Substitutions { get; }

        public Dictionary<(string, string), (string, string)> Admin1Substitutions { get; }

        public List<string> UsStatesList { get; }

        public List<string> AfricaList { get; }
        public List<string> EuropeList { get; }
        public List<string> AsiaList { get; }
        public List<string> NorthAmericaList { get; }
        public List<string> SoutAmericaList { get; }


        public Config() {
            UsStatesList = US_states.ToList();
            AfricaList = African_countries.ToList();
            EuropeList = European_countries.ToList();
            AsiaList = Asian_countries.ToList();
            NorthAmericaList = NorthAmerican_countries.ToList();
            SoutAmericaList = SouthAmerican_countries.ToList();

            Admin0RemoveList = Admin0Remove.ToList();

            Admin1RemoveList = new();
            if (Admin1Remove.Length % 2 != 0) {
                throw new ProgrammingException("Admin1Remove needs to be even length");
            }
            for (int i = 0; i < Admin1Remove.Length; i += 2) {
                Admin1RemoveList.Add((Admin1Remove[i], Admin1Remove[i + 1]));
            }

            Admin0Substitutions = new();
            if (Admin0Subs.Length % 2 != 0) {
                throw new ProgrammingException("Admin0Subs needs to be even length");
            }
            for (int i = 0; i < Admin0Subs.Length; i += 2) {
                Admin0Substitutions.Add(Admin0Subs[i], Admin0Subs[i + 1]);
            }

            Admin1Substitutions = new();
            if (Admin1Subs.Length % 4 != 0) {
                throw new ProgrammingException("Admin1Subs needs to be even length");
            }
            for (int i = 0; i < Admin1Subs.Length; i += 4) {
                Admin1Substitutions.Add((Admin1Subs[i], Admin1Subs[i + 1]), (Admin1Subs[i + 2], Admin1Subs[i + 3]));
            }

        }

        private readonly string[] Admin0Remove = { "Cruise Ship", "Summer Olympics 2020", "Others", "Taipei and environs", "occupied Palestinian territory",
                                                    "MS Zaandam", " Azerbaijan", "The Bahamas", "Bahamas, The", "Channel Islands", "Diamond Princess",
                                                    "Russian Federation", "St. Martin", "Saint Martine", "Republic of Moldova", "Republic of the Congo", "Cape Verde", 
                                                    "Greenland", "Macau", "Ivory Coast", "North Ireland", "Gibraltar", "UK"};

        // Pairs of substitutions for Admin0.  Length must be even
        private readonly string[] Admin1Remove = { "Australia", "From Diamond Princess", "US", "Wuhan Evacuee", "Canada", "Diamond Princess", "Canada", "Toronto, ON",
                                                    "Canada", "Recovered", "Canada", "Grand Princess", "Canada", "Montreal, QC", "United Kingdom", "United Kingdom",
                                                    "United Kingdom", "UK", "United Kingdom", "Unknown", "Israel", "From Diamond Princess", "Canada", " Montreal, QC",
                                                    "Canada", "London, ON"};

        // Pairs of substitutions for Admin0.  Length must be even
        private readonly string[] Admin0Subs = { "Mainland China", "China", "Palestine", "West Bank and Gaza", "The Gambia", "Gambia",
                                                 "Viet Nam", "Vietnam", "Iran (Islamic Republic of)", "Iran", "Gambia, The", "Gambia",
                                                 "Taiwan*", "Taiwan", "Czechia", "Czech Republic", "Holy See", "Vatican City",
                                                 "Republic of Korea", "South Korea", "Korea, South", "South Korea", "Republic of Ireland", "Ireland"};

        // Quadruples of substituions for Admin1.  Length must be divisible by four
        private readonly string[] Admin1Subs = { "Hong Kong", "Hong Kong", "China", "Hong Kong", "Hong Kong SAR", "Hong Kong", "China", "Hong Kong", "Reunion", "", "France", "Reunion",
                                                "Macao SAR", "Macau", "China", "Macau", "Faroe Islands", "", "Denmark", "Faroe Islands", "United Kingdom", "Falkland Islands (Malvinas)",
                                                "United Kingdom", "Falkland Islands", "United Kingdom", "Falkland Islands (Islas Malvinas)", "United Kingdom", "Falkland Islands",
                                                "Cayman Islands", "", "United Kingdom", "Cayman Islands"};

        private readonly string[] US_states = {"Alabama", "Alaska", "Arizona", "Arkansas", "California", "Colorado", "Connecticut", "Delaware", "District of Columbia",
                                                "Florida", "Georgia","Hawaii","Idaho","Illinois","Indiana","Iowa","Kansas","Kentucky","Louisiana","Maine","Maryland",
                                                "Massachusetts", "Michigan", "Minnesota", "Mississippi", "Missouri", "Montana", "Nebraska", "Nevada", "New Hampshire",
                                                "New Jersey", "New Mexico","New York","North Carolina","North Dakota","Ohio","Oklahoma","Oregon","Pennsylvania",
                                                "Puerto Rico", "Rhode Island", "South Carolina", "South Dakota", "Tennessee", "Texas", "Utah", "Vermont", "Virginia",
                                                "Washington", "West Virginia","Wisconsin","Wyoming" };

        private readonly string[] African_countries = { "Algeria", "Angola", "Benin", "Botswana", "Burkina Faso", "Burundi", "Cabo Verde",
                                                "Cameroon", "Central African Republic", "Chad", "Comoros", "Congo (Brazzaville)",
                                                 "Congo (Kinshasa)", "Cote d'Ivoire", "Djibouti", "Equatorial Guinea", "Eritrea",
                                                   "Eswatini", "Ethiopia", "Gabon", "Gambia", "Ghana", "Guinea", "Guinea-Bissau",
                                                "Kenya", "Lesotho", "Madagascar", "Malawi", "Mali", "Mauritania", "Mauritius",
                                                "Morocco", "Mozambique", "Namibia", "Niger", "Nigeria", "Rwanda", "Sao Tome and Principe",
                                                "Senegal", "Seychelles", "Sierra Leone", "Somalia", "South Africa", "South Sudan",
                                                "Sudan", "Tanzania", "Togo", "Tunisia", "Uganda", "Zambia", "Zimbabwe" };

        private readonly string[] European_countries = {"Albania","Andorra","Austria","Belarus","Belgium","Bosnia and Herzegovina","Bulgaria",
                                                "Croatia","Cyprus","Czech Republic","Denmark","Estonia","Finland","France","Germany",
                                                "Greece","Iceland","Ireland","Italy","Kosovo","Latvia","Liechtenstein","Lithuania",
                                                "Luxembourg","Moldova","Monaco","Montenegro","Netherlands","Norway","Poland","Portugal",
                                                "Romania","Russia","San Marino","Serbia","Slovakia","Slovenia","Spain","Sweden","Switzerland",
                                                "Turkey","Ukraine","United Kingdom","Vatican City" };

        private readonly string[] Asian_countries = {};

        private readonly string[] NorthAmerican_countries = { };

        private readonly string[] SouthAmerican_countries = { };


    }
}



