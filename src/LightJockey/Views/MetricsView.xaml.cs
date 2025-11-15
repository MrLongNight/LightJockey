using System.Windows.Controls;
using LightJockey.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LightJockey.Views
{
    /// <summary>
    /// Interaction logic for MetricsView.xaml
    /// </summary>
    public partial class MetricsView : UserControl
    {
        public MetricsView()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetRequiredService<MetricsViewModel>();
        }
    }
}
