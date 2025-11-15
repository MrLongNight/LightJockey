using CommunityToolkit.Mvvm.ComponentModel;

namespace LightJockey.Models;

public partial class AppSettings : ObservableObject
{
    [ObservableProperty]
    private string _logLevel = "Information";

    [ObservableProperty]
    private int _retainedLogFileCount = 10;
}
