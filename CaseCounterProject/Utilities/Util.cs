using System;
using System.Text;
using System.IO;

namespace Utilities {
    public class Util {
        public static string ExtractFilename(string filePath) {
            string[] path = filePath.Split('\\');
            return path.Length < 1 ? "" : path[^1];
        }

        public static void WriteToFile(StringBuilder sb, string filePath) {
            using (StreamWriter outputFile = new(filePath)) {
                outputFile.WriteLine(sb.ToString());
            }
        }
    }

    public class DataException : Exception {

        public DataException(string msg) : base(msg) {

        }

    }

    public class ProgrammingException : Exception {

        public ProgrammingException(string msg) : base(msg) {

        }

    }
}
