// Copyright (c) SharpCrafters s.r.o. All rights reserved. Released under the MIT license.

using BuildGitHubTestProduct;
using PostSharp.Engineering.BuildTools;
using PostSharp.Engineering.BuildTools.Build;
using PostSharp.Engineering.BuildTools.Build.Model;
using PostSharp.Engineering.BuildTools.Build.Publishers;
using PostSharp.Engineering.BuildTools.Build.Solutions;
using PostSharp.Engineering.BuildTools.Dependencies.Definitions;
using Spectre.Console.Cli;
using TestDependencies = PostSharp.Engineering.BuildTools.Dependencies.Definitions.TestDependencies.V2023_1;

var product = new Product( TestDependencies.GitHub )
{
    Solutions = new Solution[] { new DotNetSolution( "src\\PostSharp.Engineering.Test.GitHub.sln" ) },
    PublicArtifacts = Pattern.Create( "PostSharp.Engineering.Test.GitHub.$(PackageVersion).nupkg" ),
    Dependencies = new[] { DevelopmentDependencies.PostSharpEngineering, TestDependencies.TestProduct },
    Configurations = Product.DefaultConfigurations
        .WithValue( BuildConfiguration.Public,
            b => b with
            {
                PublicPublishers = new Publisher[] {
                    new TestPublisher( Pattern.Create( "*.nupkg" ) ),
                    new MergePublisher()
                }
            } )
};

var commandApp = new CommandApp();

commandApp.AddProductCommands( product );

return commandApp.Run( args );