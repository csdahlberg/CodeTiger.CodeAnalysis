﻿using System;

namespace UnitTests.CodeTiger.CodeAnalysis.Analyzers;

/// <summary>
/// Location where the diagnostic appears, as determined by path, line number, and column number.
/// </summary>
public readonly struct DiagnosticResultLocation
{
    public string Path { get; }

    public int Line { get; }

    public int Column { get; }

    public DiagnosticResultLocation(string path, int line, int column)
    {
        if (line < -1)
        {
            throw new ArgumentOutOfRangeException(nameof(line), "line must be >= -1");
        }

        if (column < -1)
        {
            throw new ArgumentOutOfRangeException(nameof(column), "column must be >= -1");
        }

        Path = path;
        Line = line;
        Column = column;
    }
}
