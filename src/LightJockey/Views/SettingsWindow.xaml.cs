using System.Windows;

namespace LightJockey.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow(object dataContext)
        {
            InitializeComponent();
            DataContext = dataContext;
        }
    }
}
