// Copyright (c) SharpCrafters s.r.o. All rights reserved. Released under the MIT license.

using BuildGitHubTestProduct;
using PostSharp.Engineering.BuildTools;
using PostSharp.Engineering.BuildTools.Build;
using PostSharp.Engineering.BuildTools.Build.Model;
using PostSharp.Engineering.BuildTools.Build.Solutions;
using PostSharp.Engineering.BuildTools.Dependencies.Definitions;
using TestDependencies = PostSharp.Engineering.BuildTools.Dependencies.Definitions.TestDependencies.V2023_1;

var product = new Product( TestDependencies.GitHub )
{
    Solutions = [new DotNetSolution( "src\\PostSharp.Engineering.Test.GitHub.sln" )],
    PublicArtifacts = Pattern.Create( "PostSharp.Engineering.Test.GitHub.$(PackageVersion).nupkg" ),
    Dependencies = [DevelopmentDependencies.PostSharpEngineering, TestDependencies.TestProduct],
    Configurations = Product.DefaultConfigurations
        .WithValue( BuildConfiguration.Public,
            b => b with
            {
                PublicPublishers =
                [
                    new TestPublisher( Pattern.Create( "*.nupkg" ) )
                ]
            } )
};

return new EngineeringApp( product ).Run( args );