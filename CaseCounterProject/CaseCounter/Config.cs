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

        public Config() {
            UsStatesList = US_states.ToList();

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
                                                    "Russian Federation"};

        // Pairs of substitutions for Admin0.  Length must be even
        private readonly string[] Admin1Remove = { "Australia", "From Diamond Princess", "US", "Wuhan Evacuee", "Canada", "Diamond Princess", "Canada", "Toronto, ON",
                                                    "Canada", "Recovered", "Canada", "Grand Princess", "Canada", "Montreal, QC"};

        // Pairs of substitutions for Admin0.  Length must be even
        private readonly string[] Admin0Subs = { "Mainland China", "China", "Palestine", "West Bank and Gaza", "The Gambia", "Gambia",
                                                 "Viet Nam", "Vietnam", "Iran (Islamic Republic of)", "Iran", "Gambia, The", "Gambia",
                                                 "Taiwan*", "Taiwan", "Czechia", "Czech Republic", "Holy See", "Vatican City",
                                                 "Republic of Korea", "South Korea", "Korea, South", "South Korea"};

        // Quadruples of substituions for Admin1.  Length must be divisible by four
        private readonly string[] Admin1Subs = { "Hong Kong", "Hong Kong", "China", "Hong Kong", "Hong Kong SAR", "Hong Kong", "China", "Hong Kong" };

        private readonly string[] US_states = {"Alabama", "Alaska", "Arizona", "Arkansas", "California", "Colorado", "Connecticut", "Delaware", "District of Columbia",
                                                "Florida", "Georgia","Hawaii","Idaho","Illinois","Indiana","Iowa","Kansas","Kentucky","Louisiana","Maine","Maryland",
                                                "Massachusetts", "Michigan", "Minnesota", "Mississippi", "Missouri", "Montana", "Nebraska", "Nevada", "New Hampshire",
                                                "New Jersey", "New Mexico","New York","North Carolina","North Dakota","Ohio","Oklahoma","Oregon","Pennsylvania",
                                                "Puerto Rico", "Rhode Island", "South Carolina", "South Dakota", "Tennessee", "Texas", "Utah", "Vermont", "Virginia",
                                                "Washington", "West Virginia","Wisconsin","Wyoming" };
    }
}



