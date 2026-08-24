using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ExperienceHost.DataAccess.SQL.Entities;

namespace StartMenuExperienceHostEx.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private ObservableCollection<ApplicationViewModel> _applications = new();

    public MainWindowViewModel()
    {
        LoadApplications();
    }

    private void LoadApplications()
    {
        var entities = new List<EntityApplication>
        {
            new EntityApplication
            {
                Name = "curl",
                ImagePath = @"C:\Users\UnderKo\Downloads\pf-dYL6WJUk (1).jpg",
                Disk = "C:",
                FilePath = @"F:\curl\curl.exe"
            },
            new EntityApplication
            {
                Name = "Visual Studio 2022",
                ImagePath = @"C:\Images\vs.jpg",
                Disk = "C:",
                FilePath = @"C:\Program Files\Microsoft Visual Studio\devenv.exe"
            },
            new EntityApplication
            {
                Name = "Google Chrome",
                ImagePath = @"C:\Images\chrome.jpg",
                Disk = "C:",
                FilePath = @"C:\Program Files\Google\Chrome\chrome.exe"
            },
        };

        foreach (var entity in entities)
        {
            Applications.Add(new ApplicationViewModel(entity));
        }
    }
}