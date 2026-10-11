using System.Windows.Controls;
using SOCpit.App.ViewModels;

namespace SOCpit.App.Views;

public partial class IncidentsView : UserControl
{
    public IncidentsView()
    {
        InitializeComponent();
    }

    public IncidentsView(IncidentsViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
