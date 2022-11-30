// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace QFXViewer;

internal static class StringHelpers
{
    /// <summary>
    /// Formats the account number, adding spaces between number groups to improve readability.
    /// </summary>
    /// <param name="acct">Account number.</param>
    /// <returns>Formatted account number.</returns>
    internal static string FormatAcctNumber(string acct)
    {
        switch (acct.Length)
        {
            case 16:
                return acct.Insert(12, " ")
                                .Insert(8, " ")
                                .Insert(4, " ");
            case 15:
                return acct.Insert(11, " ")
                                .Insert(7, " ")
                                .Insert(3, " ");
            case 12:
                return acct.Insert(8, " ")
                                .Insert(4, " ");
            case 10:
                return acct.Insert(6, " ")
                                .Insert(2, " ");
            case 9:
                return acct.Insert(5, " ")
                                .Insert(2, " ");
        }
        return acct;
    }
}