using System.Windows.Controls;
using SOCpit.App.ViewModels;

namespace SOCpit.App.Views;

public partial class ReportsView : UserControl
{
    public ReportsView()
    {
        InitializeComponent();
    }

    public ReportsView(ReportsViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
