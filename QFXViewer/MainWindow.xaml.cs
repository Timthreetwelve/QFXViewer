// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace QFXViewer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    #region Stopwatch
    private readonly Stopwatch _stopwatch = new();
    #endregion Stopwatch

    #region NLog Instance
    private static readonly Logger _log = LogManager.GetLogger("logTemp");
    #endregion NLog Instance

    public MainWindow()
    {
        InitializeSettings();

        InitializeComponent();

        ReadSettings();

        ProcessCommandLine();
    }

    #region Settings
    /// <summary>
    /// Read and apply settings
    /// </summary>
    private void InitializeSettings()
    {
        _stopwatch.Start();

        UserSettings.Init(UserSettings.AppFolder, UserSettings.DefaultFilename, true);
    }

    public void ReadSettings()
    {
        // Set NLog configuration
        NLHelpers.NLogConfig(false);

        // Unhandled exception handler
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        // Put version number in window title
        WindowTitleVersionAdmin();

        // Log the version, build date and commit id
        _log.Info($"{AppInfo.AppName} ({AppInfo.AppProduct}) {AppInfo.AppVersion} is starting up");
        _log.Info($"{AppInfo.AppName} {AppInfo.AppCopyright}");
        _log.Debug($"{AppInfo.AppName} Build date: {BuildInfo.BuildDateUtc.ToUniversalTime():f} (UTC)");
        _log.Debug($"{AppInfo.AppName} Commit ID: {BuildInfo.CommitIDString} ");

        // Log the .NET version, app framework and OS platform
        string version = Environment.Version.ToString();
        _log.Debug($".NET version: {AppInfo.RuntimeVersion.Replace(".NET", "")}  ({version})");
        _log.Debug(AppInfo.Framework);
        _log.Debug(AppInfo.OsPlatform);

        // Window position
        UserSettings.Setting.SetWindowPos();
        Topmost = UserSettings.Setting.KeepOnTop;

        // Set theme
        MainWindowUIHelpers.SetBaseTheme((ThemeType)UserSettings.Setting.DarkMode);

        // Set primary accent color
        MainWindowUIHelpers.SetPrimaryColor((AccentColor)UserSettings.Setting.PrimaryColor);

        // Set UI size
        MainWindowUIHelpers.UIScale((MySize)UserSettings.Setting.UISize);

        // Settings change event
        UserSettings.Setting.PropertyChanged += UserSettingChanged;
    }
    #endregion Settings

    #region Setting change
    /// <summary>
    /// My way of handling changes in UserSettings
    /// </summary>
    /// <param name="sender"></param>
    private void UserSettingChanged(object sender, PropertyChangedEventArgs e)
    {
        PropertyInfo prop = sender.GetType().GetProperty(e.PropertyName);
        object newValue = prop?.GetValue(sender, null);
        _log.Debug($"Setting change: {e.PropertyName} New Value: {newValue}");
        switch (e.PropertyName)
        {
            case nameof(UserSettings.Setting.KeepOnTop):
                Topmost = (bool)newValue;
                break;

            case nameof(UserSettings.Setting.IncludeDebug):
                NLHelpers.SetLogLevel((bool)newValue);
                break;

            case nameof(UserSettings.Setting.DarkMode):
                MainWindowUIHelpers.SetBaseTheme((ThemeType)newValue);
                break;

            case nameof(UserSettings.Setting.PrimaryColor):
                MainWindowUIHelpers.SetPrimaryColor((AccentColor)newValue);
                break;

            case nameof(UserSettings.Setting.UISize):
                int size = (int)newValue;
                double newSize = MainWindowUIHelpers.UIScale((MySize)size);
                MainGrid.LayoutTransform = new ScaleTransform(newSize, newSize);
                break;
        }
    }
    #endregion Setting change

    #region Window Events
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        // Stop the stopwatch and record elapsed time
        _stopwatch.Stop();
        _log.Info($"{AppInfo.AppName} is shutting down.  Elapsed time: {_stopwatch.Elapsed:h\\:mm\\:ss\\.ff}");

        // Shut down NLog
        LogManager.Shutdown();

        // Save settings
        UserSettings.Setting.SaveWindowPos();
        UserSettings.SaveSettings();
    }
    #endregion Window Events

    #region Window Title
    /// <summary>
    /// Puts the version number in the title bar as well as Administrator if running elevated
    /// </summary>
    public void WindowTitleVersionAdmin()
    {
        // Set the windows title
        if (IsAdministrator())
        {
            Title = AppInfo.AppName + " - " + AppInfo.TitleVersion + " - (Administrator)";
        }
        else
        {
            Title = AppInfo.AppName + " - " + AppInfo.TitleVersion;
        }
    }
    #endregion Window Title

    #region Running as Administrator?
    /// <summary>
    /// Determines if running as administrator (elevated)
    /// </summary>
    /// <returns>True if running elevated</returns>
    public static bool IsAdministrator()
    {
        return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
    }
    #endregion Running as Administrator?

    #region Unhandled Exception Handler
    /// <summary>
    /// Handles any exceptions that weren't caught by a try-catch statement
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
    {
        _log.Error("Unhandled Exception");
        Exception e = (Exception)args.ExceptionObject;
        _log.Error(e.Message);
        if (e.InnerException != null)
        {
            _log.Error(e.InnerException.ToString());
        }
        _log.Error(e.StackTrace);

        _ = MessageBox.Show("An error has occurred. See the log file",
            "ERROR",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }
    #endregion Unhandled Exception Handler

    #region Command line
    /// <summary>
    /// Processes the command line.
    /// </summary>
    private void ProcessCommandLine()
    {
        string[] clArgs = Environment.GetCommandLineArgs();

        if (clArgs?.Length > 1 && File.Exists(clArgs[1]))
        {
            _log.Debug($"File was found: {clArgs[1]}");
            FileInfo file = new(clArgs[1]);
            if (string.Equals(file.Extension, ".qfx", StringComparison.OrdinalIgnoreCase))
            {
                _log.Info($"File {file.FullName} was specified on the command line.");
                FinInfo.Info.QFXFileName = file.FullName;
                ProcessQfxFile(file.FullName);
            }
        }
        if (clArgs?.Length > 1 && !File.Exists(clArgs[1]))
        {
            _log.Warn($"\"{clArgs[1]}\" was specified on the command line but a file with that name couold not be found.");
        }
    }
    #endregion Command line

    #region Process the QFX file
    /// <summary>
    /// Checks, processes and displays the QFX file.
    /// </summary>
    /// <param name="qfxFile">The QFX file.</param>
    private void ProcessQfxFile(string qfxFile)
    {
        if (FinInfo.CheckQfxFile(qfxFile))
        {
            try
            {
                FinInfo.GetFinInfo();
                FileParser parser = new(qfxFile);
                Statement stmt = parser.BuildStatement();
                FinInfo.Info.Balance = stmt.LedgerBalance.Amount;
                FinInfo.Info.BalanceAsOf = stmt.LedgerBalance.AsOf;
                TheDataGrid.ItemsSource = stmt.Transactions;
                if (stmt.Transactions.Count == 1)
                {
                    _log.Info($"Found {stmt.Transactions.Count} transaction in {qfxFile}");
                }
                else
                {
                    _log.Info($"Found {stmt.Transactions.Count} transactions in {qfxFile}");
                }
            }
            catch (Exception ex)
            {
                ErrorEncountered(qfxFile, ex);
            }
        }
        else
        {
            ErrorEncountered(qfxFile, null);
        }
    }

    /// <summary>
    /// Handles errors encountered while processing the QFX file.
    /// </summary>
    /// <param name="filename">The QFX filename.</param>
    /// <param name="ex">The exception.</param>
    private void ErrorEncountered(string filename, Exception ex)
    {
        FinInfo.Info.AcctNum = null;
        FinInfo.Info.AcctType = null;
        FinInfo.Info.Balance = 0;
        FinInfo.Info.BalanceAsOf = default;
        TheDataGrid.ItemsSource = null;
        _log.Error(ex, $"Error reading {filename}");
        _ = MessageBox.Show("QFX file was not found, is invalid or empty.\nSee the log file for more info.",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
    }
    #endregion Process the QFX file

    #region Menu and Button events
    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void FileOpen_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog dlgOpen = new()
        {
            Title = "Enter File Name",
            CheckFileExists = true,
            CheckPathExists = true,
            Filter = "QFX files (*.qfx)|*.qfx"
        };
        if (dlgOpen.ShowDialog() == true)
        {
            ProcessQfxFile(dlgOpen.FileName);
            FinInfo.Info.QFXFileName = dlgOpen.FileName;
        }
    }

    private void MnuSettings_Click(object sender, RoutedEventArgs e)
    {
        if (!DialogHost.IsDialogOpen("MainDialogHost"))
        {
            DialogHelpers.ShowSettingsDialog();
        }
        else
        {
            DialogHost.Close("MainDialogHost");
            DialogHelpers.ShowSettingsDialog();
        }
    }

    private void BtnViewQfx_Click(object sender, RoutedEventArgs e)
    {
        using Process p = new();
        p.StartInfo.FileName = "notepad.exe";
        p.StartInfo.Arguments = FinInfo.Info.QFXFileName;
        p.StartInfo.UseShellExecute = true;
        p.StartInfo.ErrorDialog = false;
        _ = p.Start();
        _log.Debug($"Opening {FinInfo.Info.QFXFileName} in Notepad.exe");
    }

    private void MnuAbout_Click(object sender, RoutedEventArgs e)
    {
        if (!DialogHost.IsDialogOpen("MainDialogHost"))
        {
            DialogHelpers.ShowAboutDialog();
        }
        else
        {
            DialogHost.Close("MainDialogHost");
            DialogHelpers.ShowAboutDialog();
        }
    }

    private void MnuReadMe_Click(object sender, RoutedEventArgs e)
    {
        string readme = Path.Combine(AppInfo.AppDirectory, "ReadMe.txt");
        TextFileViewer.ViewTextFile(readme);
    }

    private void MnuLogFile_Click(object sender, RoutedEventArgs e)
    {
        string logFile = NLHelpers.GetLogfileName();
        TextFileViewer.ViewTextFile(logFile);
    }
    #endregion

    #region Keyboard Events
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        // CTRL key combos
        if (e.Key == Key.OemComma)
        {
            if (!DialogHost.IsDialogOpen("MainDialogHost"))
            {
                DialogHelpers.ShowSettingsDialog();
            }
            else
            {
                DialogHost.Close("MainDialogHost");
                DialogHelpers.ShowSettingsDialog();
            }
        }

        if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
        {
            if (e.Key == Key.M)
            {
                switch (UserSettings.Setting.DarkMode)
                {
                    case (int)ThemeType.Light:
                        UserSettings.Setting.DarkMode = (int)ThemeType.Dark;
                        break;
                    case (int)ThemeType.Dark:
                        UserSettings.Setting.DarkMode = (int)ThemeType.Darker;
                        break;
                    case (int)ThemeType.Darker:
                        UserSettings.Setting.DarkMode = (int)ThemeType.System;
                        break;
                    case (int)ThemeType.System:
                        UserSettings.Setting.DarkMode = (int)ThemeType.Light;
                        break;
                }
            }

            if (e.Key == Key.N)
            {
                if (UserSettings.Setting.PrimaryColor >= (int)AccentColor.BlueGray)
                {
                    UserSettings.Setting.PrimaryColor = 0;
                }
                else
                {
                    UserSettings.Setting.PrimaryColor++;
                }
            }
        }

        // No CTRL or Shift key
        if (e.Key == Key.F1)
        {
            if (!DialogHost.IsDialogOpen("MainDialogHost"))
            {
                DialogHelpers.ShowAboutDialog();
            }
            else
            {
                DialogHost.Close("MainDialogHost");
                DialogHelpers.ShowAboutDialog();
            }
        }

        if (e.Key == Key.Escape)
        {
            TheDataGrid.SelectedItem = null;
        }
    }
    #endregion

    #region Drag & Drop handlers
    /// <summary>
    /// Handles the PreviewDragOver event of the Window control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
    private void Window_PreviewDragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            List<string> dragfiles = ((DataObject)e.Data).GetFileDropList().Cast<string>().ToList();
            FileInfo fileInfo = new(dragfiles.FirstOrDefault());
            e.Effects = (dragfiles?.Count == 1 && fileInfo.Extension == ".qfx")
                ? DragDropEffects.Copy
                : DragDropEffects.None;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    /// <summary>
    /// Handles the PreviewDrop event of the Window control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
    private void Window_PreviewDrop(object sender, DragEventArgs e)
    {
        if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
        {
            string filename = ((DataObject)e.Data).GetFileDropList().Cast<string>().ToList().FirstOrDefault();
            _log.Debug($"File dropped: {filename}");
            FinInfo.Info.QFXFileName = filename;
            ProcessQfxFile(filename);
        }
    }
    #endregion Drag & Drop handlers
}
