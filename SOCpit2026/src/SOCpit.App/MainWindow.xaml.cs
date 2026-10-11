// SOCpit - Security Operations Center
// Main window for WPF application
// Based on MATRIX V3 with MVVM pattern

using Microsoft.Extensions.DependencyInjection;
using SOCpit.App.ViewModels;
using System.Windows;

namespace SOCpit.App
{
    /// <summary>Interaction logic for MainWindow.xaml</summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if (App.ServiceProvider != null)
            {
                DataContext = App.ServiceProvider.GetRequiredService<MainViewModel>();
            }
        }

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
