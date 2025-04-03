using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace AteChips.Video.ImGui;
public class ImGuiFileBrowser
{
    public string CurrentDirectory { get; private set; } = Directory.GetCurrentDirectory();
    public string? SelectedFile { get; private set; }
    private int _selectedIndex = -1;

    private readonly List<string> _entries = [];

    private readonly List<(string Root, string Label)> _cachedDrives = [];
    private double _lastDriveScanTime;
    private readonly TimeSpan _driveCacheDuration = TimeSpan.FromSeconds(120);

    public bool IsOpen { get; set; }

    public void Open(string? startDirectory = null)
    {
        CurrentDirectory = startDirectory ?? Directory.GetCurrentDirectory();
        RefreshEntries();
        IsOpen = true;
        SelectedFile = null;
    }

    public void Reset()
    {
        IsOpen = false;
        SelectedFile = null;
    }

    private void RefreshDrives()
    {
        _cachedDrives.Clear();

        try
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (!drive.IsReady) { continue; }

                string root = drive.RootDirectory.FullName;
                string label = drive.VolumeLabel;
                string type = drive.DriveType.ToString();

                string formatted = string.IsNullOrEmpty(label)
                    ? $"{root} [{type}]"
                    : $"{root} ({label}) [{type}]";

                _cachedDrives.Add((root, formatted));
            }

            _lastDriveScanTime = DateTime.Now.TimeOfDay.TotalSeconds;
        }
        catch { /* ignored */ }
    }

    private void RefreshEntries()
    {
        _entries.Clear();

        try
        {
            _entries.Add("..");

            IOrderedEnumerable<string> dirs = Directory.GetDirectories(CurrentDirectory).OrderBy(d => d);
            _entries.AddRange(dirs.Select(Path.GetFileName)!);

            IOrderedEnumerable<string> files = Directory.GetFiles(CurrentDirectory)
                .OrderByDescending(f => Path.GetExtension(f) == ".ch8")
                .ThenBy(f => f);

            _entries.AddRange(files.Select(Path.GetFileName)!);
        }
        catch (Exception ex)
        {
            _entries.Clear();
            _entries.Add($"<Error: {ex.Message}>");
        }
    }

    public void Render(string title = "Open ROM")
    {
        if (!IsOpen) { return; }

        bool isOpen = IsOpen;

        double now = DateTime.Now.TimeOfDay.TotalSeconds;
        if (_cachedDrives.Count == 0 || now - _lastDriveScanTime > _driveCacheDuration.TotalSeconds)
        {
            RefreshDrives();
        }


        if (ImGuiNET.ImGui.Begin(title, ref isOpen, ImGuiWindowFlags.NoCollapse))
        {

            if (ImGuiNET.ImGui.Button("Desktop"))
            {
                CurrentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                RefreshEntries();
            }
            ImGuiNET.ImGui.SameLine();

            if (ImGuiNET.ImGui.Button("Downloads"))
            {
                string downloads = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "Downloads"
                );
                if (Directory.Exists(downloads))
                {
                    CurrentDirectory = downloads;
                    RefreshEntries();
                }
            }
            ImGuiNET.ImGui.SameLine();

            if (ImGuiNET.ImGui.Button("App Folder"))
            {
                CurrentDirectory = AppContext.BaseDirectory;
                RefreshEntries();
            }

            ImGuiNET.ImGui.SameLine();

            ImGuiNET.ImGui.SetNextItemWidth(ImGuiNET.ImGui.GetContentRegionAvail().X);
            if (ImGuiNET.ImGui.BeginCombo("##Drives", CurrentDirectory[..3])) // show root (e.g. "C:\")
            {
                foreach ((string root, string label) in _cachedDrives)
                {
                    if (ImGuiNET.ImGui.Selectable(label))
                    {
                        CurrentDirectory = root;
                        RefreshEntries();
                    }
                }
                ImGuiNET.ImGui.EndCombo();
            }


            ImGuiNET.ImGui.Text($"Current directory: {CurrentDirectory}");
            ImGuiNET.ImGui.Separator();

            if (ImGuiNET.ImGui.BeginChild("FileList", new Vector2(-1, -23), ImGuiChildFlags.Borders,
                    ImGuiWindowFlags.AlwaysVerticalScrollbar))
            {
                for (int i = 0; i < _entries.Count; i++)
                {
                    bool selected = (i == _selectedIndex);
                    if (ImGuiNET.ImGui.Selectable(_entries[i], selected))
                    {
                        _selectedIndex = i;
                    }

                    // Double-click to activate
                    if (ImGuiNET.ImGui.IsItemHovered() && ImGuiNET.ImGui.IsMouseDoubleClicked(0))
                    {
                        ActivateEntry(_entries[i]);
                    }
                }
            }
            ImGuiNET.ImGui.EndChild();

            if (_selectedIndex >= 0 && _selectedIndex < _entries.Count)
            {
                string selectedName = _entries[_selectedIndex];
                string fullPath = Path.Combine(CurrentDirectory, selectedName);
                if (File.Exists(fullPath))
                {
                    if (ImGuiNET.ImGui.Button("Open"))
                    {
                        SelectedFile = fullPath;
                        isOpen = false;
                    }
                    ImGuiNET.ImGui.SameLine();
                }
            }

            if (ImGuiNET.ImGui.Button("Cancel"))
            {
                isOpen = false;
                SelectedFile = null;
            }
        }
        ImGuiNET.ImGui.End();
        IsOpen = isOpen;
    }

    private void ActivateEntry(string entry)
    {
        string path = Path.Combine(CurrentDirectory, entry);
        if (Directory.Exists(path))
        {
            CurrentDirectory = Path.GetFullPath(path);
            RefreshEntries();
            _selectedIndex = -1;
        }
        else if (File.Exists(path))
        {
            SelectedFile = path;
            IsOpen = false;
        }
    }
}