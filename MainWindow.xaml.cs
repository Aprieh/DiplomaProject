using AutoCAD;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.IO;

namespace DiplomaProject
{
    public partial class MainWindow : Window
    {
        private DatabaseContext databaseContext = new();
        public ObservableCollection<Project> Projects { get; } = new ObservableCollection<Project>();
        private HeatsinkPlotter HeatsinkPlotter { get; set; }
        public Material? SelectedMaterial { get; private set; }
        public Heatsink? SelectedHeatsink { get; set; }
        public Project? SelectedProject { get; set; }
        public Fastener SelectedFastener { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            HeatsinkPlotter = new HeatsinkPlotter();
            MyPlot.Model = HeatsinkPlotter.PlotModel;
            DataContext = this;
            InitialState();
            LoadProjects();
        }
        private void InitialState()
        {
            InitDataTab.Visibility = Visibility.Hidden;
            DesignTab.Visibility = Visibility.Hidden;

            HeightTo.Visibility = Visibility.Hidden;
            HeightUnits.Visibility = Visibility.Hidden;
            OptimizeHeightTextBoxUpTo.Visibility = Visibility.Hidden;
            HeightFrom.Text = "Высота (H):";

            ThicknessTo.Visibility = Visibility.Hidden;
            OptimizeThicknessTextBoxUpTo.Visibility = Visibility.Hidden;
            ThicknessUnits.Visibility = Visibility.Hidden;
            ThicknessFrom.Text = "Толщина (Delta):";

            RibCountTo.Visibility = Visibility.Hidden;
            OptimizeRibCountTextBoxUpTo.Visibility = Visibility.Hidden;
            RibCountUnits.Visibility = Visibility.Hidden;
            RibCountFrom.Text = "Количество (Z):";

            Binding binding = new Binding
            {
                Source = EmissiveRatioTextBox,
                Path = new PropertyPath("Text"),
                Mode = BindingMode.OneWay
            };
            EmissivityStatusTextBox.SetBinding(TextBox.TextProperty, binding);

            Rendering2DButton.IsEnabled = false;
            RenderAutoCADMenuItem.IsEnabled = false;

            SimpleFieldsDelegates();
            RangeFieldsDelegates();
            MaterialFieldsDelegates();
            InitialFieldChecks();

            PopulateFatenerList();

            InputHelper.InitialDataTextBoxes.AddRange([InitLengthTextBox, InitWidthTextBox, InitTDPTextBox,
                MaterialNameTextBox, ThermalCondtextBox, EmissiveRatioTextBox,
                DensityTextBox, UserEmissivityRatioTextBox, EnvTempTextBox, LimitTempTextBox]);
            InputHelper.ProjectInfoTextBoxes.AddRange([ProjectNameTextBox, CreationDatetextBox,
                ChangeDateTextBox, ProjectDirTextBox, ProjectComment]);

            InputHelper.DesignTextBoxes.AddRange([OptimizeHeightTextBox, OptimizeHeightTextBoxUpTo,
                OptimizeThicknessTextBox, OptimizeThicknessTextBoxUpTo, OptimizeRibCountTextBox, OptimizeRibCountTextBoxUpTo,
                BaseThicknesstextBox, HeightResult, ThicknessResult, CountResult, TemperatureResult]);
            InputHelper.DesignCheckBoxes.AddRange([OptimizeHeightCheckBox, OptimizeThicknessCheckBox, OptimizeRibCountCheckBox]);

            UserEmissivityRatioTextBox.Background = Brushes.LightGray;
            UserEmissivityRatioTextBox.IsEnabled = false;

            InputHelper.OnAllowRenderChanged += InputHelper_OnAllowRenderChanged;

            ProjectTabInterfaceEnablement(false);
        }
        private void SimpleFieldsDelegates()
        {
            InitWallLengthTextBox.TextChanged += InputHelper.CommonTextCheck;
            InitLengthTextBox.TextChanged += InputHelper.CommonTextCheck;
            InitWidthTextBox.TextChanged += InputHelper.CommonTextCheck;
            InitTDPTextBox.TextChanged += InputHelper.CommonTextCheck;
            UserEmissivityRatioTextBox.TextChanged += InputHelper.CommonTextCheck;
            EnvTempTextBox.TextChanged += InputHelper.CommonTextCheck;
            LimitTempTextBox.TextChanged += InputHelper.CommonTextCheck;
            BaseThicknesstextBox.TextChanged += InputHelper.CommonTextCheck;
            HeightResult.TextChanged += InputHelper.CommonTextCheck;
            ThicknessResult.TextChanged += InputHelper.CommonTextCheck;
            CountResult.TextChanged += InputHelper.CommonTextCheck;
        }
        private void RangeFieldsDelegates()
        {
            OptimizeHeightTextBox.TextChanged += RangeFieldTextChanged;
            OptimizeHeightTextBoxUpTo.TextChanged += RangeFieldTextChanged;
            OptimizeThicknessTextBox.TextChanged += RangeFieldTextChanged;
            OptimizeThicknessTextBoxUpTo.TextChanged += RangeFieldTextChanged;
            OptimizeRibCountTextBox.TextChanged += RangeFieldTextChanged;
            OptimizeRibCountTextBoxUpTo.TextChanged += RangeFieldTextChanged;
        }
        private void MaterialFieldsDelegates()
        {
            MaterialNameTextBox.TextChanged += InputHelper.MaterialTextCheck;
            ThermalCondtextBox.TextChanged += InputHelper.MaterialTextCheck;
            EmissiveRatioTextBox.TextChanged += InputHelper.MaterialTextCheck;
            DensityTextBox.TextChanged += InputHelper.MaterialTextCheck;
        }
        private void InitialFieldChecks()
        {
            InputHelper.CommonTextCheck(InitWallLengthTextBox);
            InputHelper.CommonTextCheck(InitLengthTextBox);
            InputHelper.CommonTextCheck(InitWidthTextBox);
            InputHelper.CommonTextCheck(InitTDPTextBox);
            InputHelper.CommonTextCheck(UserEmissivityRatioTextBox);
            InputHelper.CommonTextCheck(EnvTempTextBox);
            InputHelper.CommonTextCheck(LimitTempTextBox);
            InputHelper.CommonTextCheck(BaseThicknesstextBox);
            InputHelper.CommonTextCheck(HeightResult);
            InputHelper.CommonTextCheck(ThicknessResult);
            InputHelper.CommonTextCheck(CountResult);

            InputHelper.RangeFieldTextCheck(OptimizeHeightTextBox, OptimizeHeightTextBoxUpTo);
            InputHelper.RangeFieldTextCheck(OptimizeThicknessTextBox, OptimizeThicknessTextBoxUpTo);
            InputHelper.RangeFieldTextCheck(OptimizeRibCountTextBox, OptimizeRibCountTextBoxUpTo);

            InputHelper.MaterialTextCheck(MaterialNameTextBox);
            InputHelper.MaterialTextCheck(ThermalCondtextBox);
            InputHelper.MaterialTextCheck(EmissiveRatioTextBox);
            InputHelper.MaterialTextCheck(DensityTextBox);
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void LoadProjects()
        {
            Projects.Clear();
            List<Project> projectsFromDB = databaseContext.Projects.ToList();
            foreach (Project project in projectsFromDB)
            {
                Projects.Add(project);
            }
        }
        private void OptimizeHeightCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            HeightTo.Visibility = Visibility.Visible;
            HeightUnits.Visibility = Visibility.Visible;
            OptimizeHeightTextBoxUpTo.Visibility = Visibility.Visible;
            HeightFrom.Text = "Высота (H) от:";
            OptimizeHeightTextBox.Clear();
            OptimizeHeightTextBoxUpTo.Clear();
        }
        private void OptimizeHeightCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            HeightTo.Visibility = Visibility.Hidden;
            HeightUnits.Visibility = Visibility.Hidden;
            OptimizeHeightTextBoxUpTo.Visibility = Visibility.Hidden;
            HeightFrom.Text = "Высота (H):";
            OptimizeHeightTextBox.Clear();
            OptimizeHeightTextBoxUpTo.Clear();
        }
        private void OptimizeThicknessCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ThicknessTo.Visibility = Visibility.Visible;
            OptimizeThicknessTextBoxUpTo.Visibility = Visibility.Visible;
            ThicknessUnits.Visibility = Visibility.Visible;
            ThicknessFrom.Text = "Толщина (Delta) от:";
            OptimizeThicknessTextBox.Clear();
            OptimizeThicknessTextBoxUpTo.Clear();
        }
        private void OptimizeThicknessCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ThicknessTo.Visibility = Visibility.Hidden;
            OptimizeThicknessTextBoxUpTo.Visibility = Visibility.Hidden;
            ThicknessUnits.Visibility = Visibility.Hidden;
            ThicknessFrom.Text = "Толщина (Delta):";
            OptimizeThicknessTextBox.Clear();
            OptimizeThicknessTextBoxUpTo.Clear();
        }
        private void DrawAndFitInViewButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                InputHelper.FasteningStripsGeometryCheck(InitWallLengthTextBox, InitLengthTextBox, SelectedFastener);

                HeatsinkPlotter.L = InputHelper.GetDoubleValue(InitLengthTextBox);
                HeatsinkPlotter.C = InputHelper.GetDoubleValue(BaseThicknesstextBox);
                HeatsinkPlotter.H = InputHelper.GetDoubleValue(HeightResult);
                HeatsinkPlotter.Delta = InputHelper.GetDoubleValue(ThicknessResult);
                HeatsinkPlotter.Z = InputHelper.GetIntValue(CountResult);
                HeatsinkPlotter.FasteningStripWidth = (InputHelper.GetDoubleValue(InitWallLengthTextBox) - InputHelper.GetDoubleValue(InitLengthTextBox)) / 2;
                HeatsinkPlotter.ThereadDiameter = SelectedFastener.ThreadDiameter;

                HeatsinkPlotter.DrawHeatsink();
                FitInViewButton_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusBarText.Content = $"Ошибка: {ex.Message}";
            }
        }
        private void MyPlot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            HeatsinkPlotter.PlotSizeChanged(sender, e);
            FitInViewButton_Click(sender, e);
        }

        private void FitInViewButton_Click(object sender, RoutedEventArgs e)
        {
            HeatsinkPlotter.FitInView(MyPlot.ActualWidth, MyPlot.ActualHeight);
        }
        private void OptimizeRibCountCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            RibCountTo.Visibility = Visibility.Visible;
            OptimizeRibCountTextBoxUpTo.Visibility = Visibility.Visible;
            RibCountUnits.Visibility = Visibility.Visible;
            RibCountFrom.Text = "Количество (Z) от:";
            OptimizeRibCountTextBox.Clear();
            OptimizeRibCountTextBoxUpTo.Clear();
        }
        private void OptimizeRibCountCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            RibCountTo.Visibility = Visibility.Hidden;
            OptimizeRibCountTextBoxUpTo.Visibility = Visibility.Hidden;
            RibCountUnits.Visibility = Visibility.Hidden;
            RibCountFrom.Text = "Количество (Z):";
            OptimizeRibCountTextBox.Clear();
            OptimizeRibCountTextBoxUpTo.Clear();
        }

        private void ChangeReceivedParametersCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            HeightResult.IsReadOnly = false;
            ThicknessResult.IsReadOnly = false;
            CountResult.IsReadOnly = false;
            TemperatureResult.IsReadOnly = false;
        }
        private void ChangeReceivedParametersCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            HeightResult.IsReadOnly = true;
            ThicknessResult.IsReadOnly = true;
            CountResult.IsReadOnly = true;
            TemperatureResult.IsReadOnly = true;
        }
        private void UserEmissivityCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            EmissiveRatioTextBox.Background = Brushes.LightGray;
            UserEmissivityRatioTextBox.Background = Brushes.White;
            EmissiveRatioTextBox.IsEnabled = false;
            UserEmissivityRatioTextBox.IsEnabled = true;
            UserEmissivityRatioTextBox.Clear();
            Binding binding = new Binding
            {
                Source = UserEmissivityRatioTextBox,
                Path = new PropertyPath("Text"),
                Mode = BindingMode.OneWay
            };
            EmissivityStatusTextBox.SetBinding(TextBox.TextProperty, binding);
        }
        private void UserEmissivityCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            SolidColorBrush invalidBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fee5e3"));
            EmissiveRatioTextBox.Background = (EmissiveRatioTextBox.Text == "" || EmissiveRatioTextBox.Text == null) ? invalidBrush : Brushes.White;
            UserEmissivityRatioTextBox.Background = Brushes.LightGray;
            EmissiveRatioTextBox.IsEnabled = true;
            UserEmissivityRatioTextBox.IsEnabled = false;
            UserEmissivityRatioTextBox.Clear();
            Binding binding = new Binding
            {
                Source = EmissiveRatioTextBox,
                Path = new PropertyPath("Text"),
                Mode = BindingMode.OneWay
            };
            EmissivityStatusTextBox.SetBinding(TextBox.TextProperty, binding);
        }
        private double GetEmissivity()
        {
            if (EmissiveRatioTextBox.IsEnabled == true)
            {
                return InputHelper.GetDoubleValue(EmissiveRatioTextBox);
            }
            else
            {
                return InputHelper.GetDoubleValue(UserEmissivityRatioTextBox);
            }
        }
        private void RangeFieldTextChanged(object sender, RoutedEventArgs e)
        {
            if (sender == OptimizeHeightTextBox || sender == OptimizeHeightTextBoxUpTo)
            {
                InputHelper.RangeFieldTextCheck(OptimizeHeightTextBox, OptimizeHeightTextBoxUpTo);
            }
            else if (sender == OptimizeThicknessTextBox || sender == OptimizeThicknessTextBoxUpTo)
            {
                InputHelper.RangeFieldTextCheck(OptimizeThicknessTextBox, OptimizeThicknessTextBoxUpTo);
            }
            else if (sender == OptimizeRibCountTextBox || sender == OptimizeRibCountTextBoxUpTo)
            {
                InputHelper.RangeFieldTextCheck(OptimizeRibCountTextBox, OptimizeRibCountTextBoxUpTo);
            }
        }
        
        private void CalculateGeometry_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                InputHelper.FasteningStripsGeometryCheck(InitWallLengthTextBox, InitLengthTextBox, SelectedFastener);
                double wallLength = InputHelper.GetDoubleValue(InitWallLengthTextBox, 1000);
                double length = InputHelper.GetDoubleValue(InitLengthTextBox, 1000);
                double width = InputHelper.GetDoubleValue(InitWidthTextBox, 1000);
                double tDP = InputHelper.GetDoubleValue(InitTDPTextBox);
                double thermalCond = double.Parse(ThermalCondtextBox.Text, CultureInfo.InvariantCulture);
                double emissivity = GetEmissivity();
                double environmentTemp = InputHelper.GetDoubleValue(EnvTempTextBox);
                double limitTemp = InputHelper.GetDoubleValue(LimitTempTextBox);
                double heightLower = InputHelper.GetDoubleValue(OptimizeHeightTextBox, 1000);
                double heightUpper = InputHelper.GetDoubleValue(OptimizeHeightTextBoxUpTo, 1000);
                double thicknessLower = InputHelper.GetDoubleValue(OptimizeThicknessTextBox, 1000);
                double thicknessUpper = InputHelper.GetDoubleValue(OptimizeThicknessTextBoxUpTo, 1000);
                int countLower = InputHelper.GetIntValue(OptimizeRibCountTextBox);
                int countUpper = InputHelper.GetIntValue(OptimizeRibCountTextBoxUpTo);
                double baseThickness = InputHelper.GetDoubleValue(BaseThicknesstextBox, 1000);

                HeatsinkStaticParameters heatsinkStaticParameters = new(length, width, baseThickness, tDP, environmentTemp, thermalCond, emissivity);
                RibParams ribParams = new(heightLower, heightUpper, thicknessLower, thicknessUpper, countLower, countUpper);
                var GA = new OptimizationAlgorithm(heatsinkStaticParameters, ribParams, limitTemp);
                GA.PerformOptimization();
                HeightResult.Text = (GA.optimizationResult.BestHeight * 1000).ToString();
                ThicknessResult.Text = (GA.optimizationResult.BestThickness * 1000).ToString();
                CountResult.Text = GA.optimizationResult.BestCount.ToString();
                TemperatureResult.Text = Math.Round(GA.optimizationResult.ResultingTemperature, 2).ToString();
                ResultingWeightStatusTextBox.Text = GetWeight(GA, baseThickness, wallLength, length, width);
                StatusBarText.Content = GA.optimizationResult.StatusMessage;
                InputHelper.AllowRender = true;
            }
            catch (Exception ex)
            {
                HeightResult.Clear();
                ThicknessResult.Clear();
                CountResult.Clear();
                TemperatureResult.Clear();
                ResultingWeightStatusTextBox.Clear();
                StatusBarText.Content = ex.Message;
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusBarText.Content = $"Ошибка: {ex.Message}";
            }
        }

        private string GetWeight(OptimizationAlgorithm GA, double baseThickness, double wallLength, double length, double width)
        {
            double density = InputHelper.GetDoubleValue(DensityTextBox);
            double rawHeatsink = GA.optimizationResult.Volume * density;
            double holeWeight = Math.PI * Math.Pow((SelectedFastener.ThreadDiameter / 1000 / 2), 2) * baseThickness * density;
            double fasteningStrip = (wallLength - length) * width * baseThickness * density - 2 * holeWeight;
            return Math.Round(rawHeatsink + fasteningStrip, 3).ToString();
        }

        private void ChooseMaterialButton_Click(object sender, RoutedEventArgs e)
        {
            MaterialSelection materialSelection = new();
            materialSelection.ShowDialog();
            var result = materialSelection.DialogResult;
            if (result == true)
            {
                SelectedMaterial = materialSelection.SelectedMaterial;
                MaterialSelectionFields(SelectedMaterial);
            }
        }

        private void MaterialSelectionFields(Material selectedMaterial)
        {
            MaterialNameTextBox.Text = selectedMaterial.Name;
            ThermalCondtextBox.Text = selectedMaterial.ThermalConductivity.ToString();
            EmissiveRatioTextBox.Text = selectedMaterial.Emissivity.ToString();
            DensityTextBox.Text = selectedMaterial.Density.ToString();
            SelectedMaterial = selectedMaterial;
        }

        private void ClearChoiceButton_Click(object sender, RoutedEventArgs e)
        {
            MaterialNameTextBox.Clear();
            ThermalCondtextBox.Clear();
            EmissiveRatioTextBox.Clear();
            DensityTextBox.Clear();
        }
        private void ProjectsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProjectsList.SelectedItem != null && ProjectsList.SelectedItem is Project selectedProject)
            {
                SelectedProject = selectedProject;  // Используем уже проверенную и приведенную переменную
                DisplayProjectProperties(SelectedProject);
                SelectedHeatsink = databaseContext.Heatsinks.FirstOrDefault(h => h.HeatsinkID == SelectedProject.HeatsinkID);
                ProjectTabInterfaceEnablement(true);
            }
            else
            {
                ProjectTabInterfaceEnablement(false);
                SelectedHeatsink = null;
                SelectedProject = null;
            }
        }
        private void ProjectTabInterfaceEnablement(bool mode)
        {
            if (mode == true)
            {
                SetPath.IsEnabled = true;
                CreateprojectFile.IsEnabled = true;
                ChangeProjectButton.IsEnabled = true;
                RenameProjectButton.IsEnabled = true;
                DeleteProjectButton.IsEnabled = true;
                SaveMenuItem.IsEnabled = true;
            }
            else
            {
                SetPath.IsEnabled = false;
                CreateprojectFile.IsEnabled = false;
                ChangeProjectButton.IsEnabled = false;
                RenameProjectButton.IsEnabled = false;
                DeleteProjectButton.IsEnabled = false;
                SaveMenuItem.IsEnabled = false;
            }
        }
        private void DisplayProjectProperties(Project selectedProject)
        {
            ProjectNameTextBox.Text = selectedProject.ProjectName;
            CreationDatetextBox.Text = selectedProject.CreationDate.ToString();
            ChangeDateTextBox.Text = selectedProject.LastModifiedDate.ToString();
            ProjectDirTextBox.Text = selectedProject.CADFilePath;
            ProjectComment.Text = selectedProject.Description;
        }
        
        private void Rendering2DButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var autocadRender = new AutoCadRenderer
                {
                    L = InputHelper.GetDoubleValue(InitLengthTextBox),
                    D = InputHelper.GetDoubleValue(InitWidthTextBox),
                    C = InputHelper.GetDoubleValue(BaseThicknesstextBox),
                    H = InputHelper.GetDoubleValue(HeightResult),
                    Delta = InputHelper.GetDoubleValue(ThicknessResult),
                    Z = InputHelper.GetIntValue(CountResult),
                    ThreadDiameter = SelectedFastener.ThreadDiameter,
                    FasteningStrip = (InputHelper.GetDoubleValue(InitWallLengthTextBox) - InputHelper.GetDoubleValue(InitLengthTextBox)) / 2,
                    FilePath = SelectedProject.CADFilePath
                };
                autocadRender.Render();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ProjectDirFileSearchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.InitialDirectory = ProjectDirTextBox.Text;
                openFileDialog.Filter = "AutoCAD Files (*.dwg;*.dxf)|*.dwg;*.dxf|All files (*.*)|*.*";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == true)
                {
                    string selectedFileName = openFileDialog.FileName;

                    AcadApplication acadApp;
                    try
                    {
                        // Попытка получить уже запущенную инстанцию AutoCAD
                        acadApp = (AcadApplication)COMInterop.GetActiveCOMObject("AutoCAD.Application");
                    }
                    catch
                    {
                        // Если AutoCAD не запущен, создаем новую инстанцию
                        acadApp = (AcadApplication)Activator.CreateInstance(Type.GetTypeFromProgID("AutoCAD.Application"), true);
                    }
                    acadApp.Visible = true;
                    acadApp.Documents.Open(selectedFileName);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ChangeProjectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectedProject == null)
                    throw new InvalidOperationException("Проект не выбран. Выберите один из представленных проектов или создайте новый");

                InputHelper.ClearTab(InitDataTab);
                UserEmissivityCheckBox.IsChecked = false;
                InputHelper.ClearTab(DesignTab);

                ProjectInitComment.Text = SelectedProject.Description;

                InitWallLengthTextBox.Text = SelectedHeatsink.WallLength?.ToString() ?? "";
                SelectedFastener = InputHelper.FastenerList.FirstOrDefault(f => f.Name == SelectedHeatsink.FastenerType);
                FastenerSelection.SelectedItem = SelectedFastener;

                InitLengthTextBox.Text = SelectedHeatsink.Length?.ToString() ?? "";
                InitWidthTextBox.Text = SelectedHeatsink.Depth?.ToString() ?? "";
                InitTDPTextBox.Text = SelectedHeatsink.TDP?.ToString() ?? "";
                if (SelectedHeatsink.MaterialID != null)
                {
                    MaterialSelectionFields(databaseContext.Materials.FirstOrDefault(m => m.MaterialID == SelectedHeatsink.MaterialID));
                }
                if (SelectedHeatsink.Emissivity == null && SelectedHeatsink.MaterialID == null)
                {
                    UserEmissivityCheckBox.IsChecked = false;
                    UserEmissivityRatioTextBox.Text = "";
                }
                else if (SelectedHeatsink.MaterialID != null)
                {
                    if (SelectedHeatsink.Emissivity != null)
                    {
                        UserEmissivityCheckBox.IsChecked = true;
                        UserEmissivityRatioTextBox.Text = SelectedHeatsink.Emissivity.ToString();
                    }
                    else
                        UserEmissivityCheckBox.IsChecked = false;
                }
                EnvTempTextBox.Text = SelectedHeatsink.TemperatureEnvironment?.ToString() ?? "";
                LimitTempTextBox.Text = SelectedHeatsink.TemperatureLimit?.ToString() ?? "";
                OptimizeHeightTextBox.Text = SelectedHeatsink.FinHeight?.ToString() ?? "";
                OptimizeThicknessTextBox.Text = SelectedHeatsink.FinThickness?.ToString() ?? "";
                OptimizeRibCountTextBox.Text = SelectedHeatsink.NumberOfFins?.ToString() ?? "";
                BaseThicknesstextBox.Text = SelectedHeatsink.BaseThickness?.ToString() ?? "";
                HeightResult.Text = SelectedHeatsink.FinHeight?.ToString() ?? "";
                ThicknessResult.Text = SelectedHeatsink.FinThickness?.ToString() ?? "";
                CountResult.Text = SelectedHeatsink.NumberOfFins?.ToString() ?? "";
                TemperatureResult.Text = SelectedHeatsink.TemperatureAchieved?.ToString() ?? "";

                MessageBox.Show($"Проект \"{SelectedProject.ProjectName}\" успешно загружен.", "Инофрмация", MessageBoxButton.OK, MessageBoxImage.Information);
                StatusBarText.Content = $"Проект \"{SelectedProject.ProjectName}\" успешно загружен.";
                InitDataTab.Visibility = Visibility.Visible;
                DesignTab.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении проекта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void CreateProject_Click(object sender, EventArgs e)
        {
            var createProject = new CreateProject();
            if (createProject.ShowDialog() == true)
            {
                LoadProjects();
            }
        }
        private void OpenProject_Click(object sender, RoutedEventArgs e)
        {
            InterfaceTabs.SelectedItem = ProjectInfoTab;
        }
        private void SaveCurrentProject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var project = databaseContext.Projects.FirstOrDefault(p => p.ProjectID == SelectedProject.ProjectID);
                var heatsink = databaseContext.Heatsinks.FirstOrDefault(hs => hs.HeatsinkID == SelectedHeatsink.HeatsinkID);

                if (project != null)
                {
                    project.LastModifiedDate = DateTime.Now;
                    project.Description = ProjectInitComment.Text;

                    heatsink.MaterialID = SelectedMaterial != null ? SelectedMaterial.MaterialID : null;
                    heatsink.WallLength = InputHelper.GetNullableDouble(InitWallLengthTextBox);
                    heatsink.FastenerType = SelectedFastener.Name;

                    heatsink.Length = InputHelper.GetNullableDouble(LengthStatusTextBox);
                    heatsink.Depth = InputHelper.GetNullableDouble(WidthStatusTextBox);
                    heatsink.BaseThickness = InputHelper.GetNullableDouble(BaseThicknessStatusTextBox);
                    heatsink.FinHeight = InputHelper.GetNullableDouble(HeightStatusTextBox);
                    heatsink.FinThickness = InputHelper.GetNullableDouble(ThicknessStatusTextBox);
                    heatsink.NumberOfFins = InputHelper.GetNullableInt(RibCountStatusTextBox);
                    heatsink.TDP = InputHelper.GetNullableDouble(TDPStatusTextBox);
                    heatsink.Emissivity = InputHelper.GetEmissivity(EmissiveRatioTextBox, UserEmissivityRatioTextBox, UserEmissivityCheckBox);
                    heatsink.TemperatureEnvironment = InputHelper.GetNullableDouble(EnvironmentTempStatusTextBox);
                    heatsink.TemperatureLimit = InputHelper.GetNullableDouble(LimitTempStatusTextBox);
                    heatsink.TemperatureAchieved = InputHelper.GetNullableDouble(AchievedTempStatusTextBox);

                    databaseContext.SaveChanges();

                    LoadProjects();
                    ProjectsList.SelectedItem = project;
                    UpdateProjectTab();

                    MessageBox.Show($"Проект \"{SelectedProject.ProjectName}\" успешно сохранён.", "Инофрмация", MessageBoxButton.OK, MessageBoxImage.Information);
                    StatusBarText.Content = $"Проект \"{SelectedProject.ProjectName}\" успешно сохранен.";
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        private void UpdateProjectTab()
        {
            ProjectNameTextBox.Text = SelectedProject.ProjectName;
            CreationDatetextBox.Text = SelectedProject.CreationDate.ToString();
            ChangeDateTextBox.Text = SelectedProject.LastModifiedDate.ToString();
            ProjectDirTextBox.Text = SelectedProject.CADFilePath;
            ProjectComment.Text = SelectedProject.Description;
        }
        private void RemoveDescription_Click(object sender, RoutedEventArgs e)
        {
            ProjectInitComment.Clear();
        }
        private void AddDescription_Click(object sender, RoutedEventArgs e)
        {
            InputHelper.AddDescription(SelectedProject, SelectedHeatsink, SelectedMaterial, ProjectInitComment, ResultingWeightStatusTextBox.Text);
        }
        private void DeleteProjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedProject == null)
            {
                MessageBox.Show("Проект не выбран. Пожалуйста, выберите проект для удаления.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var selectedproject = SelectedProject;
            var responce = MessageBox.Show("Вы едйствительно хотите удалить выбранный проект?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (responce == MessageBoxResult.Yes)
            {
                try
                {
                    databaseContext.Projects.Remove(selectedproject);
                    databaseContext.SaveChanges();
                    MessageBox.Show("Проект успешно удалён.", "Удаление завершено", MessageBoxButton.OK, MessageBoxImage.Information);
                    InputHelper.ClearTab(ProjectInfoTab);
                    LoadProjects();
                    responce = MessageBox.Show("Выхотите удалить связанный в проектом AutoCAD .dwg файл?", "Удаление проекта", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (responce == MessageBoxResult.Yes)
                    {
                        var filePath = selectedproject.CADFilePath;
                        if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                        {
                            try
                            {
                                File.Delete(filePath);
                                MessageBox.Show("Файл успешно удален.", "Удаление файла", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Не удалось удалить файл. Ошибка: {ex.Message}", "Ошибка удаления файла", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Файл не найден или путь некорректен.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void SetPath_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new();
            openFileDialog.Filter = "AutoCAD Files (*.dwg;*.dxf)|*.dwg;*.dxf|All files (*.*)|*.*";
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == true)
            {
                SelectedProject.CADFilePath = openFileDialog.FileName;
                SelectedProject.LastModifiedDate = DateTime.Now;
                UpdateProjectTab();
                var project = databaseContext.Projects.FirstOrDefault(p => p.ProjectID == SelectedProject.ProjectID);

                if (project != null)
                {
                    project.CADFilePath = openFileDialog.FileName;
                    databaseContext.SaveChanges();
                }
            }
        }
        private void FullScreen_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState != WindowState.Maximized)
            {
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowState = WindowState.Normal;
            }
        }
        private void RestoreSize_Click(object sender, RoutedEventArgs e)
        {
            WindowStyle = WindowStyle.SingleBorderWindow;
            ResizeMode = ResizeMode.CanResize;
            WindowState = WindowState.Normal;
        }
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void PopulateFatenerList()
        {
            FastenerSelection.ItemsSource = InputHelper.FastenerList;
        }
        private void FastenerType_SelectionChanged(object sender, RoutedEventArgs e)
        {
            InputHelper.AllowRender = true;
            if (FastenerSelection.SelectedItem is Fastener selectedFastener)
            {
                FastenerTypeTextBox.Text = selectedFastener.Name;
            }
            else
            {
                FastenerTypeTextBox.Text = "";
            }
        }
        private void AutoCADLaunch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var acadApp = (AcadApplication)COMInterop.GetActiveCOMObject("AutoCAD.Application");
                acadApp.Visible = true;  // Убедимся, что AutoCAD видим
                MessageBox.Show("AutoCAD уже запущен и теперь он активирован.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {
                try
                {
                    var acadApp = (AcadApplication)Activator.CreateInstance(Type.GetTypeFromProgID("AutoCAD.Application"), true);
                    acadApp.Visible = true;  // Делаем AutoCAD видимым после запуска
                    MessageBox.Show("AutoCAD был успешно запущен.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось запустить AutoCAD: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RenameProjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedProject != null)
            {
                Rename rename = new();
                if (rename.ShowDialog() == true)
                {
                    var selectedProject = databaseContext.Projects.FirstOrDefault(p => p.ProjectID == SelectedProject.ProjectID);
                    selectedProject.ProjectName = rename.NewProjectName;
                    SelectedProject.ProjectName = rename.NewProjectName;
                    var responce = MessageBox.Show("Хотите ли вы также переименовать сопряженный файл AutoCAD?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (responce == MessageBoxResult.Yes)
                    {
                        string oldPath = SelectedProject.CADFilePath;
                        string newPath = Path.Combine(Path.GetDirectoryName(oldPath), rename.NewProjectName + Path.GetExtension(oldPath));
                        File.Move(oldPath, newPath);
                        selectedProject.CADFilePath = newPath;
                        SelectedProject.CADFilePath = newPath;
                        databaseContext.SaveChanges();
                        LoadProjects();
                        ProjectsList.SelectedItem = selectedProject;
                        UpdateProjectTab();
                    }
                }
            }
        }
        private void CreateProjectFile_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedProject != null)
            {
                var selectedProject = databaseContext.Projects.FirstOrDefault(p => p.ProjectID == SelectedProject.ProjectID);
                if (File.Exists(SelectedProject.CADFilePath))
                {
                    var responce = MessageBox.Show("У данного проекта уже есть ассоциированный файл. Выхотите его удалить?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (responce == MessageBoxResult.Yes)
                    {
                        File.Delete(selectedProject.CADFilePath);
                        selectedProject.CADFilePath = null;
                        SelectedProject = selectedProject;
                        databaseContext.SaveChanges();
                        LoadProjects();
                        ProjectsList.SelectedItem = selectedProject;
                        UpdateProjectTab();
                    }
                }
                SaveFileDialog saveFileDialog = new();
                saveFileDialog.Filter = "AutoCAD Files (*.dwg)|*.dwg"; ;
                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                saveFileDialog.Title = "Создать файл AutoCAD";
                if (saveFileDialog.ShowDialog() == true)
                {
                    string sourcePath = @"Resources\A3 Stamp.dwg";
                    string FileName = saveFileDialog.FileName;
                    try
                    {
                        File.Copy(sourcePath, FileName, true);
                        MessageBox.Show("Файл успешно создан.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        selectedProject.CADFilePath = FileName;
                        SelectedProject = selectedProject;
                        databaseContext.SaveChanges();
                        LoadProjects();
                        ProjectsList.SelectedItem = selectedProject;
                        UpdateProjectTab();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при создании файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        private void InputHelper_OnAllowRenderChanged(object sender, EventArgs e)
        {
            Rendering2DButton.IsEnabled = InputHelper.AllowRender;
            RenderAutoCADMenuItem.IsEnabled = InputHelper.AllowRender;

            var backgroundColor = InputHelper.AllowRender ? new SolidColorBrush(Color.FromRgb(220, 255, 204)) : Brushes.White;
            WallLengthStatusTextBox.Background = backgroundColor;
            LengthStatusTextBox.Background = backgroundColor;
            WidthStatusTextBox.Background = backgroundColor;
            BaseThicknessStatusTextBox.Background = backgroundColor;
            HeightStatusTextBox.Background = backgroundColor;
            ThicknessStatusTextBox.Background = backgroundColor;
            RibCountStatusTextBox.Background = backgroundColor;
            TDPStatusTextBox.Background = backgroundColor;
            EmissivityStatusTextBox.Background = backgroundColor;
            ThermalConductStatusTextBox.Background = backgroundColor;
            EnvironmentTempStatusTextBox.Background = backgroundColor;
            AchievedTempStatusTextBox.Background = backgroundColor;
            LimitTempStatusTextBox.Background = backgroundColor;
            MaterialStatusTextBox.Background = backgroundColor;
            FastenerTypeTextBox.Background = backgroundColor;
            ResultingWeightStatusTextBox.Background = backgroundColor;
        }
        private void HelpMenuItem_Click(object sender, RoutedEventArgs e)
        {
            HelpWindow helpWindow = new HelpWindow();
            helpWindow.ShowDialog(); 
        }
        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }
        private void FeedbackMenuItem_Click(object sender, RoutedEventArgs e)
        {
            FeedbackWindow feedbackWindow = new FeedbackWindow();
            feedbackWindow.ShowDialog();
        }
    }
}

