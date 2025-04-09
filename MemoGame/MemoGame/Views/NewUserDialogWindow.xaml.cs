using System.Windows;
using System.Windows.Controls;
using MemoGame.ViewModels;

namespace MemoGame.Views
{
    /// <summary>
    /// Interaction logic for NewUserDialogWindow.xaml
    /// </summary>
    public partial class NewUserDialogWindow : UserControl
    {
        public NewUserDialogWindow()
        {
            InitializeComponent();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as NewUserDialogViewModel;
            if (viewModel != null)
            {
                viewModel.WindowCloseRequested?.Invoke(true);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as NewUserDialogViewModel;
            if (viewModel != null)
            {
                viewModel.WindowCloseRequested?.Invoke(false);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as NewUserDialogViewModel;
            if (viewModel != null)
            {
                viewModel.WindowCloseRequested?.Invoke(false);
            }
        }
    }
}