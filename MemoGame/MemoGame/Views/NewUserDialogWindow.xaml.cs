using System.Windows;

namespace MemoGame.Views
{
    /// <summary>
    /// Interaction logic for NewUserDialogWindow.xaml
    /// </summary>
    public partial class NewUserDialogWindow : Window
    {
        public NewUserDialogWindow()
        {
            InitializeComponent();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}