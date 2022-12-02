// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace QFXViewer;

/// <summary>
/// Class used to provide additional properties and methods.
/// </summary>
/// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
internal class FinInfo : INotifyPropertyChanged
{
    private static IEnumerable<string> qfxLines;
    private static readonly Logger log = LogManager.GetLogger("logTemp");

    public static FinInfo Info { get; } = new FinInfo();

    #region Properties
    public string AcctType
    {
        get
        {
            return acctType ?? "n/a";
        }
        set
        {
            acctType = value;
            OnPropertyChanged();
        }
    }

    public string AcctNum
    {
        get
        {
            return acctNum ?? "n/a";
        }
        set
        {
            acctNum = value;
            OnPropertyChanged();
        }
    }

    public string OrgName
    {
        get
        {
            return orgName ?? "n/a";
        }
        set
        {
            orgName = value;
            OnPropertyChanged();
        }
    }

    public decimal Balance
    {
        get => balance;
        set
        {
            balance = value;
            OnPropertyChanged();
        }
    }

    public DateTime BalanceAsOf
    {
        get => balanceAsOf;
        set
        {
            balanceAsOf = value;
            OnPropertyChanged();
        }
    }

    public string QFXFileName
    {
        get => qfxFileName ?? "n/a";
        set
        {
            qfxFileName = value;
            OnPropertyChanged();
        }
    }
    #endregion Properties

    #region Private backing fields
    private string acctNum;
    private string acctType;
    private decimal balance;
    private DateTime balanceAsOf;
    private string orgName;
    private string qfxFileName;
    #endregion Private backing fields

    #region Property change event
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
            log.Error($"File not found: {qfxFile}");
            return false;
        }

        qfxLines = File.ReadLines(qfxFile);

        if (!qfxLines.Any(x => x.Contains("<OFX>")))
        {
            log.Error($"{qfxFile} <OFX> tag not found.");
            return false;
        }

        if (qfxLines.Any(x => x.Contains("<STMTTRNRS>")))
        {
            stmtFound = true;
        }

        if (qfxLines.Any(x => x.Contains("<CCSTMTTRNRS>")))
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

        foreach (string line in qfxLines)
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
                int pos = (int)line.IndexOf("<ORG>");
                Info.OrgName = line[(pos + 5)..];
            }
            else if (line.Contains("<ACCTID>"))
            {
                int pos = (int)line.IndexOf("<ACCTID>");
                string temp = line[(pos + 8)..];
                Info.AcctNum = StringHelpers.FormatAcctNumber(temp);
            }
        }
    }
    #endregion Methods
}
