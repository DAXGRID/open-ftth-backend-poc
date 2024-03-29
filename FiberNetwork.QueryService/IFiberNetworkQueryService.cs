﻿using Core.ReadModel.Network;
using FiberNetwork.Events.Model;
using System;
using System.Collections.Generic;

namespace FiberNetwork.QueryService
{
    public interface IFiberNetworkQueryService
    {
        bool CheckIfFiberCableIdExists(Guid id);

        FiberCableInfo GetFiberCableInfo(Guid id);

        List<SegmentWithRouteNodeRelationInfo> GetLineSegmentsRelatedToPointOfInterest(Guid pointOfInterestId, string lineId = null);

        List<ISegmentRelation> GetLineSegmentsRelatedToRouteSegment(Guid routeSegmentId, string lineId = null);


        /*
         * 
         * List<ConduitRelationInfo> GetConduitSegmentsRelatedToPointOfInterest(Guid pointOfInterestId, string conduitId = null);
        List<ConduitRelationInfo> GetConduitSegmentsRelatedToRouteSegment(Guid routeSegmentId, string conduitId = null);

        ConduitInfo GetConduitInfo(Guid id);


        SingleConduitInfo GetSingleConduitInfo(Guid id);

        SingleConduitSegmentJunctionInfo GetSingleConduitSegmentJunctionInfo(Guid id);
       
        ConduitLineInfo CreateConduitLineInfoFromConduitSegment(ConduitSegmentInfo sourceConduitSegment);
        */

        void Clean();
    }
}
