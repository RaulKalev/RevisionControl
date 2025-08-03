using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

public class ThemeManager
{
    private const string ConfigFilePath = @"C:\ProgramData\RK Tools\RevisionControl\config.json";
    private readonly Window _window;
    private bool _isDarkMode = true;

    public bool IsDarkMode
    {
        get => _isDarkMode;
        set { _isDarkMode = value; SaveConfig(); }
    }

    public double WindowWidth { get; set; } = 1200;
    public double WindowHeight { get; set; } = 900;
    public double WindowLeft { get; set; } = 100;
    public double WindowTop { get; set; } = 100;

    public event EventHandler ThemeChanged;

    public ThemeManager(Window window)
    {
        _window = window;
        LoadConfig();
        ApplyTheme();
    }

    public void ToggleTheme()
    {
        _isDarkMode = !_isDarkMode;
        ApplyTheme();
        SaveConfig();
        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ApplyTheme()
    {
        var assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        var themeUri = _isDarkMode
            ? $"pack://application:,,,/{assemblyName};component/Themes/DarkTheme.xaml"
            : $"pack://application:,,,/{assemblyName};component/Themes/LightTheme.xaml";

        try
        {
            var resourceDict = new ResourceDictionary { Source = new Uri(themeUri, UriKind.Absolute) };
            _window.Resources.MergedDictionaries.Clear();
            _window.Resources.MergedDictionaries.Add(resourceDict);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Failed to load theme: {ex.Message}", "Theme Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void LoadConfig()
    {
        try
        {
            if (File.Exists(ConfigFilePath))
            {
                var json = File.ReadAllText(ConfigFilePath);
                var config = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                if (config != null)
                {
                    if (config.ContainsKey("IsDarkMode")) _isDarkMode = Convert.ToBoolean(config["IsDarkMode"]);
                    if (config.ContainsKey("WindowWidth")) WindowWidth = Convert.ToDouble(config["WindowWidth"]);
                    if (config.ContainsKey("WindowHeight")) WindowHeight = Convert.ToDouble(config["WindowHeight"]);
                    if (config.ContainsKey("WindowLeft")) WindowLeft = Convert.ToDouble(config["WindowLeft"]);
                    if (config.ContainsKey("WindowTop")) WindowTop = Convert.ToDouble(config["WindowTop"]);

                }
            }
        }
        catch { /* ignore */ }
    }

    public void SaveConfig()
    {
        try
        {
            var config = new Dictionary<string, object>
            {
                ["IsDarkMode"] = _isDarkMode,
                ["WindowWidth"] = _window.Width,
                ["WindowHeight"] = _window.Height,
                ["WindowLeft"] = _window.Left,
                ["WindowTop"] = _window.Top
            };
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigFilePath));
            var json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(ConfigFilePath, json);
        }
        catch { /* ignore */ }
    }

}
