// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace QFXViewer;

/// <summary>
/// Class used to provide additional properties and methods.
/// </summary>
internal class FinInfo : INotifyPropertyChanged
{
    #region Private fields
    private static IEnumerable<string>? _qfxLines;
    private static readonly Logger _log = LogManager.GetLogger("logTemp");
    #endregion Private fields

    public static FinInfo Info { get; } = new FinInfo();

    #region Properties
    public string AcctType
    {
        get
        {
            return _acctType ?? "n/a";
        }
        set
        {
            _acctType = value;
            OnPropertyChanged();
        }
    }

    public string AcctNum
    {
        get
        {
            return _acctNum ?? "n/a";
        }
        set
        {
            _acctNum = value;
            OnPropertyChanged();
        }
    }

    public string OrgName
    {
        get
        {
            return _orgName ?? "n/a";
        }
        set
        {
            _orgName = value;
            OnPropertyChanged();
        }
    }

    public decimal Balance
    {
        get => _balance;
        set
        {
            _balance = value;
            OnPropertyChanged();
        }
    }

    public DateTime BalanceAsOf
    {
        get => _balanceAsOf;
        set
        {
            _balanceAsOf = value;
            OnPropertyChanged();
        }
    }

    public string QFXFileName
    {
        get => _qfxFileName ?? "n/a";
        set
        {
            _qfxFileName = value;
            OnPropertyChanged();
        }
    }
    #endregion Properties

    #region Private backing fields
    private string? _acctNum;
    private string? _acctType;
    private decimal _balance;
    private DateTime _balanceAsOf;
    private string? _orgName;
    private string? _qfxFileName;

    #endregion Private backing fields

    #region Property change event
    /// <summary>Occurs when a property value changes.</summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Property change event

    #region Methods
    /// <summary>
    /// Checks the QFX file.
    /// </summary>
    /// <param name="qfxFile">The QFX file.</param>
    /// <returns>True if file is found and has an OFX tag and either a STMTTRNRS tag or a CCSTMTTRNRS tag.</returns>
    public static bool CheckQfxFile(string qfxFile)
    {
        bool stmtFound = false;

        if (!File.Exists(qfxFile))
        {
            _log.Error($"File not found: {qfxFile}");
            return false;
        }

        _qfxLines = File.ReadLines(qfxFile);

        if (!_qfxLines.Any(x => x.Contains("<OFX>")))
        {
            _log.Error($"{qfxFile} <OFX> tag not found.");
            return false;
        }

        if (_qfxLines.Any(x => x.Contains("<STMTTRNRS>")))
        {
            stmtFound = true;
        }

        if (_qfxLines.Any(x => x.Contains("<CCSTMTTRNRS>")))
        {
            stmtFound = true;
        }
        return stmtFound;
    }

    /// <summary>
    /// Gets addition information from the QFX file that QFXParser doesn't provide.
    /// </summary>
    public static void GetFinInfo()
    {
        TextInfo textInfo = new CultureInfo("en-US").TextInfo;

        if (_qfxLines != null)
        {
            foreach (string line in _qfxLines)
            {
                if (line.Contains("<ACCTTYPE>"))
                {
                    int pos = line.IndexOf("<ACCTTYPE>");
                    Info.AcctType = textInfo.ToTitleCase(line[(pos + 10)..].ToLower());
                }
                else if (line.Contains("<CCSTMTTRNRS>"))
                {
                    Info.AcctType = "Credit Card";
                }
                else if (line.Contains("<ORG>"))
                {
                    int pos = line.IndexOf("<ORG>");
                    Info.OrgName = line[(pos + 5)..];
                }
                else if (line.Contains("<ACCTID>"))
                {
                    int pos = line.IndexOf("<ACCTID>");
                    string temp = line[(pos + 8)..];
                    Info.AcctNum = StringHelpers.FormatAcctNumber(temp);
                }
            }
        }
    }

    /// <summary>
    /// Gets the bank message
    /// </summary>
    /// <returns>
    /// Text of the message if one is found. Otherwise returns string.empty.
    /// </returns>
    public static string GetBankMsg()
    {
        if (_qfxLines != null)
        {
            string qfxLines = string.Concat(_qfxLines);

            int from = qfxLines.IndexOf("<BANKMSGSRSV1>");
            if (from == -1)
            {
                return string.Empty;
            }
            else
            {
                int to = qfxLines.IndexOf("</BANKMSGSRSV1>");
                if (to == -1)
                {
                    return string.Empty;
                }
                else
                {
                    from += "<BANKMSGSRSV1>".Length;
                    string msg = qfxLines[from..to];
                    if (!msg.StartsWith('<'))
                    {
                        return msg;
                    }
                }
            }
        }
        return string.Empty;
    }
    #endregion Methods
}
