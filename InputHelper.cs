using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.IO;
using System.CodeDom;

namespace DiplomaProject
{
    public struct Fastener
    {
        public string Name { get; set; }
        public double HeadDiameter { get; set; }
        public double ThreadDiameter { get; set; }
        public Fastener(string name, double headDiameter, double threadDiameter)
        {
            Name = name;
            HeadDiameter = headDiameter;
            ThreadDiameter = threadDiameter;
        }
    }
    public class PathValidatorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string path = value as string;
            return File.Exists(path);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Back conversion is not supported.");
        }
    }

    public static class InputHelper
    {
        
        public static readonly SolidColorBrush InvalidBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fee5e3"));
        public static readonly SolidColorBrush EqualBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e6f5fa"));
        public static readonly SolidColorBrush ValidBrush = Brushes.White;
        public static readonly SolidColorBrush ZeroBrush = Brushes.LightYellow;

        public static List<TextBox> ProjectInfoTextBoxes = new();
        public static List<TextBox> InitialDataTextBoxes = new();
        public static List<TextBox> DesignTextBoxes = new();
        public static List<CheckBox> DesignCheckBoxes = new();
        public static List<Fastener> FastenerList = new List<Fastener> 
        {
              new Fastener ("M1", 3.0, 1.0),
              new Fastener("M1.5", 3.5, 1.5),
              new Fastener("M2", 4.0, 2.0),
              new Fastener("M2.5", 4.5, 2.5),
              new Fastener("M2.8", 4.8, 2.8),
              new Fastener("M3", 5.0, 3.0),
              new Fastener("M3.5", 5.5, 3.5),
              new Fastener("M4", 6.0, 4.0),
              new Fastener("M4.5", 6.5, 4.5),
              new Fastener("M5", 7.0, 5.0),
              new Fastener("M6", 8.0, 6.0),
              new Fastener("M7", 9.0, 7.0),
              new Fastener("M8", 10.0, 8.0),
              new Fastener("M9", 11.0, 9.0),
              new Fastener("M10", 12.0, 10.0)
        };
        private static bool _allowRender = false;
        public static bool AllowRender 
        {
            get => _allowRender;
            set
            {
                if (AllowRender != value)
                {
                    _allowRender = value;
                    OnAllowRenderChanged?.Invoke(null, EventArgs.Empty);
                }
            }
        }
        public static event EventHandler OnAllowRenderChanged;
        public static void UncheckCheckDesignBoxes()
        {
            if (DesignCheckBoxes.Count == 0)
                throw new InvalidOperationException("DesignCheckBoxes is empty.");
            foreach (CheckBox CheckBox in DesignCheckBoxes)
            {
                CheckBox.IsChecked = false;
            }
        }
        public static void ClearTextBoxes(List<TextBox> textBoxes)
        {
            foreach (TextBox TextBox in textBoxes)
            {
                TextBox.Clear();
            }
        }
        public static void ClearTab(TabItem tabItem)
        {
            string tabName = tabItem.Name;
            switch (tabName)
            {
                case "ProjectInfoTab":
                    ClearTextBoxes(ProjectInfoTextBoxes);
                    break;
                case "InitDataTab":
                    ClearTextBoxes(InitialDataTextBoxes);
                    break;
                case "DesignTab":
                    ClearTextBoxes(InitialDataTextBoxes);
                    UncheckCheckDesignBoxes();
                    break;
            }
        }
        public static double? GetEmissivity(TextBox emissivity, TextBox userEmissivity, CheckBox userEmissivityCheckBox)
        {
            if (userEmissivityCheckBox.IsChecked == true)
            {
                return GetNullableDouble(userEmissivity);
            }
            else
                return null;
        }
        public static bool IsInvalidpath(string filePath)
        {
            return System.IO.File.Exists(filePath);
        }
        public static string FormatNullable(double? value, string unit)
        {
            return value.HasValue ? $"{value.Value} {unit}".Trim() : "Н/Д";
        }

        // Аналогичный метод для целочисленных значений
        public static string FormatNullable(int? value, string unit)
        {
            return value.HasValue ? $"{value.Value} {unit}".Trim() : "Н/Д";
        }
        public static double? GetNullableDouble(TextBox textBox)
        {
            string text = textBox.Text.Replace(",", ".");
            if (string.IsNullOrWhiteSpace(text))
                return null;
            if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
                return result;
            throw new FormatException("Невозможно преобразовать введенные данные в число.");
        }

        public static int? GetNullableInt(TextBox textBox)
        {
            string text = textBox.Text.Replace(",", ".");
            if (string.IsNullOrWhiteSpace(text))
                return null;
            if (int.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out int result))
                return result;
            throw new FormatException("Невозможно преобразовать введенные данные в целое число.");
        }
        public static double GetDoubleValue(TextBox textBox, double divider = 1)
        {
            string text = textBox.Text.Replace(",", ".");
            if ((string)textBox.Tag == "Invalid" || (string)textBox.Tag == null)
            {
                throw new Exception("Поля имеют неверно введенные данные");
            }
            return double.Parse(text, CultureInfo.InvariantCulture) / divider;
        }
        public static int GetIntValue(TextBox textBox, double divider = 1)
        {
            string text = textBox.Text.Replace(",", ".");
            if ((string)textBox.Tag == "Invalid" || (string)textBox.Tag == null)
            {
                throw new Exception("Поля имеют неверно введенные данные");
            }
            return int.Parse(text, CultureInfo.InvariantCulture);
        }
        public static string getStringValue(TextBox textBox)
        {
            if ((string)textBox.Tag == "Invalid" || (string)textBox.Tag == null)
            {
                throw new Exception("Поля имеют неверно введенные данные");
            }
            return textBox.Text;
        }
        public static void CommonTextCheck(object sender, RoutedEventArgs e)
        {
            AllowRender = false;
            var givenTextBox = sender as TextBox;
            if (givenTextBox == null) { return; }
            string text = givenTextBox.Text.Replace(",", ".");

            bool isValidNumber = double.TryParse(text, CultureInfo.InvariantCulture, out double parsedValue);
            if (isValidNumber)
            {
                if (parsedValue == 0)
                {
                    givenTextBox.Background = ZeroBrush;
                    givenTextBox.Tag = "Invalid";
                    return;
                }
                givenTextBox.Background = ValidBrush;
                givenTextBox.Tag = "Valid";
            }
            else
            {
                givenTextBox.Background = InvalidBrush;
                givenTextBox.Tag = "Invalid";
            }
        }
        public static void MaterialTextCheck(object sender, RoutedEventArgs e)
        {
            AllowRender = false;
            var givenTextBox = sender as TextBox;
            if (givenTextBox == null) { return; }
            string text = givenTextBox.Text;
            if (text == "" || text == null)
            {
                givenTextBox.Background = InvalidBrush;
                givenTextBox.Tag = "Invalid";
            }
            else
            {
                givenTextBox.Background = ValidBrush;
                givenTextBox.Tag = "Valid";
            }
        }
        public static void MaterialTextCheckSpecialCase(object sender, RoutedEventArgs e)
        {
            var givenTextBox = sender as TextBox;
            if (givenTextBox == null) { return; }
            string text = givenTextBox.Text;
            if (string.IsNullOrEmpty(text))
            {
                givenTextBox.Background = ValidBrush;
                givenTextBox.Tag = "Invalid";
            }
            else
            {
                givenTextBox.Background = ValidBrush;
                givenTextBox.Tag = "Valid";
            }
        }
        public static void CommonTextCheckSpesialCase(object sender, RoutedEventArgs e)
        {
            var givenTextBox = sender as TextBox;
            if (givenTextBox == null) { return; }
            string text = givenTextBox.Text;
            if (string.IsNullOrEmpty(text))
            {
                givenTextBox.Background = ValidBrush;
                givenTextBox.Tag = "Invalid";
            }
            else
            {
                bool isValidNumber = double.TryParse(givenTextBox.Text, CultureInfo.InvariantCulture, out double parsedValue);
                if (isValidNumber)
                {
                    if (parsedValue == 0)
                    {
                        givenTextBox.Background = ZeroBrush;
                        givenTextBox.Tag = "Invalid";
                        return;
                    }
                    givenTextBox.Background = ValidBrush;
                    givenTextBox.Tag = "Valid";
                }
                else
                {
                    givenTextBox.Background = InvalidBrush;
                    givenTextBox.Tag = "Invalid";
                }
            }
        }
        public static void CommonTextCheck(TextBox givenTextBox)
        {
            AllowRender = false;
            string text = givenTextBox.Text.Replace(",", ".");
            bool isValidNumber = double.TryParse(text, CultureInfo.InvariantCulture, out double parsedValue);
            if (isValidNumber)
            {
                if (parsedValue == 0)
                {
                    givenTextBox.Background = ZeroBrush;
                    givenTextBox.Tag = "Invalid";
                    return;
                }
                givenTextBox.Background = ValidBrush;
                givenTextBox.Tag = "Valid";
            }
            else
            {

                givenTextBox.Background = InvalidBrush;
                givenTextBox.Tag = "Invalid";
            }
        }
        public static void MaterialTextCheck(TextBox givenTextBox)
        {
            AllowRender = false;

            string text = givenTextBox.Text;
            if (string.IsNullOrEmpty(text))
            {
                givenTextBox.Background = InvalidBrush;
                givenTextBox.Tag = "Invalid";
            }
            else
            {
                givenTextBox.Background = ValidBrush;
                givenTextBox.Tag = "Valid";
            }
        }
        public static void RangeFieldTextCheck(TextBox lower, TextBox upper)
        {
            AllowRender = false;

            if (lower == null || upper == null) { return; }
            string lowerText = lower.Text.Replace(",", "."),
                    upperText = upper.Text.Replace(",", ".");

            bool isLowerValid = double.TryParse(lowerText, CultureInfo.InvariantCulture, out double lowerResult);
            bool isUpperValid = double.TryParse(upperText, CultureInfo.InvariantCulture, out double upperResult);

            lower.Background = ValidBrush;
            upper.Background = ValidBrush;

            if (upper.Visibility == Visibility.Visible)
            {
                if (!isLowerValid || !isUpperValid)
                {
                    lower.Background = InvalidBrush;
                    upper.Background = InvalidBrush;
                    lower.Tag = "Invalid";
                    upper.Tag = "Invalid";
                }
                else if (lowerResult > upperResult)
                {
                    lower.Background = InvalidBrush;
                    upper.Background = InvalidBrush;
                    lower.Tag = "Invalid";
                    upper.Tag = "Invalid";
                }
                else if (lowerResult == upperResult && lowerResult == 0 && upperResult == 0)
                {
                    lower.Background = InvalidBrush;
                    upper.Background = InvalidBrush;
                    lower.Tag = "Invalid";
                    upper.Tag = "Invalid";
                }
                else if (lowerResult == upperResult)
                {
                    lower.Background = EqualBrush;
                    upper.Background = EqualBrush;
                    lower.Tag = "Valid";
                    upper.Tag = "Valid";
                }
                else
                {
                    if (lowerResult == 0)
                    {
                        lower.Background = ZeroBrush;
                        upper.Background = ZeroBrush;
                    }
                    lower.Tag = "Valid";
                    upper.Tag = "Valid";
                }
            }
            else if (upper.Visibility == Visibility.Hidden)
            {
                if (isLowerValid)
                {
                    if (lowerResult == 0)
                    {
                        lower.Background = ZeroBrush;
                        upper.Background = ZeroBrush;
                        upper.Text = lowerResult.ToString();
                        lower.Tag = "Invalid";
                        return;
                    }
                    upper.Text = lowerResult.ToString();
                    lower.Background = EqualBrush;
                    upper.Background = EqualBrush;
                    lower.Tag = "Valid";
                    upper.Tag = "Valid";
                }
                else
                {
                    upper.Text = lowerResult.ToString();
                    lower.Background = InvalidBrush;
                    upper.Background = InvalidBrush;
                    lower.Tag = "Invalid";
                    upper.Tag = "Invalid";
                }
            }
        }
        public static void AddDescription(Project selProject, Heatsink selHeatsink, Material material, TextBox projectInitComment, string weight)
        {
            if (selProject != null && selProject.Heatsink != null)
            {
                var heatsink = selHeatsink;
                string description = $"Проект: {selProject.ProjectName}\n" +
                                     $"Длина стенки корпуса: {FormatNullable(heatsink.WallLength, "мм")}\n" +
                                     $"Крепеж: {(heatsink.FastenerType != null ? heatsink.FastenerType : "Н/Д")}\n" +
                                     $"Материал: {(material != null ? material.Name : "Н/Д")}\n" +
                                     $"Длина радиатора: {FormatNullable(heatsink.Length, "мм")}\n" +
                                     $"Ширина радиатора: {FormatNullable(heatsink.Depth, "мм")}\n" +
                                     $"Количество рёбер: {FormatNullable(heatsink.NumberOfFins, "")}\n" +
                                     $"Высота ребер: {FormatNullable(heatsink.FinHeight, "мм")}\n" +
                                     $"Толщина ребер: {FormatNullable(heatsink.FinThickness, "мм")}\n" +
                                     $"Толщина основания: {FormatNullable(heatsink.BaseThickness, "мм")}\n" +
                                     $"Мощность источника (TDP): {FormatNullable(heatsink.TDP, "Вт")}\n" +
                                     $"Предельная температура: {FormatNullable(heatsink.TemperatureLimit, "°C")}\n" +
                                     $"Достигнутая температура: {FormatNullable(heatsink.TemperatureAchieved, "°C")}\n" +
                                     $"Температура окружающей среды: {FormatNullable(heatsink.TemperatureEnvironment, "°C")}\n" +
                                     $"Эмиссивность: {(heatsink.Emissivity != null ? heatsink.Emissivity : (material != null ? material.Emissivity : "Н/Д"))}\n" +
                                     $"Вес заготовки: {(string.IsNullOrEmpty(weight) ? "Н/Д" : weight)}";
                projectInitComment.Text += description;
            }
            else
            {
                MessageBox.Show("Выберите проект и радиатор.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public static void FasteningStripsGeometryCheck(TextBox wallLengthTB, TextBox heatsinkLengthTB, Fastener fastener)
        {
            double wallLength = GetDoubleValue(wallLengthTB);
            double heatsinkLength = GetDoubleValue(heatsinkLengthTB);
            if (fastener.Name == null)
            {
                throw new Exception("Тип крепления не выбран.");
            }
            if ((wallLength - heatsinkLength)/2 < fastener.HeadDiameter)
            {
                throw new Exception("Размер крепежной планки слишком мал для выбранного типа крепежа. Пересмотрите тип крепления или исходную геометрию.");
            }
        }
    }
}
