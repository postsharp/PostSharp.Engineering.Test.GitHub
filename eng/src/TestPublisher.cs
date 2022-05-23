// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Engineering.BuildTools.Build;
using PostSharp.Engineering.BuildTools.Build.Model;
using PostSharp.Engineering.BuildTools.ContinuousIntegration;
using PostSharp.Engineering.BuildTools.Utilities;
using System;

namespace BuildGitHubTestProduct;

internal class TestPublisher : ArtifactPublisher
{
    public TestPublisher(Pattern files) : base(files)
    {
    }

    public override SuccessCode Execute(
        BuildContext context,
        PublishSettings settings,
        string? file,
        BuildInfo buildInfo,
        BuildConfigurationInfo configuration )
    {
        var hasEnvironmentError = false;

        context.Console.WriteMessage( $"Publishing {file}." );
        
        if ( TeamCityHelper.IsTeamCityBuild( settings ) )
        {
            context.Console.WriteMessage( "We are on TeamCity." );
        }
        
        if ( hasEnvironmentError )
        {
            return SuccessCode.Fatal;
        }

        if ( settings.Dry )
        {
            context.Console.WriteImportantMessage( "Dry run: publishing test." );

            return SuccessCode.Success;
        }
        else
        {
            context.Console.WriteImportantMessage ( "Running regular test publisher..." ); 
            
            return ToolInvocationHelper.InvokeTool(
                context.Console,
                "git",
                "--version",
                Environment.CurrentDirectory )
                ? SuccessCode.Success
                : SuccessCode.Error;
        }
    }
}