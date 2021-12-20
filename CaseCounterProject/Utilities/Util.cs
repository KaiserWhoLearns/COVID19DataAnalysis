using System;
using System.Text;
using System.IO;
using System.Windows;
using System.Collections.Generic;

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

        public static (Point, Point) BoundingBox(List<Point> pointList) {
            double minX = Double.MaxValue;
            double minY = Double.MaxValue;
            double maxX = Double.MinValue;
            double maxY = Double.MinValue;

            if (pointList.Count == 0) {
                throw new DataException("Cannot pass an empty pointlist to BoundingBox");
            }

            foreach (Point p in pointList) {
                minX = Math.Min(minX, p.X);
                minY = Math.Min(minY, p.Y);
                maxX = Math.Max(maxX, p.X);
                maxY = Math.Max(maxY, p.Y);
            }

            return (new Point(minX, minY), new Point(maxX, maxY));

        }

        // Y-coordinate for mapping lat/long to a Mercator projection.  The original coordinates are in degrees, so map width is 360.
        public static double Mercator(double latitude) {
            double latRad = Math.PI * latitude / 180;
            double mercN = Math.Log(Math.Tan(Math.PI / 4 + latRad / 2));
            double result = 180 * mercN / Math.PI;
            return result;
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
