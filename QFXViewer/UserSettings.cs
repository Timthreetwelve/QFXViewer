﻿// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

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
        get => darkmode;
        set
        {
            darkmode = value;
            OnPropertyChanged();
        }
    }

    public bool IncludeDebug
    {
        get => includeDebug;
        set
        {
            includeDebug = value;
            OnPropertyChanged();
        }
    }

    public bool KeepOnTop
    {
        get => keepOnTop;
        set
        {
            keepOnTop = value;
            OnPropertyChanged();
        }
    }

    public int PrimaryColor
    {
        get => primaryColor;
        set
        {
            primaryColor = value;
            OnPropertyChanged();
        }
    }

    public bool ShowMemoCol
    {
        get => showMemoCol;
        set
        {
            showMemoCol = value;
            OnPropertyChanged();
        }
    }

    public bool ShowRefCol
    {
        get => showRefCol;
        set
        {
            showRefCol = value;
            OnPropertyChanged();
        }
    }

    public bool ShowTransCol
    {
        get => showTransCol;
        set
        {
            showTransCol = value;
            OnPropertyChanged();
        }
    }

    public bool ShowTypeCol
    {
        get => showTypeCol;
        set
        {
            showTypeCol = value;
            OnPropertyChanged();
        }
    }

    public int UISize
    {
        get => uiSize;
        set
        {
            uiSize = value;
            OnPropertyChanged();
        }
    }

    public double WindowHeight
    {
        get
        {
            if (windowHeight < 100)
            {
                windowHeight = 100;
            }
            return windowHeight;
        }
        set => windowHeight = value;
    }

    public double WindowLeft
    {
        get
        {
            if (windowLeft < 0)
            {
                windowLeft = 100;
            }
            return windowLeft;
        }
        set => windowLeft = value;
    }

    public double WindowTop
    {
        get
        {
            if (windowTop < 0)
            {
                windowTop = 100;
            }
            return windowTop;
        }
        set => windowTop = value;
    }

    public double WindowWidth
    {
        get
        {
            if (windowWidth < 100)
            {
                windowWidth = 100;
            }
            return windowWidth;
        }
        set => windowWidth = value;
    }
    #endregion Properties

    #region Private backing fields
    private int darkmode = (int)ThemeType.System;
    private bool includeDebug;
    private bool keepOnTop;
    private int primaryColor = (int)AccentColor.Blue;
    private bool showMemoCol = true;
    private bool showRefCol = true;
    private bool showTransCol = true;
    private bool showTypeCol = true;
    private int uiSize = (int)MySize.Default;
    private double windowHeight = 500;
    private double windowLeft = 300;
    private double windowTop = 300;
    private double windowWidth = 800;
    #endregion Private backing fields

    #region Handle property change event
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Handle property change event
}
