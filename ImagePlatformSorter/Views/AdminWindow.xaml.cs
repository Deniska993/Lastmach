using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using ImagePlatformSorter.Models;
using ImagePlatformSorter.Services;

namespace ImagePlatformSorter.Views;

public partial class AdminWindow : Window, INotifyPropertyChanged
{
    private readonly ConfigService _configService;
    private PlatformConfig? _selectedPlatform;

    public AdminWindow(ConfigService configService)
    {
        _configService = configService;
        InitializeComponent();
        DataContext = this;

        foreach (var platform in _configService.Load())
        {
            Platforms.Add(platform.Clone());
        }

        ConfigPathDisplay = $"Файл настроек: {_configService.ConfigPath}";
    }

    public ObservableCollection<PlatformConfig> Platforms { get; } = new();

    public string ConfigPathDisplay { get; private set; } = string.Empty;

    public PlatformConfig? SelectedPlatform
    {
        get => _selectedPlatform;
        set
        {
            if (_selectedPlatform == value)
            {
                return;
            }

            _selectedPlatform = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void AddPlatform_Click(object sender, RoutedEventArgs e)
    {
        var editor = new PlatformEditorWindow(null)
        {
            Owner = this
        };

        if (editor.ShowDialog() != true || editor.Result is null)
        {
            return;
        }

        if (HasDuplicateName(editor.Result.Name))
        {
            ShowDuplicateNameMessage(editor.Result.Name);
            return;
        }

        Platforms.Add(editor.Result);
        SelectedPlatform = editor.Result;
    }

    private void EditPlatform_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedPlatform is null)
        {
            System.Windows.MessageBox.Show(
                this,
                "Сначала выберите площадку в списке.",
                "Нет выбранной площадки",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var currentName = SelectedPlatform.Name;

        var editor = new PlatformEditorWindow(SelectedPlatform.Clone())
        {
            Owner = this
        };

        if (editor.ShowDialog() != true || editor.Result is null)
        {
            return;
        }

        if (HasDuplicateName(editor.Result.Name, currentName))
        {
            ShowDuplicateNameMessage(editor.Result.Name);
            return;
        }

        var index = Platforms.IndexOf(SelectedPlatform);
        Platforms[index] = editor.Result;
        SelectedPlatform = editor.Result;
    }

    private void DeletePlatform_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedPlatform is null)
        {
            System.Windows.MessageBox.Show(
                this,
                "Сначала выберите площадку в списке.",
                "Нет выбранной площадки",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var result = System.Windows.MessageBox.Show(
            this,
            $"Удалить площадку \"{SelectedPlatform.Name}\"?",
            "Подтверждение удаления",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        Platforms.Remove(SelectedPlatform);
        SelectedPlatform = null;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        _configService.Save(Platforms);
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => Close();

    private bool HasDuplicateName(string name, string? ignoreName = null) =>
        Platforms.Any(platform =>
            !string.Equals(platform.Name, ignoreName, StringComparison.CurrentCultureIgnoreCase) &&
            string.Equals(platform.Name, name, StringComparison.CurrentCultureIgnoreCase));

    private void ShowDuplicateNameMessage(string platformName)
    {
        System.Windows.MessageBox.Show(
            this,
            $"Площадка с названием \"{platformName}\" уже существует.",
            "Дублирующееся название",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

