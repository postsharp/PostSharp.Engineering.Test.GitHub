// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using BuildGitHubTestProduct;
using PostSharp.Engineering.BuildTools;
using PostSharp.Engineering.BuildTools.Build;
using PostSharp.Engineering.BuildTools.Build.Model;
using PostSharp.Engineering.BuildTools.Build.Solutions;
using PostSharp.Engineering.BuildTools.Dependencies.Model;
using Spectre.Console.Cli;

var testFile = Pattern.Create( "testssssda" );

var product = new Product( TestDependencies.GitHub )
{
    ProductName = "PostSharp.Engineering.Test.GitHub",
    Solutions = new Solution[] { new DotNetSolution( "src\\PostSharp.Engineering.Test.GitHub.sln" ) },
    PublicArtifacts = Pattern.Create( "PostSharp.Engineering.Test.GitHub.$(PackageVersion).nupkg" ),
    Dependencies = new[] { Dependencies.PostSharpEngineering },
    Configurations = Product.DefaultConfigurations
        .WithValue( BuildConfiguration.Public, new BuildConfigurationInfo( 
            MSBuildName: "Release",
            PublicPublishers: new Publisher[]
            {
                new TestPublisher( testFile )
            } ) ),
    RequiresBranchMerging = true
};

var commandApp = new CommandApp();

commandApp.AddProductCommands( product );

return commandApp.Run( args );