using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ImagePlatformSorter.Models;

public sealed class PlatformSelectionItem : INotifyPropertyChanged
{
    private bool _isSelected;

    public PlatformSelectionItem(PlatformConfig config)
    {
        Config = config;
    }

    public PlatformConfig Config { get; }

    public string Name => Config.Name;

    public string SizesDisplay => Config.SizesDisplay;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value)
            {
                return;
            }

            _isSelected = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

