using System;

namespace AteChips.Core.Shared.Interfaces;

public interface ISettingsChangedNotifier 
{
    event Action? SettingsChanged;
}