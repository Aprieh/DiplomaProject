using System.Windows;

namespace DiplomaProject
{
    public partial class Rename : Window
    {
        public string NewProjectName { get; set; }
        public string WindowPurpose { get; set; } = "Переименовать";
        public Rename()
        {
            InitializeComponent();
            DataContext = this;
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            NewProjectName = NameInput.Text;
            Close();
        }
    }
}
