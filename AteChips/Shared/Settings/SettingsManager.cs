using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using AteChips.Core.Shared.Interfaces;
using AteChips.Shared.Settings;

namespace Shared.Settings;

public static class SettingsManager
{
    private static readonly string _settingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "AteChips",
        "config.json"
    );

    public static Chip8Settings Current { get; private set; } = new();

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private static Timer? _saveTimer;

    public static void Load()
    {
        if (!File.Exists(_settingsPath))
        {
            Current = new Chip8Settings();
            return;
        }

        try
        {
            var json = File.ReadAllText(_settingsPath);

            if (string.IsNullOrWhiteSpace(json))
            {
                Current = new Chip8Settings();
                return;
            }

            var loaded = JsonSerializer.Deserialize<Chip8Settings>(json, _jsonOptions);

            if (loaded is not null)
            {
                Current = loaded;
            }
            else
            {
                Current = new Chip8Settings(); // fallback if deserialization returns null
            }
        }
        catch (Exception ex)
        {
            // Optionally: log the error somewhere
            Console.Error.WriteLine($"⚠️ Failed to load settings: {ex.Message}");
            Current = new Chip8Settings(); // always fall back
        }
    }

    public static void Save()
    {
        string? directory = Path.GetDirectoryName(_settingsPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory); // Creates it if needed; no-op if it exists
        }

        var json = JsonSerializer.Serialize(Current, _jsonOptions);
        File.WriteAllText(_settingsPath, json);
    }

    public static void SaveOnChangeDebounced(TimeSpan? delay = null)
    {
        delay ??= TimeSpan.FromSeconds(1);
        _saveTimer?.Dispose();
        _saveTimer = new Timer(_ => Save(), null, delay.Value, Timeout.InfiniteTimeSpan);
    }

    public static void Register(ISettingsChangedNotifier notifier)
    {
        notifier.SettingsChanged += () => SaveOnChangeDebounced();
    }
}