﻿using ConduitNetwork.Events.Model;
using ConduitNetwork.QueryService;
using ConduitNetwork.ReadModel;
using FiberNetwork.Events.Model;
using GraphQL.DataLoader;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Network.Trace;
using QueryModel.Conduit;
using RouteNetwork.QueryService;
using RouteNetwork.ReadModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentService.GraphQL.Types
{
    public class FiberCableSegment : ObjectGraphType<FiberCableSegmentInfo>
    {
        IRouteNetworkState routeNetworkQueryService;
        IConduitNetworkQueryService conduitNetworkEqueryService;

        public FiberCableSegment(IRouteNetworkState routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkEqueryService, IDataLoaderContextAccessor dataLoader, IHttpContextAccessor httpContextAccessor)
        {
            this.routeNetworkQueryService = routeNetworkQueryService;
            this.conduitNetworkEqueryService = conduitNetworkEqueryService;

            var traversal = new TraversalHelper(routeNetworkQueryService);

            Description = "A fiber cable will initially contain one segment that spans the whole length of fiber cable that was originally placed in the route network. When the user starts to cut the cable at various nodes, more fiber cable segments will emerge. However, the original fiber cable asset is the same, now just cut in pieces. The segment represent the pieces. The line represent the original asset. Graph connectivity is maintained on segment level. Use line field to access general asset information. Use the fiberCable field to access fiber cable specific asset information.";

            // Interface fields
            Interface<SegmentInterface>();

            Field(x => x.Id, type: typeof(IdGraphType)).Description("Guid property");

            Field<LineSegmentRelationTypeEnumType>(
            "RelationType",
            resolve: context =>
            {
                Guid routeNodeId = (Guid)httpContextAccessor.HttpContext.Items["routeNodeId"];
                return context.Source.RelationType(routeNodeId);
            });

            Field(x => x.Line.LineKind, type: typeof(LineSegmentKindType)).Description("Type of line segment - i.e. multi conduit, single conduit, fiber cable etc.");

            Field(x => x.Line, type: typeof(LineInterface)).Description("Line that this segment belongs to.");

            Field(x => x.Parents, type: typeof(ListGraphType<SegmentInterface>)).Description("The parent segments of this segment, if this segment is contained within another segment network - i.e. a fiber cable segment running within one of more conduit segments.");

            Field(x => x.Children, type: typeof(ListGraphType<SegmentInterface>)).Description("The child segments of this segment. As an example, if this is multi conduit, then child segments might be fiber cable segments or inner conduit segments running inside the multi conduit.");

            Field<RouteNodeType>(
            "FromRouteNode",
            resolve: context =>
            {
                return routeNetworkQueryService.GetRouteNodeInfo(context.Source.FromRouteNodeId);
            });

            Field<RouteNodeType>(
            "ToRouteNode",
            resolve: context =>
            {
                return routeNetworkQueryService.GetRouteNodeInfo(context.Source.ToRouteNodeId);
            });

            Field<SegmentTraversalType>(
            "Traversal",
            resolve: context =>
            {
                return traversal.CreateTraversalInfoFromSegment(context.Source);
            });

            // Additional fields

            Field<FiberCable>(
            "FiberCable",
            "The original fiber cable that segment belongs to.",
            resolve: context =>
            {
                return context.Source.Line;
            });

            /*
            Field<ConduitLineType>(
            "Line",
            resolve: context =>
            {
                return conduitNetworkEqueryService.CreateConduitLineInfoFromConduitSegment(context.Source);
            });
            */

           

            /*
            Field<ListGraphType<RouteSegmentType>>(
           "AllRouteSegments",
           resolve: context =>
           {
               List<RouteSegmentInfo> result = new List<RouteSegmentInfo>();

               var woi = routeNetworkQueryService.GetWalkOfInterestInfo(context.Source.Conduit.GetRootConduit().WalkOfInterestId).SubWalk2(context.Source.FromRouteNodeId, context.Source.ToRouteNodeId);

               foreach (var segmentId in woi.AllSegmentIds)
               {
                   result.Add(routeNetworkQueryService.GetRouteSegmentInfo(segmentId));
               }

               return result;
           });

            Field<ListGraphType<RouteNodeType>>(
            "AllRouteNodes",
            resolve: context =>
            {
                List<RouteNodeInfo> result = new List<RouteNodeInfo>();

                var woi = routeNetworkQueryService.GetWalkOfInterestInfo(context.Source.Conduit.GetRootConduit().WalkOfInterestId).SubWalk2(context.Source.FromRouteNodeId, context.Source.ToRouteNodeId);

                foreach (var nodeId in woi.AllNodeIds)
                {
                    result.Add(routeNetworkQueryService.GetRouteNodeInfo(nodeId));
                }

                return result;
            });
            */

          

            
        }
    }
}
