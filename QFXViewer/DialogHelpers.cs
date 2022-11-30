// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace FileHashes;

internal static class DialogHelpers
{
    /// <summary>
    /// Shows the About dialog.
    /// </summary>
    internal static async void ShowAboutDialog()
    {
        About about = new();
        _ = await DialogHost.Show(about, "MainDialogHost");
    }

    /// <summary>
    /// Shows the Settings dialog
    /// </summary>
    internal static async void ShowSettingsDialog()
    {
        Settings settings = new();
        _ = await DialogHost.Show(settings, "MainDialogHost");
    }
}
