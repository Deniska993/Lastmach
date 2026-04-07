using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using ImagePlatformSorter.Models;

namespace ImagePlatformSorter.Views;

public partial class PlatformEditorWindow : Window, INotifyPropertyChanged
{
    private string _platformName = string.Empty;
    private string _maxFileSizeKbText = string.Empty;
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
        MaxFileSizeKbText = platform.MaxFileSizeKb?.ToString() ?? string.Empty;
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

    public string MaxFileSizeKbText
    {
        get => _maxFileSizeKbText;
        set
        {
            if (_maxFileSizeKbText == value)
            {
                return;
            }

            _maxFileSizeKbText = value;
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
                "Р’РІРµРґРёС‚Рµ РЅР°Р·РІР°РЅРёРµ РїР»РѕС‰Р°РґРєРё.",
                "РџСѓСЃС‚РѕРµ РЅР°Р·РІР°РЅРёРµ",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var sizes = PlatformConfig.ExtractSizes(SizesText).ToList();

        if (sizes.Count == 0)
        {
            System.Windows.MessageBox.Show(
                this,
                "Р”РѕР±Р°РІСЊС‚Рµ С…РѕС‚СЏ Р±С‹ РѕРґРёРЅ СЂР°Р·РјРµСЂ.",
                "РџСѓСЃС‚РѕР№ СЃРїРёСЃРѕРє СЂР°Р·РјРµСЂРѕРІ",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        int? maxFileSizeKb = null;
        var maxFileSizeText = MaxFileSizeKbText.Trim();

        if (!string.IsNullOrWhiteSpace(maxFileSizeText))
        {
            if (!int.TryParse(maxFileSizeText, out var parsedMaxFileSizeKb) || parsedMaxFileSizeKb <= 0)
            {
                System.Windows.MessageBox.Show(
                    this,
                    "Р’РІРµРґРёС‚Рµ РєРѕСЂСЂРµРєС‚РЅС‹Р№ Р»РёРјРёС‚ РІРµСЃР° РІ РљР‘ РёР»Рё РѕСЃС‚Р°РІСЊС‚Рµ РїРѕР»Рµ РїСѓСЃС‚С‹Рј.",
                    "РќРµРєРѕСЂСЂРµРєС‚РЅС‹Р№ Р»РёРјРёС‚ РІРµСЃР°",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            maxFileSizeKb = parsedMaxFileSizeKb;
        }

        Result = new PlatformConfig
        {
            Name = name,
            Sizes = sizes,
            MaxFileSizeKb = maxFileSizeKb
        };

        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => Close();

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
