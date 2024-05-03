//using System.ComponentModel;
//using System.Diagnostics;
//using System.Globalization;
//using System.Runtime.InteropServices;
//using System.Text;

//public sealed class ProcessInfo
//{
//    public int ProcessId { get; }

//    public string? ImagePath { get; }

//    public string? ProcessName
//        => (RuntimeInformation.IsOSPlatform( OSPlatform.Windows )
//                ? Path.GetFileNameWithoutExtension( this.ImagePath )
//                : Path.GetFileName( this.ImagePath ))
//            ?.ToLowerInvariant();

//    public ProcessInfo( int processId, string? imageFileName )
//    {
//        this.ProcessId = processId;
//        this.ImagePath = imageFileName;
//    }

//    public override string ToString() => $"{this.ProcessName}({this.ProcessId})";
//}

//internal abstract class ParentProcessSearchBase
//{
//    public abstract IReadOnlyList<ProcessInfo> GetParentProcesses( ISet<string>? pivots = null );

//    protected ILogger Logger { get; }

//    protected ParentProcessSearchBase( ILogger logger )
//    {
//        this.Logger = logger;
//    }
//}

//#pragma warning disable SA1649 // File name should match first type name
//internal abstract class ParentProcessSearchBase<TProcessHandle> : ParentProcessSearchBase
//#pragma warning restore SA1649
//{
//    protected ParentProcessSearchBase( ILogger logger ) : base( logger ) { }

//    public override IReadOnlyList<ProcessInfo> GetParentProcesses( ISet<string>? pivots = null )
//    {
//        var parents = new List<ProcessInfo>();
//        var parentIds = new HashSet<int>();
//        var currentProcessHandle = this.GetCurrentProcessHandle();
//        var isSelf = true;

//        while ( !this.IsNull( currentProcessHandle ) )
//        {
//            var (imageName, currentProcessId, parentProcessHandle) = this.GetProcessInfo( currentProcessHandle );

//            var processInfo = new ProcessInfo( currentProcessId, imageName );

//            if ( isSelf )
//            {
//                isSelf = false;
//            }
//            else
//            {
//                if ( !parentIds.Add( processInfo.ProcessId ) )
//                {
//                    // There is a loop.
//                    break;
//                }

//                if ( parents.Count > 64 )
//                {
//                    throw new InvalidOperationException(
//                        string.Format(
//                            CultureInfo.InvariantCulture,
//                            "Cannot have more than 64 parents. Parent processes: {0}.",
//                            string.Join( ", ", parents.Select( pi => pi.ProcessId.ToString( CultureInfo.InvariantCulture ) ).ToArray() ) ) );
//                }

//                parents.Add( processInfo );

//                if ( pivots != null && processInfo.ProcessName != null && pivots.Contains( processInfo.ProcessName ) )
//                {
//                    break;
//                }
//            }

//            this.CloseProcessHandle( currentProcessHandle );

//            currentProcessHandle = parentProcessHandle;
//        }

//        if ( !this.IsNull( currentProcessHandle ) )
//        {
//            this.CloseProcessHandle( currentProcessHandle );
//        }

//        return parents;
//    }

//    protected abstract bool IsNull( TProcessHandle handle );

//    protected abstract TProcessHandle GetCurrentProcessHandle();

//    protected abstract (string? ImageName, int CurrentProcessId, TProcessHandle ParentProcessHandle) GetProcessInfo( TProcessHandle processHandle );

//    protected abstract void CloseProcessHandle( TProcessHandle handle );
//}

//internal class ParentProcessSearchLinux : ParentProcessSearchBase<int>
//{
//    public ParentProcessSearchLinux( ILogger logger ) : base( logger ) { }

//    protected override bool IsNull( int handle ) => handle == 0;

//    protected override int GetCurrentProcessHandle() => Process.GetCurrentProcess().Id;

//    protected override (string? ImageName, int CurrentProcessId, int ParentProcessHandle) GetProcessInfo( int processHandle )
//    {
//        // There's no handle on Linux.
//        var processId = processHandle;
//        string? commandImageName;

//        try
//        {
//            commandImageName = File.ReadAllText( "/proc/" + processId + "/comm" ).Trim();
//        }
//        catch ( Exception e )
//        {
//            this.Logger.Error?.Log( $"Could not read '/proc/{processId}/comm' file: {e.Message}" );
//            commandImageName = null;
//        }

//        // Read status file of the process.
//        string? processStatus;

//        try
//        {
//            processStatus = File.ReadAllText( "/proc/" + processId + "/stat" );
//        }
//        catch ( Exception e )
//        {
//            this.Logger.Error?.Log( $"Could not read '/proc/{processId}/stat' file: {e.Message}" );

//            throw;
//        }

//        var processStatusArray = processStatus.Split( ' ' );
//        int parentProcessId;

//        // Try parse PPID from 4th value of status information, then add the process to list of processes.
//        try
//        {
//            parentProcessId = int.Parse( processStatusArray[3], CultureInfo.InvariantCulture );
//        }
//        catch ( Exception e )
//        {
//            this.Logger.Error?.Log( $"Could not parse PPID from process '{processId}' status file: {e.Message}" );

//            throw;
//        }

//        return (commandImageName, processId, parentProcessId);
//    }

//    protected override void CloseProcessHandle( int handle ) { }
//}

//internal class ParentProcessSearchMac : ParentProcessSearchBase<int>
//{
//    public ParentProcessSearchMac( ILogger logger ) : base( logger ) { }

//    protected override bool IsNull( int handle ) => handle == 0;

//    protected override int GetCurrentProcessHandle() => Process.GetCurrentProcess().Id;

//    protected override (string? ImageName, int CurrentProcessId, int ParentProcessHandle) GetProcessInfo( int processHandle )
//    {
//        // There's no handle on Mac.
//        var processId = processHandle;

//        try
//        {
//            var processStartInfo = new ProcessStartInfo
//            {
//                FileName = "ps",
//                Arguments = $"-o ppid= -o command= {processId}",
//                UseShellExecute = false,
//                RedirectStandardOutput = true,
//                RedirectStandardError = true,
//                CreateNoWindow = true
//            };

//            using ( var cmdProcess = Process.Start( processStartInfo ) )
//            {
//                if ( cmdProcess == null )
//                {
//                    throw new InvalidOperationException( "Failed to start 'ps' command." );
//                }

//                cmdProcess.WaitForExit();
//                var output = cmdProcess.StandardOutput.ReadToEnd().Trim();

//                this.Logger.Trace?.Log( $"ps {processId} output: {output}" );

//                var pidAndCommand = output.Split( new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries );

//                // The remaining fields are command arguments.
//                if ( pidAndCommand.Length < 2 )
//                {
//                    throw new InvalidOperationException( $"Unexpected output from 'ps' command: '{output}'." );
//                }

//                var parentProcessId = int.Parse( pidAndCommand[0], CultureInfo.InvariantCulture );

//                // Examples:
//                // -bash
//                // /init
//                // /usr/bin/dotnet
//                var imageName = pidAndCommand[1]
//                    .Split( new[] { '/' }, StringSplitOptions.RemoveEmptyEntries )
//                    .Last()
//                    .Trim();

//                return (imageName, processId, parentProcessId);
//            }
//        }
//        catch ( Exception ex )
//        {
//            Console.WriteLine( "Error reading parent process on macOS: " + ex.Message );

//            throw;
//        }
//    }

//    protected override void CloseProcessHandle( int handle ) { }
//}

//internal sealed class ParentProcessSearchWindows : ParentProcessSearchBase<IntPtr>
//{
//    public ParentProcessSearchWindows( ILogger logger ) : base( logger ) { }

//    [DllImport( "kernel32" )]
//    private static extern IntPtr GetCurrentProcess();

//    [StructLayout( LayoutKind.Sequential )]
//    private struct PROCESS_BASIC_INFORMATION
//    {
//        public IntPtr Reserved1;
//        public IntPtr PebBaseAddress;
//        public IntPtr Reserved2_0;
//        public IntPtr Reserved2_1;
//        public IntPtr UniqueProcessId;
//        public IntPtr InheritedFromUniqueProcessId;
//    }

//    [DllImport( "ntdll" )]
//    private static extern int NtQueryInformationProcess(
//        IntPtr processHandle,
//        int processInformationClass,
//        ref PROCESS_BASIC_INFORMATION processInformation,
//        int processInformationLength,
//        out int returnLength );

//    [DllImport( "kernel32" )]
//    private static extern IntPtr OpenProcess( uint dwDesiredAccess, bool bInheritHandle, int dwProcessId );

//    [DllImport( "kernel32" )]
//    private static extern bool CloseHandle( IntPtr hObject );

//    [DllImport( "psapi", CharSet = CharSet.Unicode )]
//    private static extern int GetProcessImageFileName(
//        IntPtr hProcess,
//        [Out] [MarshalAs( UnmanagedType.LPWStr )]
//        StringBuilder lpImageFileName,
//        int nSize );

//    private const uint PROCESS_QUERY_INFORMATION = 0x0400;

//    protected override bool IsNull( IntPtr handle ) => handle == IntPtr.Zero;

//    protected override IntPtr GetCurrentProcessHandle() => GetCurrentProcess();

//    protected override (string? ImageName, int CurrentProcessId, IntPtr ParentProcessHandle) GetProcessInfo( IntPtr hProcess )
//    {
//        string? imageName = null;
//        var stringBuilder = new StringBuilder( 1024 );

//        if ( GetProcessImageFileName( hProcess, stringBuilder, 1024 ) != 0 )
//        {
//            imageName = stringBuilder.ToString();
//        }

//        var pbi = default( PROCESS_BASIC_INFORMATION );
//        var status = NtQueryInformationProcess( hProcess, 0, ref pbi, Marshal.SizeOf( pbi ), out _ );

//        if ( status != 0 )
//        {
//            throw new Win32Exception( status );
//        }

//        int parentProcessId;
//        int processId;

//        try
//        {
//            parentProcessId = pbi.InheritedFromUniqueProcessId.ToInt32();
//        }
//        catch ( ArgumentException )
//        {
//            // not found
//            parentProcessId = 0;
//        }

//        try
//        {
//            processId = pbi.UniqueProcessId.ToInt32();
//        }
//        catch ( ArgumentException )
//        {
//            // not found
//            processId = 0;
//        }

//        var hParentProcess = parentProcessId != 0 ? OpenProcess( PROCESS_QUERY_INFORMATION, true, parentProcessId ) : IntPtr.Zero;

//        return (imageName, processId, hParentProcess);
//    }

//    protected override void CloseProcessHandle( IntPtr hProcess ) => CloseHandle( hProcess );
//}

//public static class ProcessUtilities
//{
//    private static int _isCurrentProcessUnattended;

//    public static bool IsCurrentProcessUnattended( ILoggerFactory loggerFactory )
//    {
//        var logger = loggerFactory.GetLogger( "ProcessUtilities" );

//        if ( _isCurrentProcessUnattended == 0 )
//        {
//            _isCurrentProcessUnattended = Detect() ? 1 : 2;
//        }

//        return _isCurrentProcessUnattended == 1;

//        bool Detect()
//        {
//            if ( !Environment.UserInteractive )
//            {
//                logger.Trace?.Log( "Unattended mode detected because Environment.UserInteractive = false." );

//                return true;
//            }

//            // Check the parent processes.
//            var unattendedProcesses = new HashSet<string>
//            {
//                "services",
//                "java",               // TeamCity, Atlassian Bamboo (can also be "bamboo"), Jenkins, GoCD
//                "bamboo",             // Atlassian Bamboo
//                "agent.worker",       // Azure Pipelines
//                "runner.worker",      // GitHub Actions
//                "buildkite-agent",    // BuildKite
//                "circleci-agent",     // CircleCI (Docker, but has specific process name)
//                "agent",              // Semaphore CI (Linux)
//                "sshd: travis [priv]" // Travis CI (Linux)
//            };

//            if ( RuntimeInformation.IsOSPlatform( OSPlatform.Linux ) )
//            {
//                if ( IsRunningInDockerContainer( logger ) )
//                {
//                    logger.Trace?.Log( "Unattended mode detected because of Docker containerized environment." );

//                    return true;
//                }
//            }
//            else if ( RuntimeInformation.IsOSPlatform( OSPlatform.Windows ) )
//            {
//                if ( Environment.OSVersion.Version.Major >= 6 && Process.GetCurrentProcess().SessionId == 0 )
//                {
//                    logger.Trace?.Log( "Unattended mode detected because SessionId = 0 on Windows." );

//                    return true;
//                }
//            }

//            var notUnattendedProcesses = new HashSet<string>
//            {
//                "rider" // Rider needs to be checked, because it can have Java as its parent process.
//            };

//            IReadOnlyList<ProcessInfo> parentProcesses;

//            try
//            {
//                parentProcesses = GetParentProcesses( logger, unattendedProcesses );
//            }
//            catch ( Exception e )
//            {
//                if ( RuntimeInformation.IsOSPlatform( OSPlatform.Linux ) || RuntimeInformation.IsOSPlatform( OSPlatform.OSX ) )
//                {
//                    logger.Warning?.Log( $"Unattended mode detected because the detection was not successful: {e}" );

//                    return true;
//                }
//                else
//                {
//                    logger.Error?.Log( $"Unattended mode detected because the detection was not successful: {e}" );

//                    return false;
//                }
//            }

//            if ( logger.Trace != null )
//            {
//                logger.Trace?.Log( "Parent processes:" );

//                foreach ( var process in parentProcesses )
//                {
//                    logger.Trace?.Log(
//                        process.ImagePath == null ? $"- Unknown process ID {process.ProcessId}" : $"- {process.ProcessName}: {process.ImagePath}" );
//                }
//            }

//            var parentProcessNames = parentProcesses.Where( p => p.ProcessName != null ).Select( p => p.ProcessName! ).ToArray();

//            var notUnattendedProcessName = parentProcessNames.FirstOrDefault( p => notUnattendedProcesses.Contains( p ) );

//            if ( notUnattendedProcessName != null )
//            {
//                logger.Trace?.Log( $"Unattended mode NOT detected because of parent process '{notUnattendedProcessName}'." );

//                return false;
//            }

//            var unattendedProcessName = parentProcessNames.FirstOrDefault( p => unattendedProcesses.Contains( p ) );

//            if ( unattendedProcessName != null )
//            {
//                logger.Trace?.Log( $"Unattended mode detected because of parent process '{unattendedProcessName}'." );

//                return true;
//            }

//            logger.Trace?.Log( "Unattended mode NOT detected." );

//            return false;
//        }
//    }

//    /// <summary>
//    /// Gets the parent processes of the current process.
//    /// </summary>
//    /// <param name="logger">The logger.</param>
//    /// <param name="pivots">List of process names, that will stop the search when encountered. This helps to improve the performance.</param>
//    /// <returns>The list of parent processes of the current process.</returns>
//    public static IReadOnlyList<ProcessInfo> GetParentProcesses( ILogger? logger = null, ISet<string>? pivots = null )
//    {
//        logger ??= NullLogger.Instance;

//        ParentProcessSearchBase parentProcessSearch;

//        if ( RuntimeInformation.IsOSPlatform( OSPlatform.Windows ) )
//        {
//            parentProcessSearch = new ParentProcessSearchWindows( logger );
//        }
//        else if ( RuntimeInformation.IsOSPlatform( OSPlatform.Linux ) )
//        {
//            parentProcessSearch = new ParentProcessSearchLinux( logger );
//        }
//        else if ( RuntimeInformation.IsOSPlatform( OSPlatform.OSX ) )
//        {
//            parentProcessSearch = new ParentProcessSearchMac( logger );
//        }
//        else
//        {
//            throw new NotSupportedException( "Getting parent processes in not supported on the current platform." );
//        }

//        return parentProcessSearch.GetParentProcesses( pivots );
//    }

//    private static bool IsRunningInDockerContainer( ILogger logger )
//    {
//        // If the process is running inside a Docker container,
//        // init (pid '1') process control group collection will have /docker/ as a part of the groups hierarchies.
//        string? processesControlGroup = null;
//        var controlGroupFile = "/proc/1/cgroup";

//        try
//        {
//            processesControlGroup = File.ReadAllText( controlGroupFile );
//        }
//        catch ( Exception e )
//        {
//            logger.Error?.Log( $"Could not read '{controlGroupFile}' file: {e.Message}" );
//        }

//        var isRunningInsideDockerContainer = false;

//        if ( !string.IsNullOrEmpty( processesControlGroup ) )
//        {
//#pragma warning disable CS8602, CA1307
//            isRunningInsideDockerContainer = processesControlGroup.Contains( "docker" );
//#pragma warning restore CS8602, CA1307
//        }

//        return isRunningInsideDockerContainer;
//    }
//}

//internal class NullLogger
//{
//    public static ILogger? Instance { get; internal set; }
//}