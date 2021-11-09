using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaseCounter {

    // Static tables the could be configuration files
    public class Config {

        public List<string> Admin0RemoveList { get; }
        public Dictionary<string, string> Admin0Substitutions { get; }

        public Config() {
            Admin0RemoveList = Admin0Remove.ToList();

            Admin0Substitutions = new();
            for (int i = 0; i < Admin0SubSource.Length; i++)
                Admin0Substitutions.Add(Admin0SubSource[i], Admin0SubTarget[i]);
        }

        private readonly string[] Admin0Remove = { "Cruise Ship", "Summer Olympics 2020", "Others", "Taipei and environs", "occupied Palestinian territory", "MS Zaandam", " Azerbaijan"  };
        private readonly string[] Admin0SubSource = { "Mainland China", "Palestine", "The Gambia", "Viet Nam" };
        private readonly string[] Admin0SubTarget = { "China", "West Bank and Gaza", "Gambia", "Vietnam" };
    }
}
