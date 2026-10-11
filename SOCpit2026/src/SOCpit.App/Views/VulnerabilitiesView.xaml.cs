using System.Windows.Controls;
using SOCpit.App.ViewModels;

namespace SOCpit.App.Views;

public partial class VulnerabilitiesView : UserControl
{
    public VulnerabilitiesView()
    {
        InitializeComponent();
    }

    public VulnerabilitiesView(VulnerabilitiesViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
