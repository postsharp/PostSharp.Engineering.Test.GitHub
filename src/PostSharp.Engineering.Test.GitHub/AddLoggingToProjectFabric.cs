//// Copyright (c) SharpCrafters s.r.o. All rights reserved. Released under the MIT license.

//using Metalama.Framework.Code;
//using Metalama.Framework.Fabrics;

//namespace My.Product;

//internal class AddLoggingToProjectFabric : ProjectFabric
//{
//    public override void AmendProject( IProjectAmender amender )
//    {
//        amender
//            .Outbound
//            .SelectMany( p => p.Types.SelectMany( t => t.Methods ) )
//            .Where( m => m.Accessibility == Accessibility.Public )
//            .AddAspectIfEligible<LogAttribute>();
//    }
//}
