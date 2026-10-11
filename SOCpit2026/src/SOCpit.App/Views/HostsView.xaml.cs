using System.Windows.Controls;
using SOCpit.App.ViewModels;

namespace SOCpit.App.Views;

public partial class HostsView : UserControl
{
    public HostsView()
    {
        InitializeComponent();
    }

    public HostsView(HostsViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
