using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using ImagePlatformSorter.Models;
using ImagePlatformSorter.Services;
using Forms = System.Windows.Forms;

namespace ImagePlatformSorter.Views;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private readonly ConfigService _configService = new();
    private readonly ImageSortingService _sortingService = new();
    private string _inputFolder = string.Empty;
    private string _outputFolder = string.Empty;
    private string _runLog = "Р—РґРµСЃСЊ РїРѕСЏРІРёС‚СЃСЏ СЂРµР·СѓР»СЊС‚Р°С‚ СЃРѕСЂС‚РёСЂРѕРІРєРё.";
    private bool _overwriteExisting;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        LoadPlatforms();
    }

    public ObservableCollection<PlatformSelectionItem> Platforms { get; } = new();

    public string InputFolder
    {
        get => _inputFolder;
        set
        {
            if (_inputFolder == value)
            {
                return;
            }

            _inputFolder = value;
            OnPropertyChanged();
        }
    }

    public string OutputFolder
    {
        get => _outputFolder;
        set
        {
            if (_outputFolder == value)
            {
                return;
            }

            _outputFolder = value;
            OnPropertyChanged();
        }
    }

    public string RunLog
    {
        get => _runLog;
        set
        {
            if (_runLog == value)
            {
                return;
            }

            _runLog = value;
            OnPropertyChanged();
        }
    }

    public bool OverwriteExisting
    {
        get => _overwriteExisting;
        set
        {
            if (_overwriteExisting == value)
            {
                return;
            }

            _overwriteExisting = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void LoadPlatforms()
    {
        Platforms.Clear();

        foreach (var config in _configService.Load())
        {
            Platforms.Add(new PlatformSelectionItem(config));
        }

        RunLog = Platforms.Count == 0
            ? $"РљРѕРЅС„РёРі Р·Р°РіСЂСѓР¶РµРЅ, РЅРѕ СЃРїРёСЃРѕРє РїР»РѕС‰Р°РґРѕРє РїСѓСЃС‚: {_configService.ConfigPath}"
            : $"РљРѕРЅС„РёРі Р·Р°РіСЂСѓР¶РµРЅ: {_configService.ConfigPath}";
    }

    private void BrowseInputFolder_Click(object sender, RoutedEventArgs e)
    {
        var selectedFolder = PickFolder(InputFolder);

        if (string.IsNullOrWhiteSpace(selectedFolder))
        {
            return;
        }

        InputFolder = selectedFolder;

        if (string.IsNullOrWhiteSpace(OutputFolder))
        {
            OutputFolder = Path.Combine(selectedFolder, "Sorted");
        }
    }

    private void BrowseOutputFolder_Click(object sender, RoutedEventArgs e)
    {
        var selectedFolder = PickFolder(OutputFolder);

        if (!string.IsNullOrWhiteSpace(selectedFolder))
        {
            OutputFolder = selectedFolder;
        }
    }

    private void OpenAdminWindow_Click(object sender, RoutedEventArgs e)
    {
        var adminWindow = new AdminWindow(_configService)
        {
            Owner = this
        };

        if (adminWindow.ShowDialog() == true)
        {
            LoadPlatforms();
        }
    }

    private async void RunSorting_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(InputFolder) || !Directory.Exists(InputFolder))
        {
            System.Windows.MessageBox.Show(
                this,
                "Р’С‹Р±РµСЂРёС‚Рµ СЃСѓС‰РµСЃС‚РІСѓСЋС‰СѓСЋ РїР°РїРєСѓ СЃ РёР·РѕР±СЂР°Р¶РµРЅРёСЏРјРё.",
                "РќРµ РІС‹Р±СЂР°РЅР° РІС…РѕРґРЅР°СЏ РїР°РїРєР°",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var selectedPlatforms = Platforms
            .Where(static item => item.IsSelected)
            .Select(static item => item.Config.Clone())
            .ToList();

        if (selectedPlatforms.Count == 0)
        {
            System.Windows.MessageBox.Show(
                this,
                "РћС‚РјРµС‚СЊС‚Рµ С…РѕС‚СЏ Р±С‹ РѕРґРЅСѓ РїР»РѕС‰Р°РґРєСѓ РґР»СЏ СЃРѕСЂС‚РёСЂРѕРІРєРё.",
                "РќРµ РІС‹Р±СЂР°РЅС‹ РїР»РѕС‰Р°РґРєРё",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        try
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            RunLog = "РРґС‘С‚ СЃРѕСЂС‚РёСЂРѕРІРєР°...";

            var request = new SortRequest
            {
                InputFolder = InputFolder,
                OutputFolder = string.IsNullOrWhiteSpace(OutputFolder) ? null : OutputFolder,
                Platforms = selectedPlatforms,
                OverwriteExisting = OverwriteExisting
            };

            var result = await Task.Run(() => _sortingService.Sort(request));
            RunLog = result.ToMultilineText();

            System.Windows.MessageBox.Show(
                this,
                $"РЎРѕСЂС‚РёСЂРѕРІРєР° Р·Р°РІРµСЂС€РµРЅР°.\n\nР’С‹С…РѕРґРЅР°СЏ РїР°РїРєР°:\n{result.OutputFolder}",
                "Р“РѕС‚РѕРІРѕ",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            RunLog = $"РћС€РёР±РєР°: {ex.Message}";

            System.Windows.MessageBox.Show(
                this,
                ex.Message,
                "РћС€РёР±РєР°",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }
    }

    private static string? PickFolder(string? initialPath)
    {
        using var dialog = new Forms.FolderBrowserDialog
        {
            SelectedPath = Directory.Exists(initialPath) ? initialPath : Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            UseDescriptionForTitle = true,
            Description = "Р’С‹Р±РµСЂРёС‚Рµ РїР°РїРєСѓ"
        };

        return dialog.ShowDialog() == Forms.DialogResult.OK
            ? dialog.SelectedPath
            : null;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
