// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace QFXViewer;

public class UserSettings : SettingsManager<UserSettings>, INotifyPropertyChanged
{
    #region Methods
    public void SaveWindowPos()
    {
        Window mainWindow = Application.Current.MainWindow;
        WindowHeight = Math.Floor(mainWindow.Height);
        WindowLeft = Math.Floor(mainWindow.Left);
        WindowTop = Math.Floor(mainWindow.Top);
        WindowWidth = Math.Floor(mainWindow.Width);
    }

    public void SetWindowPos()
    {
        Window mainWindow = Application.Current.MainWindow;
        mainWindow.Height = WindowHeight;
        mainWindow.Left = WindowLeft;
        mainWindow.Top = WindowTop;
        mainWindow.Width = WindowWidth;
    }
    #endregion Methods

    #region Properties
    public int DarkMode
    {
        get => _darkmode;
        set
        {
            _darkmode = value;
            OnPropertyChanged();
        }
    }

    public bool IncludeDebug
    {
        get => _includeDebug;
        set
        {
            _includeDebug = value;
            OnPropertyChanged();
        }
    }

    public bool KeepOnTop
    {
        get => _keepOnTop;
        set
        {
            _keepOnTop = value;
            OnPropertyChanged();
        }
    }

    public int PrimaryColor
    {
        get => _primaryColor;
        set
        {
            _primaryColor = value;
            OnPropertyChanged();
        }
    }

    public bool ShowMemoCol
    {
        get => _showMemoCol;
        set
        {
            _showMemoCol = value;
            OnPropertyChanged();
        }
    }

    public bool ShowRefCol
    {
        get => _showRefCol;
        set
        {
            _showRefCol = value;
            OnPropertyChanged();
        }
    }

    public bool ShowTransCol
    {
        get => _showTransCol;
        set
        {
            _showTransCol = value;
            OnPropertyChanged();
        }
    }

    public bool ShowTypeCol
    {
        get => _showTypeCol;
        set
        {
            _showTypeCol = value;
            OnPropertyChanged();
        }
    }

    public int UISize
    {
        get => _uiSize;
        set
        {
            _uiSize = value;
            OnPropertyChanged();
        }
    }

    public double WindowHeight
    {
        get
        {
            if (_windowHeight < 100)
            {
                _windowHeight = 100;
            }
            return _windowHeight;
        }
        set => _windowHeight = value;
    }

    public double WindowLeft
    {
        get
        {
            if (_windowLeft < 0)
            {
                _windowLeft = 100;
            }
            return _windowLeft;
        }
        set => _windowLeft = value;
    }

    public double WindowTop
    {
        get
        {
            if (_windowTop < 0)
            {
                _windowTop = 100;
            }
            return _windowTop;
        }
        set => _windowTop = value;
    }

    public double WindowWidth
    {
        get
        {
            if (_windowWidth < 100)
            {
                _windowWidth = 100;
            }
            return _windowWidth;
        }
        set => _windowWidth = value;
    }
    #endregion Properties

    #region Private backing fields
    private int _darkmode = (int)ThemeType.System;
    private bool _includeDebug;
    private bool _keepOnTop;
    private int _primaryColor = (int)AccentColor.Blue;
    private bool _showMemoCol = true;
    private bool _showRefCol = true;
    private bool _showTransCol = true;
    private bool _showTypeCol = true;
    private int _uiSize = (int)MySize.Default;
    private double _windowHeight = 500;
    private double _windowLeft = 300;
    private double _windowTop = 300;
    private double _windowWidth = 800;
    #endregion Private backing fields

    #region Handle property change event
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Handle property change event
}
