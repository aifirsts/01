using System.Windows.Controls;
using SOCpit.App.ViewModels;

namespace SOCpit.App.Views;

public partial class NetworksView : UserControl
{
    public NetworksView()
    {
        InitializeComponent();
    }

    public NetworksView(NetworksViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
