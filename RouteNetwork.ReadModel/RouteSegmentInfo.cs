﻿using Core.GraphSupport.Model;
using Core.ReadModel.Network;
using RouteNetwork.Events.Model;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace RouteNetwork.ReadModel
{
    public sealed class RouteSegmentInfo : GraphEdge, IRouteElementInfo,  INetworkElement
    {
        public Guid Id { get; set; }
        public Guid FromNodeId { get; set; }
        public Guid ToNodeId { get; set; }
        public string Name { get; set; }
        public RouteSegmentKindEnum SegmentKind { get; set; }
        public Geometry Geometry { get; set; }

        public LineKindEnum LineKind
        {
            get
            {
                return LineKindEnum.Route;
            }
        }

        [IgnoreDataMember]
        public double Length { get; set; }

        [IgnoreDataMember]
        public RouteNodeInfo FromNode { get; set; }

        [IgnoreDataMember]
        public RouteNodeInfo ToNode { get; set; }

        [IgnoreDataMember]
        private List<WalkOfInterestInfo> _walkOfInterests { get; set; }

        public void AddWalkOfInterest(WalkOfInterestInfo walkOfInterest)
        {
            if (_walkOfInterests == null)
                _walkOfInterests = new List<WalkOfInterestInfo>();

            _walkOfInterests.Add(walkOfInterest);
        }

        [IgnoreDataMember]
        public override List<IGraphElement> OutgoingElements
        {
            get
            {
                return new List<IGraphElement>() { ToNode };
            }
        }

        [IgnoreDataMember]
        public override List<IGraphElement> IngoingElements
        {
            get
            {
                return new List<IGraphElement>() { FromNode };
            }
        }

        [IgnoreDataMember]
        public override List<IGraphElement> NeighborElements
        {
            get
            {
                return new List<IGraphElement>() { ToNode, FromNode };
            }
        }

        [IgnoreDataMember]
        public List<WalkOfInterestInfo> WalkOfInterests
        {
            get
            {
                if (_walkOfInterests != null)
                    return _walkOfInterests;
                else

                    return new List<WalkOfInterestInfo>();
            }
        }

        public LineKindEnum LineSegmentKind => LineKindEnum.Route;

        /*
        List<ILine> ILine.Children {
            get { return new List<ILine>(); }
            set { }
        }

        List<ILineSegment> ILine.Segments
        {
            get { return new List<ILineSegment>(); }
            set { }
        }

        public ILine Parent { get { return null; } set { } }

        public int SequenceNumber { get; set; }
        */

        public override string ToString()
        {
            return "RouteSegment (" + FromNode.Name + " -> " + ToNode.Name + ")";
        }
    }
}
