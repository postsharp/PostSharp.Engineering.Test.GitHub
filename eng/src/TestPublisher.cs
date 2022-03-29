// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Engineering.BuildTools.Build;
using PostSharp.Engineering.BuildTools.Build.Model;
using System;

namespace BuildGitHubTestProduct;

internal class TestPublisher : Publisher
{
    public TestPublisher(Pattern files) : base(files)
    {
    }

    public override SuccessCode Execute(
        BuildContext context,
        PublishSettings settings,
        string file,
        BuildInfo buildInfo,
        BuildConfigurationInfo configuration )
    {
        if ( Environment.GetEnvironmentVariable( "TEAMCITY_GIT_PATH" ) != null )
        {
            context.Console.WriteMessage( "We are on TeamCity" );
        }

        context.Console.WriteMessage( "Published." );
        
        return SuccessCode.Success;
    }
}