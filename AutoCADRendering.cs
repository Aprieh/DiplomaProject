using AutoCAD;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Controls;
#pragma warning disable CS8604 // Возможно, аргумент-ссылка, допускающий значение NULL.

namespace DiplomaProject
{
    public static class COMInterop
    {
        [DllImport("oleaut32.dll", PreserveSig = false)]
        public static extern void GetActiveObject(ref Guid rclsid, IntPtr pvReserved, [MarshalAs(UnmanagedType.IUnknown)] out object ppunk);
        [DllImport("ole32.dll")]
        public static extern int CLSIDFromProgID([MarshalAs(UnmanagedType.LPWStr)] string lpszProgID, out Guid pclsid);
        public static object GetActiveCOMObject(string progId)
        {
            CLSIDFromProgID(progId, out Guid clsid);
            GetActiveObject(ref clsid, IntPtr.Zero, out object obj);
            return obj;
        }
    }
    internal class AutoCadRenderer
    {
        public static bool hasJustBeenLaunched { get; set; }
        public double L { get; set; }
        public double D { get; set; }
        public double C { get; set; }
        public double H { get; set; }
        public double Delta { get; set; }
        public int Z { get; set; }
        public string FilePath { get; set; }
        public double ThreadDiameter { get; set; }
        public double FasteningStrip { get; set; }


        private AcadApplication acadApp;
        private AcadDocument acadDoc;

        private readonly string layerName = "RadiatorLayer";

        //private static List<AcadEntity> radiatorEntities = new();

        private AcadDocument FindOpenDocument(AcadApplication app, string filePath)
        {
            string targetFileName = Path.GetFileName(filePath);  // Получаем имя файла из полного пути

            foreach (AcadDocument doc in app.Documents)
            {
                string openDocName = Path.GetFileName(doc.Name);  // Сравниваем только имена файлов

                if (string.Equals(openDocName, targetFileName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return doc;
                }
            }
            return null;
        }
        private void EnsureLayer(AcadDocument doc, string layerName)
        {
            AcadLayer layer;
            try
            {
                layer = doc.Layers.Item(layerName);
            }
            catch
            {
                layer = doc.Layers.Add(layerName);
            }
            doc.ActiveLayer = layer;
        }
        private void ClearLayer()
        {
            AcadModelSpace modelSpace = acadDoc.ModelSpace;
            List<AcadEntity> entitiesToDelete = new List<AcadEntity>();

            foreach (AcadEntity entity in modelSpace)
            {
                if (entity.Layer == layerName)
                {
                    entitiesToDelete.Add(entity);
                }
            }

            foreach (AcadEntity entity in entitiesToDelete)
            {
                entity.Delete();
            }
        }
        private AcadEntity AddRectangle(AcadModelSpace modelSpace, double x, double y, double width, double height)
        {
            double[] rectCorner1 = { x, y, 0 };
            double[] rectCorner2 = { x, y + height, 0 };
            double[] rectCorner3 = { x + width, y + height, 0 };
            double[] rectCorner4 = { x + width, y, 0 };

            double[] vertices =
            {
                rectCorner1[0], rectCorner1[1],
                rectCorner2[0], rectCorner2[1],
                rectCorner3[0], rectCorner3[1],
                rectCorner4[0], rectCorner4[1],
                rectCorner1[0], rectCorner1[1] // Замыкаем полилинию
            };

            AcadLWPolyline acPoly = modelSpace.AddLightWeightPolyline(vertices);
            acPoly.Closed = true;
            return acPoly as AcadEntity;
        }
        private AcadEntity AddCircle(AcadModelSpace modelSpace, double centerX, double centerY, double radius)
        {
            object centerPoint = new double[] { centerX, centerY, 0 };
            return modelSpace.AddCircle(centerPoint, radius) as AcadEntity;
        }
        private AcadEntity AddDiameterDimension(AcadModelSpace modelSpace, double centerX, double centerY, double radius)
        {
            double startX = centerX - radius;
            double startY = centerY;
            double endX = centerX + radius;
            double endY = centerY;

            object startPt = new double[] { startX, startY, 0 };
            object endPt = new double[] { endX, endY, 0 };

            object textPoint = new double[] { centerX, centerY + 10, 0 }; // Смещение по Y для видимости

            AcadDimAligned dim = modelSpace.AddDimAligned(startPt, endPt, textPoint);

            dim.TextOverride = $"\\U+2205{radius * 2}"; // Диаметр = 2 * радиус

            return dim as AcadEntity;
        }
        public void Render()
        {
            try
            {
                acadApp = (AcadApplication)COMInterop.GetActiveCOMObject("AutoCAD.Application");
                hasJustBeenLaunched = false;
            }
            catch
            {
                acadApp = (AcadApplication)Activator.CreateInstance(Type.GetTypeFromProgID("AutoCAD.Application"), true);
                hasJustBeenLaunched = true;
            }

            acadApp.Visible = true;

            acadDoc = FindOpenDocument(acadApp, FilePath);
            if (acadDoc == null)
            {
                acadDoc = acadApp.Documents.Open(FilePath);
            }

            AcadModelSpace modelSpace = acadDoc.ModelSpace;

            EnsureLayer(acadDoc, layerName);

            ClearLayer();

            acadDoc.SetVariable("PDMODE", 3);
            acadDoc.SetVariable("PDSIZE", 1);
            LoadDashedLinetype(acadDoc);

            DrawRadiator(modelSpace, 60, 150);

            DeletePhantomDocument();

            acadDoc.Regen(AcRegenType.acAllViewports);
        }
        private void LoadDashedLinetype(AcadDocument acadDoc)
        {
            const string linetypeName = "JIS_02_0.7";
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string acadVersion = "AutoCAD 2024"; // Consider making this dynamic if necessary
            string acadSupportPath = Path.Combine(appDataPath, $"Autodesk\\{acadVersion}\\R24.3\\rus\\Support");
            string linetypeFilePath = Path.Combine(acadSupportPath, "acadiso.lin");

            try
            {
                AcadLineType lt = acadDoc.Linetypes.Item(linetypeName);
            }
            catch
            {
                acadDoc.Linetypes.Load(linetypeName, linetypeFilePath);
            }
        }
        private AcadLine AddCustomDashedLine(AcadModelSpace modelSpace, double startX, double startY, double endX, double endY, string linetypeName)
        {
            LoadDashedLinetype(modelSpace.Document);

            AcadLine line = modelSpace.AddLine(new double[] { startX, startY, 0 }, new double[] { endX, endY, 0 }) as AcadLine;

            line.Linetype = linetypeName;
            line.Update();

            return line;
        }
        private void DeletePhantomDocument()
        {
            if (hasJustBeenLaunched)
            {
                foreach (AcadDocument doc in acadApp.Documents)
                {
                    string docName = Path.GetFileNameWithoutExtension(doc.Name);
                    if (string.Equals(docName, "Чертеж1", StringComparison.OrdinalIgnoreCase))
                    {
                        doc.Close(false);
                        break;
                    }
                }
            }
        }
        private void DrawRadiator(AcadModelSpace modelSpace, double startX, double startY)
        {
            double ribSpace = (L - (Delta * (Z + 1))) / Z;
            double dist = ribSpace + Delta;

            DrawAbove(modelSpace, startX, startY, ribSpace, dist);
            DrawFront(modelSpace, startX, startY, dist);
        }

        private void DrawFront(AcadModelSpace modelSpace, double startX, double startY, double dist)
        {
            double offset = H + 20;
            double radius = ThreadDiameter / 2;
            HeatsinkFrontPolyLine(modelSpace, startX, startY, dist, offset);

            modelSpace.AddDimAligned(new double[] { -FasteningStrip + startX, -offset + startY, 0 }, new double[] { -FasteningStrip + startX, -offset + C + startY, 0 }, new double[] { -5 - FasteningStrip + startX, 0, 0 });
            modelSpace.AddDimAligned(new double[] { startX, -offset + startY, 0 }, new double[] { startX, -offset + C + H + startY, 0 }, new double[] { -10 - FasteningStrip + startX, 0, 0 });

            AddCustomDashedLine(modelSpace,
                                                     -FasteningStrip / 2 - radius + startX,
                                                     -offset + startY,
                                                     -FasteningStrip / 2 - radius + startX,
                                                     -offset + C + startY,
                                                     "JIS_02_0.7");

            AddCustomDashedLine(modelSpace,
                                                     -FasteningStrip / 2 + radius + startX,
                                                     -offset + startY,
                                                     -FasteningStrip / 2 + radius + startX,
                                                     -offset + C + startY,
                                                     "JIS_02_0.7");


            AddCustomDashedLine(modelSpace,
                                                     FasteningStrip / 2 - radius + startX + L,
                                                     -offset + startY,
                                                     FasteningStrip / 2 - radius + startX + L,
                                                     -offset + C + startY,
                                                     "JIS_02_0.7");

            AddCustomDashedLine(modelSpace,
                                                     FasteningStrip / 2 + radius + startX + L,
                                                     -offset + startY,
                                                     FasteningStrip / 2 + radius + startX + L,
                                                     -offset + C + startY,
                                                     "JIS_02_0.7");

            AddCustomDashedLine(modelSpace, -FasteningStrip / 2 + startX, -offset + startY - C * 2, -FasteningStrip / 2 + startX, -offset + C + startY + C * 2, "JIS_02_0.7");
            AddCustomDashedLine(modelSpace, FasteningStrip / 2 + startX + L, -offset + startY - C * 2, FasteningStrip / 2 + startX + L, -offset + C + startY + C * 2, "JIS_02_0.7");
        }

        private void HeatsinkFrontPolyLine(AcadModelSpace modelSpace, double startX, double startY, double dist, double offset)
        {
            List<double> vertices =
                        [
                            .. new double[] { startX - FasteningStrip, -offset + startY },
                .. new double[] { startX - FasteningStrip, -offset + startY + C },
                .. new double[] { startX, -offset + startY + C },
            ];
            for (int i = 0; i <= Z; i++)
            {
                double xPosition = startX + i * dist;
                vertices.AddRange([xPosition, -offset + C + startY]);
                vertices.AddRange([xPosition, -offset + C + H + startY]);
                vertices.AddRange([xPosition + Delta, -offset + C + H + startY]);
                vertices.AddRange([xPosition + Delta, -offset + C + startY]);
            }
            vertices.AddRange(new double[] { startX + L, -offset + startY + C });
            vertices.AddRange(new double[] { startX + L + FasteningStrip, -offset + startY + C });
            vertices.AddRange(new double[] { startX + L + FasteningStrip, -offset + startY });


            // Преобразование в массив
            double[] polylineVertices = vertices.ToArray();

            // Создание полилинии
            AcadLWPolyline polyline = modelSpace.AddLightWeightPolyline(polylineVertices);
            polyline.Closed = true;
        }
        private void DrawAbove(AcadModelSpace modelSpace, double startX, double startY, double ribSpace, double dist)
        {
            AddRectangle(modelSpace, startX, startY, L, D);
            AddRectangle(modelSpace, startX - FasteningStrip, startY, FasteningStrip, D);
            AddRectangle(modelSpace, startX + L, startY, FasteningStrip, D);

            for (int i = 0; i <= Z; i++)
            {
                double xPosition = i * dist;
                AddRectangle(modelSpace, startX + xPosition, startY, Delta, D);
            }

            double threadRadius = ThreadDiameter / 2;

            AddCircle(modelSpace, -FasteningStrip / 2 + startX, D / 2 + startY, threadRadius); // Левый нижний угол
            AddDiameterDimension(modelSpace, -FasteningStrip / 2 + startX, D / 2 + startY, threadRadius);
            AddCircle(modelSpace, FasteningStrip / 2 + L + startX, D / 2 + startY, threadRadius); // Левый верхний угол

            modelSpace.AddPoint(new double[] { -FasteningStrip / 2 + startX, startY + D / 2, 0 });
            modelSpace.AddPoint(new double[] { FasteningStrip / 2 + L + startX, startY + D / 2, 0 });

            modelSpace.AddDimAligned(//размер слева ширина радиатора
               new double[] { -FasteningStrip + startX, startY, 0 },
               new double[] { -FasteningStrip + startX, startY + D, 0 },
               new double[] { -20 + startX, startY, 0 });
            modelSpace.AddDimAligned(//размер слева отношение отверстия
               new double[] { -FasteningStrip / 2 + startX, startY, 0 },
               new double[] { -FasteningStrip / 2 + startX, startY + D / 2, 0 },
               new double[] { -15 + startX, startY, 0 });
            modelSpace.AddDimAligned(//размер снизу отношение оверстия
                new double[] { -FasteningStrip + startX, startY + D / 2, 0 },
                new double[] { -FasteningStrip / 2 + startX, startY + D / 2, 0 },
                new double[] { 0, startY - 5, 0 });
            modelSpace.AddDimAligned(//размер сверку общая длина с креплением
                new double[] { -FasteningStrip + startX, startY + D, 0 },
                new double[] { FasteningStrip + L + startX, startY + D, 0 },
                new double[] { 0, D + 20 + startY, 0 });
            modelSpace.AddDimAligned(//размер сверху длина без крепления
                new double[] { startX, startY + D, 0 },
                new double[] { startX + L, startY + D, 0 },
                new double[] { 0, startY + D + 15, 0 });
            modelSpace.AddDimAligned(//размер сверху толшина ребра
                new double[] { startX, startY + D, 0 },
                new double[] { startX + Delta, startY + D, 0 },
                new double[] { 0, startY + D + 5, 0 });
            modelSpace.AddDimAligned(//размер сверху тольшина шага ребра то есть ребро и промежуток
                new double[] { startX, startY + D, 0 },
                new double[] { startX + Delta + ribSpace, startY + D, 0 },
                new double[] { 0, startY + D + 10, 0 });
        }
    }
}
