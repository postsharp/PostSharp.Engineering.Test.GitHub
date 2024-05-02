// Copyright (c) SharpCrafters s.r.o. All rights reserved. Released under the MIT license.

using Metalama.Backstage.Diagnostics;

namespace My.Product;

internal class ConsoleLoggerFactory : ILoggerFactory
{
    public void Dispose()
    {
    }

    public void Flush()
    {
    }

    public ILogger GetLogger( string category ) => new ConsoleLogger( category );
}

internal class ConsoleLogger(string category, string prefix = "") : ILogger
{
    private class ConsoleLogWriter : ILogWriter
    {
        private readonly string _category;
        private readonly string _prefix;
        private readonly string _level;

        public ConsoleLogWriter( string category, string prefix, string level )
        {
            this._category = category;
            this._prefix = prefix;
            this._level = level;
        }

        public void Log( string message )
        {
            Console.WriteLine( $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {this._level} {this._category} {this._prefix}{message}" );
        }
    }

    public ILogWriter? Trace { get; } = new ConsoleLogWriter( category, prefix, "TRACE" );

    public ILogWriter? Info { get; } = new ConsoleLogWriter( category, prefix, "INFO" );

    public ILogWriter? Warning { get; } = new ConsoleLogWriter( category, prefix, "WARNING" );

    public ILogWriter? Error { get; } = new ConsoleLogWriter( category, prefix, "ERROR" );

    public ILogger WithPrefix( string additionalPrefix )
    {
        return new ConsoleLogger( prefix + additionalPrefix );
    }
}
