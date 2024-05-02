using My.Product;
using System.Collections;

new TargetClass().TargetMethod();

var isUnattended = ProcessUtilities.IsCurrentProcessUnattended( new ConsoleLoggerFactory() );

Console.WriteLine();
Console.WriteLine( "--------------------------------------------" );
Console.WriteLine();
Console.WriteLine( $"Is unattended: {isUnattended}" );
Console.WriteLine();
Console.WriteLine( "--------------------------------------------" );
Console.WriteLine();

foreach ( DictionaryEntry ev in Environment.GetEnvironmentVariables())
{
    Console.WriteLine( $"{ev.Key}: {ev.Value}" );
}