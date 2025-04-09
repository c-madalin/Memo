using System.Windows;
using MemoryGame.ViewModel;

namespace MemoryGame.View
{
    public partial class StatisticsWindow : Window
    {
        public StatisticsWindow()
        {
            InitializeComponent();

            // Make sure we're creating a new instance of the ViewModel
            this.DataContext = new StatisticsViewModel();
        }
    }
}