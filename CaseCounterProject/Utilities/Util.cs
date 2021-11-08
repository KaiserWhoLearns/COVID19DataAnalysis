using System;

namespace Utilities {
    public class Util {
        public static string ExtractFilename(string filePath) {
            string[] path = filePath.Split('\\');
            return path.Length < 1 ? "" : path[^1];
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
