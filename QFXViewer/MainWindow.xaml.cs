// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

using FileHashes;

namespace QFXViewer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private string qfxFileName;

    #region Stopwatch
    private readonly Stopwatch stopwatch = new();
    #endregion Stopwatch

    #region NLog Instance
    private static readonly Logger log = LogManager.GetLogger("logTemp");
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
        stopwatch.Start();

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
        log.Info($"{AppInfo.AppName} ({AppInfo.AppProduct}) {AppInfo.AppVersion} is starting up");
        log.Info($"{AppInfo.AppName} {AppInfo.AppCopyright}");
        log.Debug($"{AppInfo.AppName} Build date: {BuildInfo.BuildDateUtc.ToUniversalTime():f} (UTC)");
        log.Debug($"{AppInfo.AppName} Commit ID: {BuildInfo.CommitIDString} ");

        // Log the .NET version, app framework and OS platform
        string version = Environment.Version.ToString();
        log.Debug($".NET version: {AppInfo.RuntimeVersion.Replace(".NET", "")}  ({version})");
        log.Debug(AppInfo.Framework);
        log.Debug(AppInfo.OsPlatform);

        // Window position
        UserSettings.Setting.SetWindowPos();
        Topmost = UserSettings.Setting.KeepOnTop;

        //
        MainWindowUIHelpers.SetBaseTheme((ThemeType)UserSettings.Setting.DarkMode);

        //
        MainWindowUIHelpers.SetPrimaryColor((AccentColor)UserSettings.Setting.PrimaryColor);

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
        log.Debug($"Setting change: {e.PropertyName} New Value: {newValue}");
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
        stopwatch.Stop();
        log.Info($"{AppInfo.AppName} is shutting down.  Elapsed time: {stopwatch.Elapsed:h\\:mm\\:ss\\.ff}");

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
        log.Error("Unhandled Exception");
        Exception e = (Exception)args.ExceptionObject;
        log.Error(e.Message);
        if (e.InnerException != null)
        {
            log.Error(e.InnerException.ToString());
        }
        log.Error(e.StackTrace);

        _ = MessageBox.Show("An error has occurred. See the log file",
            "ERROR",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }
    #endregion Unhandled Exception Handler

    #region Command line
    private void ProcessCommandLine()
    {
        string[] clArgs = Environment.GetCommandLineArgs();

        if (clArgs?.Length > 1 && File.Exists(clArgs[1]))
        {
            log.Debug($"File was found: {clArgs[1]}");
            FileInfo file = new(clArgs[1]);
            if (string.Equals(file.Extension, ".qfx", StringComparison.OrdinalIgnoreCase))
            {
                log.Debug($"File has correct extension: {file.Extension}");
                qfxFileName= file.FullName;
                ProcessQfxFile(file.FullName);
            }
        }
    }
    #endregion Command line

    private void ProcessQfxFile(string qfxFile)
    {
        if (FinInfo.CheckQfxFile(qfxFile))
        {
            FinInfo.GetFinInfo();
            FileParser parser = new(qfxFile);
            Statement x = parser.BuildStatement();
            FinInfo.Info.Balance = x.LedgerBalance.Amount;
            FinInfo.Info.BalanceAsOf = x.LedgerBalance.AsOf;
            TheDataGrid.ItemsSource = x.Transactions;
            tbFileName.Text = qfxFile;
        }
        else
        {
            FinInfo.Info.AcctNum = null;
            FinInfo.Info.AcctType = null;
            FinInfo.Info.Balance = 0;
            FinInfo.Info.BalanceAsOf = default;
            TheDataGrid.ItemsSource = null;
            tbFileName.Text = string.Empty;
            _ = MessageBox.Show(
            "QFX file was not found, is invalid or empty.",
            "Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        }
    }

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
            qfxFileName= dlgOpen.FileName;
        }
    }

    private void MnuSettings_Click(object sender, RoutedEventArgs e)
    {
        DialogHelpers.ShowSettingsDialog();
    }

    private void BtnViewQfx_Click(object sender, RoutedEventArgs e)
    {
        using Process p = new();
        p.StartInfo.FileName = "notepad.exe";
        p.StartInfo.Arguments = qfxFileName;
        p.StartInfo.UseShellExecute = true;
        p.StartInfo.ErrorDialog = false;
        _ = p.Start();
        log.Debug($"Opening {qfxFileName} in Notepad.exe");
    }

    private void MnuAbout_Click(object sender, RoutedEventArgs e)
    {
        DialogHelpers.ShowAboutDialog();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        // CTRL key combos
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

        // Ctrl and Shift
        if (e.KeyboardDevice.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
        {
            if (e.Key == Key.N)
            {
                if (UserSettings.Setting.PrimaryColor == 0)
                {
                    UserSettings.Setting.PrimaryColor = 18;
                }
                else
                {
                    UserSettings.Setting.PrimaryColor--;
                }
                if (e.Key == Key.R)
                {
                    string readme = Path.Combine(AppInfo.AppDirectory, "ReadMe.txt");
                    TextFileViewer.ViewTextFile(readme);
                }
            }

            // No CTRL or Shift key
            if (e.Key == Key.F1)
            {

            }
        }
    }

    private void MnuReadMe_Click(object sender, RoutedEventArgs e)
    {
        string readme = Path.Combine(AppInfo.AppDirectory, "ReadMe.txt");
        TextFileViewer.ViewTextFile(readme);
    }
}
