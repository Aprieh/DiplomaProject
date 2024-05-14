using System.Windows;

namespace DiplomaProject
{
    public partial class AddMaterial : Window
    {
        private DatabaseContext databaseContext = new();
        public bool isAdded { get; private set; }
        public event EventHandler MaterialAdded;
        protected virtual void OnMaterialAdded()
        {
            MaterialAdded?.Invoke(this, EventArgs.Empty);
        }
        public AddMaterial()
        {
            InitializeComponent();
            InitialState();
        }
        private void InitialState()
        {
            InputHelper.MaterialTextCheck(NameTextBox);
            InputHelper.CommonTextCheck(ThermalCondTextBox);
            InputHelper.CommonTextCheck(DensityTextBox);
            InputHelper.CommonTextCheck(EmissivityTextBox);
            NameTextBox.TextChanged += InputHelper.MaterialTextCheck;
            ThermalCondTextBox.TextChanged += InputHelper.CommonTextCheck;
            DensityTextBox.TextChanged += InputHelper.CommonTextCheck;
            EmissivityTextBox.TextChanged += InputHelper.CommonTextCheck;
        }
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Material material = new()
                {
                    Name = InputHelper.getStringValue(NameTextBox),
                    ThermalConductivity = InputHelper.GetDoubleValue(ThermalCondTextBox),
                    Density = InputHelper.GetDoubleValue(DensityTextBox),
                    Emissivity = InputHelper.GetDoubleValue(EmissivityTextBox)
                };
                bool exists = databaseContext.Materials.Any(m => m.Name == material.Name);
                if (exists)
                {
                    MessageBox.Show("Материал с таким именем уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                databaseContext.Add(material);
                databaseContext.SaveChanges();
                MessageBox.Show("Материал успешно добавлен", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                NameTextBox.Clear();
                ThermalCondTextBox.Clear();
                DensityTextBox.Clear();
                EmissivityTextBox.Clear();
                isAdded = true;
                OnMaterialAdded();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка ввода: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            databaseContext.Dispose();
            Close();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            databaseContext.Dispose();
        }
    }
}
