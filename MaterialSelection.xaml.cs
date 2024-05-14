using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace DiplomaProject
{
    public partial class MaterialSelection : Window
    {
        private DatabaseContext databaseContext = new();
        public ObservableCollection<Material> Materials { get; } = new ObservableCollection<Material>();
        public Material SelectedMaterial { get; private set; }
        public MaterialSelection()
        {
            InitializeComponent();
            InitialState();
            DataContext = this;
            LoadMaterials();
        }
        private void InitialState()
        {
            NameTextBox.TextChanged += InputHelper.MaterialTextCheckSpecialCase;
            ThermalCondTextBox.TextChanged += InputHelper.CommonTextCheckSpesialCase;
            DensityTextBox.TextChanged += InputHelper.CommonTextCheckSpesialCase;
            EmissivityTextBox.TextChanged += InputHelper.CommonTextCheckSpesialCase;
        }
        private void LoadMaterials()
        {
            Materials.Clear();
            List<Material> materialsFromDB = databaseContext.Materials.ToList();
            foreach (Material material in materialsFromDB)
            {
                Materials.Add(material);
            }
        }
        private void MaterialsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MaterialsList.SelectedItem is Material selectedMaterial)
            {
                NameTextBox.Text = selectedMaterial.Name;
                ThermalCondTextBox.Text = selectedMaterial.ThermalConductivity.ToString();
                DensityTextBox.Text = selectedMaterial.Density.ToString();
                EmissivityTextBox.Text = selectedMaterial.Emissivity.ToString();
                DeleteButton.IsEnabled = ChangeButton.IsEnabled = ApplyMaterialButton.IsEnabled = true;
            }
        }
        
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddMaterial addMaterial = new();
            addMaterial.MaterialAdded += (s, args) => LoadMaterials();
            addMaterial.Show();
        }
        private bool IsMaterialUsed(int materialId)
        {
            return databaseContext.Heatsinks.Any(h => h.MaterialID == materialId);
        }
        private void DeleteMaterialButtonClick(object sender, RoutedEventArgs e)
        {
            var selectedMaterial = MaterialsList.SelectedItem as Material;
            if (selectedMaterial != null)
            {
                if (IsMaterialUsed(selectedMaterial.MaterialID))
                {
                    MessageBox.Show("Этот материал используется в одном или нескольких проектах. Удаление невозможно", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                else
                {
                    var response = MessageBox.Show("Вы уверены, что хотите удалить этот материал?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (response == MessageBoxResult.Yes)
                    {
                        NameTextBox.Text = "";
                        ThermalCondTextBox.Text = "";
                        DensityTextBox.Text = "";
                        EmissivityTextBox.Text = "";
                        databaseContext.Materials.Remove(selectedMaterial);
                        databaseContext.SaveChanges();
                        LoadMaterials();
                    }
                }
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            databaseContext.Dispose();
            Close();
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            databaseContext.Dispose();
            Close();
        }
        private void ChangeButton_Click(object sender, RoutedEventArgs e)
        {
            NameTextBox.IsReadOnly = false;
            ThermalCondTextBox.IsReadOnly = false;
            DensityTextBox.IsReadOnly = false;
            EmissivityTextBox.IsReadOnly = false;

            SaveChange.Visibility = Visibility.Visible;
            CancelChange.Visibility = Visibility.Visible;
            AddButton.IsEnabled = false;
            DeleteButton.IsEnabled = false;
            ChangeButton.IsEnabled = false;
            ApplyMaterialButton.IsEnabled = false;
            CancelButton.IsEnabled = false;
            MaterialsList.IsEnabled = false;
        }
        private void SaveChange_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialsList.SelectedItem is Material selectedMaterial)
            {
                try
                {
                    // Получаем выбранный материал и обновляем его свойства
                    selectedMaterial.Name = InputHelper.getStringValue(NameTextBox);
                    selectedMaterial.ThermalConductivity = InputHelper.GetDoubleValue(ThermalCondTextBox); 
                    selectedMaterial.Density = InputHelper.GetDoubleValue(DensityTextBox);
                    selectedMaterial.Emissivity = InputHelper.GetDoubleValue(EmissivityTextBox);
                    // Обновляем материал в контексте базы данных
                    databaseContext.Materials.Update(selectedMaterial);
                    databaseContext.SaveChanges();
                    // Обновляем список материалов в интерфейсе пользователя
                    LoadMaterials();
                    // Отображаем сообщение об успешном обновлении
                    MessageBox.Show("Материал успешно обновлен", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    SetFieldsBack();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка при обновлении материала: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void CancelChange_Click(object sender, RoutedEventArgs e)
        {
            SetFieldsBack();
        }

        private void SetFieldsBack()
        {
            NameTextBox.IsReadOnly = true;
            ThermalCondTextBox.IsReadOnly = true;
            DensityTextBox.IsReadOnly = true;
            EmissivityTextBox.IsReadOnly = true;

            SaveChange.Visibility = Visibility.Hidden;
            CancelChange.Visibility = Visibility.Hidden;
            if (MaterialsList.SelectedItem is Material selectedMaterial)
            {
                NameTextBox.Text = selectedMaterial.Name;
                ThermalCondTextBox.Text = selectedMaterial.ThermalConductivity.ToString();
                DensityTextBox.Text = selectedMaterial.Density.ToString();
                EmissivityTextBox.Text = selectedMaterial.Emissivity.ToString();
            }
            AddButton.IsEnabled = true;
            DeleteButton.IsEnabled = true;
            ChangeButton.IsEnabled = true;
            ApplyMaterialButton.IsEnabled = true;
            CancelButton.IsEnabled = true;
            MaterialsList.IsEnabled = true;
        }
        private void ApplyMaterialButton_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialsList.SelectedItem is Material selectedMaterial)
            {
                SelectedMaterial = selectedMaterial;
                DialogResult = true;
            }
        }
    }
}
