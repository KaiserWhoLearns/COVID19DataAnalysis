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


        public List<(string, string, string)> Admin2RemoveList { get; }

        public Dictionary<string, string> Admin0Substitutions { get; }

        public Dictionary<(string, string), (string, string)> Admin1Substitutions { get; }

        public List<(string, int)> Admin0AnomalyList { get; }
        public List<(string, int)> Admin0StarAnomalyList { get; }
        public List<(string, string, int)> Admin1StarAnomalyList { get; }
        public List<(string, string, string, int)> Admin2AnomalyList { get; }

        public List<string> UsStatesList { get; }
        public List<string> UsLowerFortyEightList { get; }
        public List<string> UsSouthList { get; }
        public List<string> UsNorthEastList { get; }
        public List<string> UsMidWestList { get; }
        public List<string> UsWestList { get; }

        public List<string> AfricaList { get; }
        public List<string> EuropeList { get; }
        public List<string> AsiaList { get; }
        public List<string> NorthAmericaList { get; }
        public List<string> SouthAmericaList { get; }
        public List<string> ContinentList { get; }
        public List<string> CountriesWithProvincesList { get; }
        public List<string> ContinentalAfricaList { get; }
        public List<string> IndiaStatesList { get; }



        public Config() {
            UsStatesList = US_states.ToList();
            UsLowerFortyEightList = US_lower48.ToList();
            UsNorthEastList = US_northeast.ToList();
            UsMidWestList = US_midwest.ToList();
            UsSouthList = US_south.ToList();
            UsWestList = US_west.ToList();

            AfricaList = African_countries.ToList();
            ContinentalAfricaList = Continental_African_countries.ToList();
            EuropeList = European_countries.ToList();
            AsiaList = Asian_countries.ToList();
            NorthAmericaList = NorthAmerican_countries.ToList();
            SouthAmericaList = SouthAmerican_countries.ToList();
            CountriesWithProvincesList = CountriesWithProvinces.ToList();
            ContinentList = Continents.ToList();
            IndiaStatesList = India_states.ToList();

            Admin0RemoveList = Admin0Remove.ToList();

            Admin1RemoveList = new();
            if (Admin1Remove.Length % 2 != 0) {
                throw new ProgrammingException("Admin1Remove needs to be even length");
            }
            for (int i = 0; i < Admin1Remove.Length; i += 2) {
                Admin1RemoveList.Add((Admin1Remove[i], Admin1Remove[i + 1]));
            }

            Admin2RemoveList = new();
            if (Admin2Remove.Length % 3 != 0) {
                throw new ProgrammingException("Admin2Remove length needs to be divisible by three");
            }
            for (int i = 0; i < Admin2Remove.Length; i += 3) {
                Admin2RemoveList.Add((Admin2Remove[i], Admin2Remove[i + 1], Admin2Remove[i + 2]));
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
                throw new ProgrammingException("Admin1Subs needs length needs to be congruent to zero mod four");
            }
            for (int i = 0; i < Admin1Subs.Length; i += 4) {
                Admin1Substitutions.Add((Admin1Subs[i], Admin1Subs[i + 1]), (Admin1Subs[i + 2], Admin1Subs[i + 3]));
            }

            Admin0AnomalyList = new();
            if (Admin0Anomalies.Length % 2 != 0) {
                throw new ProgrammingException("Admin0Anomalies needs to be even length");
            }
            for (int i = 0; i < Admin0Anomalies.Length; i += 2) {
                Admin0AnomalyList.Add((Admin0Anomalies[i], int.Parse(Admin0Anomalies[i + 1])));
            }

            Admin0StarAnomalyList = new();
            if (Admin0StarAnomalies.Length % 2 != 0) {
                throw new ProgrammingException("Admin0StarAnomalies needs to be even length");
            }
            for (int i = 0; i < Admin0StarAnomalies.Length; i += 2) {
                Admin0StarAnomalyList.Add((Admin0StarAnomalies[i], int.Parse(Admin0StarAnomalies[i + 1])));
            }

            Admin1StarAnomalyList = new();
            if (Admin1StarAnomalies.Length % 3 != 0) {
                throw new ProgrammingException("Admin1StarAnomalies needs length divisible by 3");
            }
            for (int i = 0; i < Admin1StarAnomalies.Length; i += 3) {
                Admin1StarAnomalyList.Add((Admin1StarAnomalies[i], Admin1StarAnomalies[i + 1], int.Parse(Admin1StarAnomalies[i + 2])));
            }

            Admin2AnomalyList = new();
            if (Admin2Anomalies.Length % 4 != 0) {
                throw new ProgrammingException("Admin2Anomalies needs length divisible by 4");
            }
            for (int i = 0; i < Admin2Anomalies.Length; i += 4) {
                Admin2AnomalyList.Add((Admin2Anomalies[i], Admin2Anomalies[i + 1], Admin2Anomalies[i + 2], int.Parse(Admin2Anomalies[i + 3])));
            }


        }


        private readonly string[] Admin0Remove = { "Cruise Ship", "Summer Olympics 2020", "Others", "Taipei and environs", "occupied Palestinian territory",
                                                    "MS Zaandam", " Azerbaijan", "The Bahamas", "Bahamas, The", "Channel Islands", "Diamond Princess",
                                                    "Russian Federation", "St. Martin", "Saint Martine", "Republic of Moldova", "Republic of the Congo", "Cape Verde",
                                                    "Greenland", "Macau", "Ivory Coast", "North Ireland", "Gibraltar", "UK", "Saint Barthelemy", "Saint Martin",
                                                    "Aruba", "Jersey", "Curacao", "Guam", "Puerto Rico", "East Timor"};

        // Pairs of removal for Admin1.  Length must be even
        private readonly string[] Admin1Remove = { "Australia", "From Diamond Princess", "US", "Wuhan Evacuee", "Canada", "Diamond Princess", "Canada", "Toronto, ON",
                                                    "Canada", "Recovered", "Canada", "Grand Princess", "Canada", "Montreal, QC", "United Kingdom", "United Kingdom",
                                                    "United Kingdom", "UK", "United Kingdom", "Unknown", "Israel", "From Diamond Princess", "Canada", " Montreal, QC",
                                                    "Canada", "London, ON", "Lebanon", "None", "Iraq", "None", "Austria", "None", "Canada", "Calgary, Alberta",
                                                    "Canada", "Edmonton, Alberta", "Germany", "Bavaria", "France", "Fench Guiana", "Cook Island", "New Zealand", "India",
                                                    "Dadar Nagar Haveli", "Brazil", "Unknown", "Australia", "External territories", "Australia", "Jervis Bay Territory",
                                                    "US","Chicago, IL","US","Boston, MA","US","Los Angeles, CA","US","Orange, CA","US","Santa Clara, CA","US","Seattle, WA",
                                                    "US","Tempe, AZ","US","San Benito, CA","US","Madison, WI","US","San Diego County, CA","US","San Antonio, TX",
                                                    "US","Ashland, NE","US","Travis, CA","US","Lackland, TX","US","Humboldt County, CA","US","Sacramento County, CA",
                                                    "US","Omaha, NE (From Diamond Princess)","US","Travis, CA (From Diamond Princess)","US","Lackland, TX (From Diamond Princess)",
                                                    "US","Unassigned Location (From Diamond Princess)","US","Portland, OR","US","Snohomish County, WA","US","Providence, RI",
                                                    "US","King County, WA","US","Cook County, IL","US","Grafton County, NH","US","Hillsborough, FL","US","New York City, NY",
                                                    "US","Placer County, CA","US","San Mateo, CA","US","Sarasota, FL","US","Sonoma County, CA","US","Umatilla, OR",
                                                    "US","Fulton County, GA","US","Washington County, OR","US"," Norfolk County, MA","US","Berkeley, CA","US","Maricopa County, AZ",
                                                    "US","Wake County, NC","US","Westchester County, NY","US","Orange County, CA","US","Contra Costa County, CA","US","Bergen County, NJ",
                                                    "US","Harris County, TX","US","San Francisco County, CA","US","Clark County, NV","US","Fort Bend County, TX","US","Grant County, WA",
                                                    "US","Queens County, NY","US","Santa Rosa County, FL","US","Williamson County, TN","US","New York County, NY",
                                                    "US","Unassigned Location, WA","US","Montgomery County, MD","US","Suffolk County, MA","US","Denver County, CO",
                                                    "US","Summit County, CO","US","Chatham County, NC","US","Delaware County, PA","US","Douglas County, NE","US","Fayette County, KY",
                                                    "US","Floyd County, GA","US","Marion County, IN","US","Middlesex County, MA","US","Nassau County, NY","US","Norwell County, MA",
                                                    "US","Ramsey County, MN","US","Washoe County, NV","US","Wayne County, PA","US","Yolo County, CA","US","Santa Clara County, CA",
                                                    "US","Grand Princess Cruise Ship","US","Douglas County, CO","US","Providence County, RI","US","Alameda County, CA",
                                                    "US","Broward County, FL","US","Fairfield County, CT","US","Lee County, FL","US","Pinal County, AZ","US","Rockland County, NY",
                                                    "US","Saratoga County, NY","US","Charleston County, SC","US","Clark County, WA","US","Cobb County, GA","US","Davis County, UT",
                                                    "US","El Paso County, CO","US","Honolulu County, HI","US","Jackson County, OR ","US","Jefferson County, WA","US","Kershaw County, SC",
                                                    "US","Klamath County, OR","US","Madera County, CA","US","Pierce County, WA","US","Plymouth County, MA","US","Santa Cruz County, CA",
                                                    "US","Tulsa County, OK","US","Montgomery County, TX","US","Norfolk County, MA","US","Montgomery County, PA","US","Fairfax County, VA",
                                                    "US","Rockingham County, NH","US","Washington, D.C.","US","Berkshire County, MA","US","Davidson County, TN","US","Douglas County, OR",
                                                    "US","Fresno County, CA","US","Harford County, MD","US","Hendricks County, IN","US","Hudson County, NJ","US","Johnson County, KS",
                                                    "US","Kittitas County, WA","US","Manatee County, FL","US","Marion County, OR","US","Okaloosa County, FL","US","Polk County, GA",
                                                    "US","Riverside County, CA","US","Shelby County, TN","US","Spokane County, WA","US","St. Louis County, MO",
                                                    "US","Suffolk County, NY","US","Ulster County, NY","US","Unassigned Location, VT","US","Unknown Location, MA",
                                                    "US","Volusia County, FL","US","Johnson County, IA","US","Harrison County, KY","US","Bennington County, VT","US","Carver County, MN",
                                                    "US","Charlotte County, FL","US","Cherokee County, GA","US","Collin County, TX","US","Jefferson County, KY","US","Jefferson Parish, LA",
                                                    "US","Shasta County, CA","US","Spartanburg County, SC"};

        // Triples for removal by Admin2.  Length must be divisible by 3.  
        private readonly string[] Admin2Remove = { "US", "Washington", "Walla Walla County", "US", "Washington", "Garfield County", "US","Utah","Out of UT",
                                                    "US", "Wyoming","unassigned", "US","Utah","Washington County", "US","Utah","unassigned", "US","Utah","Southwest",
                                                    "US","California","Out of CA"};

        // Pairs of substitutions for Admin0.  Length must be even
        private readonly string[] Admin0Subs = { "Mainland China", "China", "Palestine", "West Bank and Gaza", "The Gambia", "Gambia",
                                                 "Viet Nam", "Vietnam", "Iran (Islamic Republic of)", "Iran", "Gambia, The", "Gambia",
                                                 "Taiwan*", "Taiwan", "Czechia", "Czech Republic", "Holy See", "Vatican City",
                                                 "Republic of Korea", "South Korea", "Korea, South", "South Korea", "Republic of Ireland", "Ireland"};

        // Quadruples of substituions for Admin1.  Length must be divisible by four
        private readonly string[] Admin1Subs = { "Hong Kong", "Hong Kong", "China", "Hong Kong", "Hong Kong SAR", "Hong Kong", "China", "Hong Kong", "Reunion", "", "France", "Reunion",
                                                "Macao SAR", "Macau", "China", "Macau", "Faroe Islands", "", "Denmark", "Faroe Islands", "United Kingdom", "Falkland Islands (Malvinas)",
                                                "United Kingdom", "Falkland Islands", "United Kingdom", "Falkland Islands (Islas Malvinas)", "United Kingdom", "Falkland Islands",
                                                "Cayman Islands", "", "United Kingdom", "Cayman Islands", "Mayotte", "", "France", "Mayotte", "Martinique", "", "France", "Martinique",
                                                "Guadeloupe", "", "France", "Guadeloupe", "French Guiana", "", "France", "French Guiana", "Taiwan", "Taiwan", "Taiwan", "",
                                                "France", "France", "France", "", "Denmark", "Denmark", "Denmark", "", "Netherlands", "Netherlands", "Netherlands", ""};

        private readonly string[] US_states = {"Alabama", "Alaska", "Arizona", "Arkansas", "California", "Colorado", "Connecticut", "Delaware", "District of Columbia",
                                                "Florida", "Georgia","Hawaii","Idaho","Illinois","Indiana","Iowa","Kansas","Kentucky","Louisiana","Maine","Maryland",
                                                "Massachusetts", "Michigan", "Minnesota", "Mississippi", "Missouri", "Montana", "Nebraska", "Nevada", "New Hampshire",
                                                "New Jersey", "New Mexico","New York","North Carolina","North Dakota","Ohio","Oklahoma","Oregon","Pennsylvania",
                                                "Puerto Rico", "Rhode Island", "South Carolina", "South Dakota", "Tennessee", "Texas", "Utah", "Vermont", "Virginia",
                                                "Washington", "West Virginia","Wisconsin","Wyoming" };

        private readonly string[] US_lower48 = {"Alabama", "Arizona", "Arkansas", "California", "Colorado", "Connecticut", "Delaware", "District of Columbia",
                                                "Florida", "Georgia", "Idaho","Illinois","Indiana","Iowa","Kansas","Kentucky","Louisiana","Maine","Maryland",
                                                "Massachusetts", "Michigan", "Minnesota", "Mississippi", "Missouri", "Montana", "Nebraska", "Nevada", "New Hampshire",
                                                "New Jersey", "New Mexico","New York","North Carolina","North Dakota","Ohio","Oklahoma","Oregon","Pennsylvania",
                                                "Rhode Island", "South Carolina", "South Dakota", "Tennessee", "Texas", "Utah", "Vermont", "Virginia",
                                                "Washington", "West Virginia","Wisconsin","Wyoming" };

        private readonly string[] US_midwest = {"Illinois","Indiana","Iowa","Kansas", "Michigan", "Minnesota", "Missouri", "Nebraska",
                                                "North Dakota","Ohio", "South Dakota",  "West Virginia","Wisconsin"};

        private readonly string[] US_northeast = {"Connecticut", "Delaware", "District of Columbia", "Maine","Maryland",
                                                "Massachusetts",  "New Hampshire",  "New Jersey", "New York", "Pennsylvania",
                                                "Rhode Island", "Vermont"};

        private readonly string[] US_south = {"Alabama", "Arkansas", "Florida", "Georgia", "Kentucky","Louisiana",
                                                 "Mississippi",  "North Carolina","Oklahoma",
                                                 "South Carolina",  "Tennessee", "Texas", "Virginia" };

        private readonly string[] US_west = {"Arizona", "California", "Colorado", "Idaho",
                                                "Montana", "Nevada", "New Mexico", "Oregon", "Utah", "Washington", "Wyoming" };

        private readonly string[] Continents = { "Africa", "Asia", "Europe", "NorthAmerica", "SouthAmerica" };

        private readonly string[] African_countries = { "Algeria", "Angola", "Benin", "Botswana", "Burkina Faso", "Burundi", "Cabo Verde",
                                                "Cameroon", "Central African Republic", "Chad", "Comoros", "Congo (Brazzaville)",
                                                 "Congo (Kinshasa)", "Cote d'Ivoire", "Djibouti", "Egypt", "Equatorial Guinea", "Eritrea",
                                                "Eswatini", "Ethiopia", "Gabon", "Gambia", "Ghana", "Guinea", "Guinea-Bissau",
                                                "Kenya", "Lesotho", "Liberia", "Libya", "Madagascar", "Malawi", "Mali", "Mauritania", "Mauritius",
                                                "Morocco", "Mozambique", "Namibia", "Niger", "Nigeria", "Rwanda", "Sao Tome and Principe",
                                                "Senegal", "Seychelles", "Sierra Leone", "Somalia", "South Africa", "South Sudan",
                                                "Sudan", "Tanzania", "Togo", "Tunisia", "Uganda", "Zambia", "Zimbabwe" };


        private readonly string[] Continental_African_countries = { "Algeria", "Angola", "Benin", "Botswana", "Burkina Faso", "Burundi",
                                                "Cameroon", "Central African Republic", "Chad",  "Congo (Brazzaville)",
                                                 "Congo (Kinshasa)", "Cote d'Ivoire", "Djibouti", "Egypt", "Equatorial Guinea", "Eritrea",
                                                "Eswatini", "Ethiopia", "Gabon", "Gambia", "Ghana", "Guinea", "Guinea-Bissau",
                                                "Kenya", "Lesotho", "Liberia", "Libya", "Madagascar", "Malawi", "Mali", "Mauritania",
                                                "Morocco", "Mozambique", "Namibia", "Niger", "Nigeria", "Rwanda",
                                                "Senegal",  "Sierra Leone", "Somalia", "South Africa", "South Sudan",
                                                "Sudan", "Tanzania", "Togo", "Tunisia", "Uganda", "Zambia", "Zimbabwe" };

        private readonly string[] European_countries = {"Albania","Andorra","Austria","Belarus","Belgium","Bosnia and Herzegovina","Bulgaria",
                                                "Croatia","Cyprus","Czech Republic","Denmark","Estonia","Finland","France","Germany",
                                                "Greece","Iceland","Ireland","Italy","Kosovo","Latvia","Liechtenstein","Lithuania",
                                                "Luxembourg","Moldova","Monaco","Montenegro","Netherlands","Norway","Poland","Portugal",
                                                "Romania","Russia","San Marino","Serbia","Slovakia","Slovenia","Spain","Sweden","Switzerland",
                                                "Turkey","Ukraine","United Kingdom","Vatican City" };

        private readonly string[] Asian_countries = {"Afghanistan","Armenia","Azerbaijan","Bahrain","Bangladesh","Bhutan","Burma","Cambodia",
                                                "China","Georgia","India","Indonesia","Iran","Iraq","Israel","Japan","Jordan","Kazakhstan",
                                                "Kuwait","Kyrgyzstan","Laos","Malaysia","Maldives","Mongolia","Nepal","Oman","Papua New Guinea",
                                                "Pakistan","Philippines","Qatar","Saudi Arabia","Singapore","Sri Lanka","Taiwan","Tajikistan",
                                                "Thailand","Timor-Leste","United Arab Emirates","Uzbekistan","Vietnam","West Bank and Gaza","Yemen"};

        private readonly string[] NorthAmerican_countries = {"Antigua and Barbuda", "Aruba", "Bahamas", "Barbados", "Belize", "Canada", "Costa Rica", "Cuba", "Dominica",
                                                "Dominican Republic", "El Salvador", "Grenada", "Guatemala", "Haiti", "Honduras", "Jamaica", "Mexico", "Nicaragua",
                                                "Panama", "Saint Kitts and Nevis", "Saint Lucia", "Saint Vincent and the Grenadines", "Trinidad and Tobago", "US" };

        private readonly string[] SouthAmerican_countries = {"Argentina", "Bolivia", "Brazil", "Chile", "Colombia", "Ecuador", "Paraguay",
                                                "Peru", "Uruguay", "Venezuela" };

        private readonly string[] CountriesWithProvinces = {"Australia", "Belgium", "Brazil", "Canada", "Chile", "China", "Colombia", "Denmark", "Germany", "India",
                                                "Italy", "Japan", "Malaysia", "Mexico", "Netherlands", "Pakistan", "Peru", "Russia",  "Spain", "Sweden", "Ukraine",
                                                "United Kingdom", "US" };

        private readonly string[] India_states = {"Andaman and Nicobar Islands", "Andhra Pradesh", "Arunachal Pradesh", "Assam", "Bihar", "Chandigarh",
                                                    "Chhattisgarh", "Dadra and Nagar Haveli and Daman and Diu", "Delhi", "Goa", "Gujarat", "Haryana",
                                                    "Himachal Pradesh", "Jammu and Kashmir", "Jharkhand", "Karnataka", "Kerala", "Ladakh",
                                                    "Lakshadweep", "Madhya Pradesh", "Maharashtra", "Manipur", "Meghalaya", "Mizoram", "Nagaland", "Odisha",
                                                    "Puducherry", "Punjab", "Rajasthan", "Sikkim", "Tamil Nadu", "Telangana", "Tripura", "Uttar Pradesh",
                                                    "Uttarakhand", "West Bengal" };

        // Country level anomalies
        private readonly string[] Admin0Anomalies = { "France", "484" };

        // Country level,  remove all provinces
        private readonly string[] Admin0StarAnomalies = { "United Kingdom", "141", "United Kingdom", "162", "Ukraine", "131", "Sweden", "135", "Spain", "113",
                                                          "Russia", "131", "Pakistan", "140", "Mexico", "119", "Netherlands", "177", "Italy", "113", "India", "140", 
                                                          "Germany", "113", "Brazil", "119",  "Belgium", "295", "Chile", "119"};
        // Province level,  remove all districts
        private readonly string[] Admin1StarAnomalies = { "US", "Puerto Rico", "178", "US", "Puerto Rico", "292", "US", "Georgia", "286" };

        // County level
        private readonly string[] Admin2Anomalies = {"US", "New York", "Bronx", "222", "US", "New York", "Kings", "222", "US", "New York", "New York", "222",
                                                     "US", "New York", "Queens", "222", "US", "New York", "Richmond", "222", "US", "Massachusetts", "Unassigned","225" };

    }
}



