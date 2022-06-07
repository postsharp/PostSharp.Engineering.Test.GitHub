// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Engineering.BuildTools.Build;
using PostSharp.Engineering.BuildTools.Build.Model;
using PostSharp.Engineering.BuildTools.ContinuousIntegration;
using PostSharp.Engineering.BuildTools.Utilities;
using System;

namespace BuildGitHubTestProduct;

internal class TestPublisher : Publisher
{
    private Pattern Files { get; }

    public TestPublisher( Pattern files )
    {
        this.Files = files;
    }

    public SuccessCode Execute(
        BuildContext context,
        PublishSettings settings,
        BuildInfo buildInfo,
        BuildConfigurationInfo configuration )
    {
        context.Console.WriteHeading( "Running regular test publisher..." );

        context.Console.WriteMessage( $"Publishing {this.Files}." );
        
        if ( TeamCityHelper.IsTeamCityBuild( settings ) )
        {
            context.Console.WriteMessage( "We are on TeamCity." );
        }

        if ( settings.Dry )
        {
            context.Console.WriteImportantMessage( "Dry run: publishing test." );

            return SuccessCode.Success;
        }
        else
        {
            
            return ToolInvocationHelper.InvokeTool(
                context.Console,
                "git",
                "--version",
                Environment.CurrentDirectory )
                ? SuccessCode.Success
                : SuccessCode.Error;
        }
    }
        
    protected sealed override bool Publish(
        BuildContext context,
        PublishSettings settings,
        (string Private, string Public) directories,
        BuildConfigurationInfo configuration,
        BuildInfo buildInfo,
        bool isPublic,
        ref bool hasTarget )
    {
        var success = true;
            
        switch ( this.Execute( context, settings, buildInfo, configuration ) )
        {
            case SuccessCode.Success:
                break;

            case SuccessCode.Error:
                success = false;

                break;

            case SuccessCode.Fatal:
                return false;

            default:
                throw new NotImplementedException();
        }

        if ( !success )
        {
            context.Console.WriteError( "Test publishing has failed." );
        }

        return success;
    }
}