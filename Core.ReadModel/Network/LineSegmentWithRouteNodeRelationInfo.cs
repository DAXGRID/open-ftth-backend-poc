﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ReadModel.Network
{
    public class LineSegmentWithRouteNodeRelationInfo
    {
        public Guid RouteNodeId { get; set; }
        public ILineSegment Segment { get; set; }
        public LineSegmentRelationTypeEnum RelationType { get; set; }

        public override string ToString()
        {
            string result = Segment.Line.LineKind.ToString() + " Segment: " + Segment.Id;
            return result;
        }
    }
}
