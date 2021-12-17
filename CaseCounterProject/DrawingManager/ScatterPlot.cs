using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;
using Utilities;

namespace DrawingManager {
    public class ScatterPlot {
        private Canvas canvas;

        private double sourceX;
        private double sourceY;
        private double sourceH;
        private double sourceW;

        public ScatterPlot(double x, double y, double w, double h, Canvas canvas) {
            SetBounds(x, y, w, h);

            this.canvas = canvas;
        }

        public ScatterPlot(Canvas canvas) {
            SetBounds(0, 0, 0, 0);

            this.canvas = canvas;
        }

        private void SetBounds(double x, double y, double w, double h) {
            sourceX = x;
            sourceY = y;
            sourceW = w;
            sourceH = h;
        }

        public int PlotPoint(double x, double y) {

            Ellipse point = new(); 
 
            point.Fill = Brushes.Black;
            point.Width = 10;
            point.Height = 10;
            (double posX, double posY) = Translate(x, y);

            Canvas.SetLeft(point, posX - 5);
            Canvas.SetTop(point, posY - 5);


            return canvas.Children.Add(point);
        }

        public void PlotPoints(List<Point> pointList, double boundaryWidth) {
            (Point lowerLeft, Point upperRight) = Util.BoundingBox(pointList);
            double x = lowerLeft.X - boundaryWidth;
            double y = lowerLeft.Y - boundaryWidth;
            double w = upperRight.X - lowerLeft.X + 2 * boundaryWidth;
            double h = upperRight.Y - lowerLeft.Y + 2 * boundaryWidth;
            SetBounds(x, y, w, h);

            foreach (Point p in pointList) {
                PlotPoint(p.X, p.Y);
            }
        }



        public (double, double) Translate(double x, double y) {
            double canvasW = canvas.Width;
            double canvasH = canvas.Height;


            double scaleFactor = Math.Min(canvasW / sourceW, canvasH / sourceH);

            return ((x - sourceX) * scaleFactor, canvasH - (y - sourceY) * scaleFactor);
        }

        public int Add(System.Windows.UIElement element) {
            return canvas.Children.Add(element);
        }


    }
}
