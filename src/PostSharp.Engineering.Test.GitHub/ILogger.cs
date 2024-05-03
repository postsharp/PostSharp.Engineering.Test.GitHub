// Copyright (c) SharpCrafters s.r.o. All rights reserved. Released under the MIT license.

using System.Reflection.Metadata;

public interface ILogger
{
    LogWriter Error { get; }

    LogWriter Warning { get; }

    LogWriter Trace { get; }
}

public class ConsoleLogger : ILogger
{
    public LogWriter Error { get; } = new LogWriter();

    public LogWriter Warning { get; } = new LogWriter();

    public LogWriter Trace { get; } = new LogWriter();
}

public class LogWriter
{
    internal void Log( string v )
    {
        Console.WriteLine( v );
    }
}