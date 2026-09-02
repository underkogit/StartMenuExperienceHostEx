using System.IO;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using ExperienceHost.DataAccess.SQL.Entities;

namespace StartMenuExperienceHostEx.ViewModels;

public partial class ApplicationItemViewModel : ViewModelBase
{
    private readonly EntityApplication _entity;

    [ObservableProperty] private Bitmap? _image;
    [ObservableProperty] private double _x;

    [ObservableProperty] private double _y;

    public ApplicationItemViewModel(EntityApplication entity)
    {
        _entity = entity;
        LoadImage();
    }

    public string Name => _entity.Name;
    public string Disk => _entity.Disk;
    public string FilePath => _entity.FilePath;
    public string FullFilePath => Path.Combine(_entity.Disk, _entity.FilePath);
    public string ImagePath => _entity.ImagePath;

    private void LoadImage()
    {
        try
        {
            if (!string.IsNullOrEmpty(_entity.ImagePath) && System.IO.File.Exists(_entity.ImagePath))
            {
                Image = new Bitmap(_entity.ImagePath);
            }
        }
        catch
        {
            Image = null;
        }
    }
}