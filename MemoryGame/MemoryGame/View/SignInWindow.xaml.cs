using System.Windows;

namespace MemoryGame.View
{
    public partial class SignInWindow : Window
    {
        public SignInWindow()
        {
            InitializeComponent();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            // Using the command from the ViewModel
            if (DataContext is ViewModel.SignInViewModel viewModel)
            {
                if (viewModel.ExitCommand.CanExecute(null))
                {
                    viewModel.ExitCommand.Execute(null);
                }
            }
            else
            {
                // Fallback if ViewModel is not available
                Application.Current.Shutdown();
            }
        }
    }
}