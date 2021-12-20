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

        Random random;

        public ScatterPlot(double x, double y, double w, double h, Canvas canvas) {
            SetBounds(x, y, w, h);
            random = new();

            this.canvas = canvas;
        }

        public ScatterPlot(Canvas canvas) : this(0, 0, 0, 0, canvas) {
        }

        private void SetBounds(double x, double y, double w, double h) {
            sourceX = x;
            sourceY = y;
            sourceW = w;
            sourceH = h;
        }

        // val should be in the range 0 <= val < 1 (or null)
        public int PlotPoint(double x, double y, double? val, double boundaryWidth) {

            Ellipse point = new();

            SolidColorBrush brush = new();

            if (val == null) {
                brush.Color = Color.FromArgb(255, 255, 255, 255);
            } else {
                double val2 = (double)val;
                byte green = (val2 < 0) ? (byte) 0 : ((val2 >= 1) ? (byte) 255 : (byte)(256 * val));
                
                brush.Color = Color.FromArgb(255, (byte)(255 - green), green, 0);
            }

            point.Fill = brush;
            point.StrokeThickness = 1;
            point.Stroke = Brushes.Black;

            point.Width = 20;
            point.Height = 20;
            (double posX, double posY) = Translate(x, y, boundaryWidth);

            Canvas.SetLeft(point, posX - 10);
            Canvas.SetTop(point, posY - 10);


            return canvas.Children.Add(point);
        }

        public void PlotPoints(List<Point> pointList, List<double?> valueList, double boundaryWidth) {
            (Point lowerLeft, Point upperRight) = Util.BoundingBox(pointList);
            double x = lowerLeft.X;
            double y = lowerLeft.Y;
            double w = upperRight.X - lowerLeft.X;
            double h = upperRight.Y - lowerLeft.Y;
            SetBounds(x, y, w, h);

            for (int i = 0; i < pointList.Count; i++) {
                double? val = (valueList != null) ? valueList[i] : 0.0;
                PlotPoint(pointList[i].X, pointList[i].Y, val,  boundaryWidth);
            }
        }



        public (double, double) Translate(double x, double y, double boundaryWidth) {
            double canvasW = canvas.Width - 2*boundaryWidth;
            double canvasH = canvas.Height -2*boundaryWidth;


            double scaleFactor = Math.Min(canvasW / sourceW, canvasH / sourceH);

            return ((x - sourceX) * scaleFactor + boundaryWidth, canvasH - (y - sourceY) * scaleFactor + boundaryWidth);
        }

        public int Add(System.Windows.UIElement element) {
            return canvas.Children.Add(element);
        }


    }
}
