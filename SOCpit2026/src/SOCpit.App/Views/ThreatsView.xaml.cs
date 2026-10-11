using System.Windows.Controls;
using SOCpit.App.ViewModels;

namespace SOCpit.App.Views;

public partial class ThreatsView : UserControl
{
    public ThreatsView()
    {
        InitializeComponent();
    }

    public ThreatsView(ThreatsViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
