using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Annotations;
using System.Windows;

namespace DiplomaProject
{
    public class HeatsinkPlotter
    {
        public double L { get; set; }
        public double C { get; set; }
        public double H { get; set; }
        public double Delta { get; set; }
        public int Z { get; set; }
        public double FasteningStripWidth { get; set; }
        public double ThereadDiameter { get; set; }
        public PlotModel PlotModel { get; private set; }
        public HeatsinkPlotter()
        {
            InitializePlotModel();
        }
        private void VerticalLine(double x, double y0, double y1, bool sizeAnnotation = false, double size = 0)
        {
            var lineAnnotation = new RectangleAnnotation
            {
                MinimumX = x,
                MaximumX = x,
                MinimumY = y0,
                MaximumY = y1,
                Fill = OxyColors.Black,
                StrokeThickness = 1,
                Stroke = OxyColors.Black
            };
            if (sizeAnnotation)
            {
                lineAnnotation.Text = size.ToString();
                lineAnnotation.TextPosition = new DataPoint(x - 3, (y0 + y1) / 2);
                lineAnnotation.TextColor = OxyColors.Black;
                lineAnnotation.FontSize = 9;
                lineAnnotation.TextRotation = 270;
            }
            PlotModel.Annotations.Add(lineAnnotation);
            PlotModel.InvalidatePlot(true);
        }
        private void HorizontalLine(double y, double x0, double x1, bool sizeAnnotation = false, double size = 0)
        {
            var lineAnnotation = new RectangleAnnotation
            {
                MinimumX = x0,
                MaximumX = x1,
                MinimumY = y,
                MaximumY = y,
                Fill = OxyColors.Black,
                StrokeThickness = 1,
                Stroke = OxyColors.Black
            };
            if (sizeAnnotation)
            {
                lineAnnotation.Text = Math.Round(size, 2).ToString();
                lineAnnotation.TextPosition = new DataPoint((x0 + x1) / 2, y + 3);
                lineAnnotation.TextColor = OxyColors.Black;
                lineAnnotation.FontSize = 9;
            }
            PlotModel.Annotations.Add(lineAnnotation);
            PlotModel.InvalidatePlot(true);
        }
        private void Triangle(double x, double y, int rotation = 180)
        {
            var triangleAnnotation = new PolygonAnnotation
            {
                Stroke = OxyColors.Black,
                StrokeThickness = 0,
                Fill = OxyColors.Black,
            };
            switch (rotation)
            {
                case 0:
                    triangleAnnotation.Points.Add(new DataPoint(x, y));
                    triangleAnnotation.Points.Add(new DataPoint(x + 3, y - 1));
                    triangleAnnotation.Points.Add(new DataPoint(x + 3, y + 1));
                    break;
                case 90:
                    triangleAnnotation.Points.Add(new DataPoint(x, y));
                    triangleAnnotation.Points.Add(new DataPoint(x - 1, y + 3));
                    triangleAnnotation.Points.Add(new DataPoint(x + 1, y + 3));
                    break;
                case 180:
                    triangleAnnotation.Points.Add(new DataPoint(x, y));
                    triangleAnnotation.Points.Add(new DataPoint(x - 3, y - 1));
                    triangleAnnotation.Points.Add(new DataPoint(x - 3, y + 1));
                    break;
                case 270:
                    triangleAnnotation.Points.Add(new DataPoint(x, y));
                    triangleAnnotation.Points.Add(new DataPoint(x - 1, y - 3));
                    triangleAnnotation.Points.Add(new DataPoint(x + 1, y - 3));
                    break;
            }
            PlotModel.Annotations.Add(triangleAnnotation);
            PlotModel.InvalidatePlot(true);
        }
        private void VerticalDimension(double x0, double y0, double x1, double y1, double dimensionHeight)
        {
            double height = y0 + dimensionHeight;
            if (dimensionHeight <= 0)
            {
                VerticalLine(x0, y0, height - 1);
                VerticalLine(x1, y1, height - 1);
            }
            else
            {
                VerticalLine(x0, y0, height + 1);
                VerticalLine(x1, y1, height + 1);
            }
            HorizontalLine(height, x0 - 4, x1 + 4, true, x1 - x0);
            Triangle(x0, height, 180);
            Triangle(x1, height, 0);

        }
        private void HorizontalDimension(double x0, double y0, double x1, double y1, double dimensionHeight)
        {
            double height = x0 - dimensionHeight;
            if (dimensionHeight <= 0)
            {
                HorizontalLine(y0, x0, height + 1);
                HorizontalLine(y1, x1, height + 1);
            }
            else
            {
                HorizontalLine(y0, x0, height - 1);
                HorizontalLine(y1, x1, height - 1);
            }
            VerticalLine(height, y0 - 4, y1 + 4, true, y1 - y0);
            Triangle(height, y0, 270);
            Triangle(height, y1, 90);
        }
        private void InitializePlotModel()
        {
            PlotModel = new PlotModel { };
            var linearAxisX = new LinearAxis { Position = AxisPosition.Bottom, IsPanEnabled = false, IsZoomEnabled = false };
            var linearAxisY = new LinearAxis { Position = AxisPosition.Left, IsPanEnabled = false, IsZoomEnabled = false };
            PlotModel.Axes.Add(linearAxisX);
            PlotModel.Axes.Add(linearAxisY);
        }
        public void UpdateAxisSize(double actualWidth, double actualHeight, DataPoint contentLowerLeft, DataPoint contentUpperRight, double marginRatio = 0.1)
        {
            double rawContentWidth = contentUpperRight.X - contentLowerLeft.X;
            double rawContentHeight = contentUpperRight.Y - contentLowerLeft.Y;

            double contentWidthWithMargin = rawContentWidth * (1 + marginRatio);
            double contentHeightWithMargin = rawContentHeight * (1 + marginRatio);

            double plotAspectRatio = actualWidth / actualHeight;
            double contentAspectRatio = rawContentWidth / rawContentHeight;

            double newWidth, newHeight;

            if (plotAspectRatio > contentAspectRatio)
            {
                newHeight = contentHeightWithMargin;
                newWidth = newHeight * plotAspectRatio;
            }
            else
            {
                newWidth = contentWidthWithMargin;
                newHeight = newWidth / plotAspectRatio;
            }
            double centerX = (contentLowerLeft.X + contentUpperRight.X) / 2;
            double centerY = (contentLowerLeft.Y + contentUpperRight.Y) / 2;

            PlotModel.Axes[0].Minimum = centerX - newWidth / 2;
            PlotModel.Axes[0].Maximum = centerX + newWidth / 2;
            PlotModel.Axes[1].Minimum = centerY - newHeight / 2;
            PlotModel.Axes[1].Maximum = centerY + newHeight / 2;

            PlotModel.InvalidatePlot(true);
        }
        public void DrawHeatsink()
        {
            PlotModel.Annotations.Clear();
            PlotModel.Series.Clear();
            PlotModel.InvalidatePlot(true);
            var baseRect = new RectangleAnnotation
            {
                MinimumX = 0,
                MaximumX = L,
                MinimumY = 0,
                MaximumY = C,
                Fill = OxyColors.Orange,
                Stroke = OxyColors.Black,
                StrokeThickness = 1
            };
            PlotModel.Annotations.Add(baseRect);
            var leftFastening = new RectangleAnnotation
            {
                MinimumX = -FasteningStripWidth,
                MaximumX = 0,
                MinimumY = 0,
                MaximumY = C,
                Fill = OxyColors.Orange,
                Stroke = OxyColors.Black,
                StrokeThickness = 1
            };
            PlotModel.Annotations.Add(leftFastening);
            var rightFastening = new RectangleAnnotation
            {
                MinimumX = L,
                MaximumX = L + FasteningStripWidth,
                MinimumY = 0,
                MaximumY = C,
                Fill = OxyColors.Orange,
                Stroke = OxyColors.Black,
                StrokeThickness = 1
            };
            PlotModel.Annotations.Add(rightFastening);

            var leftHole = new RectangleAnnotation
            {
                MinimumX = -FasteningStripWidth / 2 - (ThereadDiameter / 2),
                MaximumX = -FasteningStripWidth / 2 + (ThereadDiameter / 2),
                MinimumY = 0,
                MaximumY = C,
                Fill = OxyColors.Yellow,
                Stroke = OxyColors.Black,
                StrokeThickness = 1,
            };
            PlotModel.Annotations.Add(leftHole);

            var rightHole = new RectangleAnnotation
            {
                MinimumX = L + FasteningStripWidth / 2 - (ThereadDiameter / 2),
                MaximumX = L + FasteningStripWidth / 2 + (ThereadDiameter / 2),
                MinimumY = 0,
                MaximumY = C,
                Fill = OxyColors.Yellow,
                Stroke = OxyColors.Black,
                StrokeThickness = 1,
            };
            PlotModel.Annotations.Add(rightHole);

            DrawRibs();
            PlotModel.InvalidatePlot(true);
            VerticalDimension(Delta, C + H, Delta + (L - (Delta * (Z + 1))) / Z, C + H, 10);
            VerticalDimension(-FasteningStripWidth, 0, L + FasteningStripWidth, 0, -18);
            VerticalDimension(0, 0, L, 0, -12);
            VerticalDimension(-FasteningStripWidth/2 - ThereadDiameter/2, 0, -FasteningStripWidth/2 + ThereadDiameter / 2, 0, -6);
            HorizontalDimension(0, 0, 0, C + H, 1 + FasteningStripWidth);
        }
        private void DrawRibs()
        {
            double ribSpace = (L - (Delta * (Z + 1))) / Z;
            double totalHeight = C + H;
            for (int i = 0; i <= Z; i++)
            {
                double distance = i * (ribSpace + Delta);
                var rectRib = new RectangleAnnotation
                {
                    MinimumX = distance,
                    MaximumX = distance + Delta,
                    MinimumY = C,
                    MaximumY = totalHeight,
                    Fill = OxyColors.Orange,
                    Stroke = OxyColors.Black,
                    StrokeThickness = 1
                };
                PlotModel.Annotations.Add(rectRib);
            }
        }
        public void PlotSizeChanged(object sender, SizeChangedEventArgs e)
        {
            double oldWidth = e.PreviousSize.Width;
            double newWidth = e.NewSize.Width;
            double oldHeight = e.PreviousSize.Height;
            double newHeight = e.NewSize.Height;

            var xAxis = PlotModel.Axes.FirstOrDefault(ax => ax.Position == AxisPosition.Bottom);
            var yAxis = PlotModel.Axes.FirstOrDefault(ay => ay.Position == AxisPosition.Left);

            if (xAxis != null && oldWidth > 0)
            {
                double xScaleFactor = newWidth / oldWidth;
                AdjustAxis(xAxis, xScaleFactor);
            }
            if (yAxis != null && oldHeight > 0)
            {
                double yScaleFactor = newHeight / oldHeight;
                AdjustAxis(yAxis, yScaleFactor);
            }
            PlotModel.InvalidatePlot(true);
        }
        private static void AdjustAxis(Axis axis, double scaleFactor)
        {
            double midPoint = (axis.ActualMaximum + axis.ActualMinimum) / 2;
            double range = (axis.ActualMaximum - axis.ActualMinimum) * scaleFactor / 2;
            axis.Minimum = midPoint - range;
            axis.Maximum = midPoint + range;
        }
        public void FitInView(double actualWidth, double actualHeight)
        {
            DataPoint upperRight = new DataPoint(L + FasteningStripWidth, C + H + 15), lowerLeft = new DataPoint(-FasteningStripWidth, -20);
            UpdateAxisSize(actualWidth, actualHeight, lowerLeft, upperRight);
        }
    }
}
