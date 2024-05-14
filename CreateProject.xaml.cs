using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace DiplomaProject
{
    public partial class CreateProject : Window
    {
        public CreateProject()
        {
            InitializeComponent();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {

            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string templateFilePath = System.IO.Path.Combine(appDirectory, "Resources", "A3 Stamp.dwg");
            string newFilePath = System.IO.Path.Combine(ProjectPathTextBox.Text, $"{ProjectNameTextBox.Text}.dwg");
            try
            {
                using (DatabaseContext databaseContext = new())
                {
                    var heatsink = new Heatsink();
                    databaseContext.Heatsinks.Add(heatsink);
                    databaseContext.SaveChanges();

                    var project = new Project
                    {
                        ProjectName = ProjectNameTextBox.Text,
                        HeatsinkID = heatsink.HeatsinkID,
                        CreationDate = DateTime.Now,
                        LastModifiedDate = DateTime.Now,
                        CADFilePath = newFilePath,
                        Description = "",
                    };
                    databaseContext.Add(project);
                    databaseContext.SaveChanges();
                }
                System.IO.File.Copy(templateFilePath, newFilePath, overwrite: true);
                MessageBox.Show($"Проект \"{ProjectNameTextBox.Text}\" успешно создан!", "Инофрмация", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания проекта \"{ProjectNameTextBox.Text}\". Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            // Создание нового экземпляра диалога выбора папки
            using (CommonOpenFileDialog dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true; // Указываем, что нужно выбрать папку, а не файл
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop); // Устанавливаем начальную папку

                // Отображение диалога и проверка результата
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    // Если пользователь выбрал папку, отобразить путь в текстовом поле
                    ProjectPathTextBox.Text = dialog.FileName;
                }
            }
            Activate();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
