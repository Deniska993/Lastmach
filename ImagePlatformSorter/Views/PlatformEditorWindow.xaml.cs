using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using ImagePlatformSorter.Models;

namespace ImagePlatformSorter.Views;

public partial class PlatformEditorWindow : Window, INotifyPropertyChanged
{
    private string _platformName = string.Empty;
    private string _sizesText = string.Empty;

    public PlatformEditorWindow(PlatformConfig? platform)
    {
        InitializeComponent();
        DataContext = this;

        if (platform is null)
        {
            return;
        }

        PlatformName = platform.Name;
        SizesText = string.Join(Environment.NewLine, platform.NormalizedSizes);
    }

    public string PlatformName
    {
        get => _platformName;
        set
        {
            if (_platformName == value)
            {
                return;
            }

            _platformName = value;
            OnPropertyChanged();
        }
    }

    public string SizesText
    {
        get => _sizesText;
        set
        {
            if (_sizesText == value)
            {
                return;
            }

            _sizesText = value;
            OnPropertyChanged();
        }
    }

    public PlatformConfig? Result { get; private set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var name = PlatformName.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            System.Windows.MessageBox.Show(
                this,
                "Введите название площадки.",
                "Пустое название",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var sizes = PlatformConfig.ExtractSizes(SizesText).ToList();

        if (sizes.Count == 0)
        {
            System.Windows.MessageBox.Show(
                this,
                "Добавьте хотя бы один размер.",
                "Пустой список размеров",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        Result = new PlatformConfig
        {
            Name = name,
            Sizes = sizes
        };

        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => Close();

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
