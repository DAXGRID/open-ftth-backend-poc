﻿using AutoMapper;
using ConduitNetwork.Events.Model;
using ConduitNetwork.ReadModel;
using Core.ReadModel.Network;
using Marten;
using RouteNetwork.QueryService;
using RouteNetwork.ReadModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConduitNetwork.QueryService
{
    public class ConduitNetworkQueryService : IConduitNetworkQueryService
    {
        private IDocumentStore documentStore = null;

        private PointOfInterestIndex _pointOfInterestIndex;

        private IRouteNetworkState routeNetworkQueryService = null;

        private Dictionary<Guid, MultiConduitInfo> _multiConduitInfos = new Dictionary<Guid, MultiConduitInfo>();

        private Dictionary<Guid, SingleConduitInfo> _singleConduitInfos = new Dictionary<Guid, SingleConduitInfo>();

        private Dictionary<Guid, SingleConduitSegmentJunctionInfo> _singleConduitJuncionInfos = new Dictionary<Guid, SingleConduitSegmentJunctionInfo>();

        private IMapper _mapper = null;


        public ConduitNetworkQueryService(IDocumentStore documentStore, IRouteNetworkState routeNetworkQueryService)
        {
            this.documentStore = documentStore;
            this.routeNetworkQueryService = routeNetworkQueryService;
            this._pointOfInterestIndex = new PointOfInterestIndex(routeNetworkQueryService);
          

            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<SingleConduitInfo, SingleConduitInfo>();
                cfg.CreateMap<MultiConduitInfo, MultiConduitInfo>();
            });


            _mapper = config.CreateMapper();

            Load();

        }

        private void Load()
        {
            _multiConduitInfos = new Dictionary<Guid, MultiConduitInfo>();
            _singleConduitInfos = new Dictionary<Guid, SingleConduitInfo>();
            _singleConduitJuncionInfos = new Dictionary<Guid, SingleConduitSegmentJunctionInfo>();
            _pointOfInterestIndex = new PointOfInterestIndex(routeNetworkQueryService);

            using (var session = documentStore.LightweightSession())
            {
                // Fetch everything into memory for fast access
                var multiConduits = session.Query<MultiConduitInfo>();

                foreach (var multiConduit in multiConduits)
                {
                    UpdateMultiConduitInfo(multiConduit);
                }

                var singleConduits = session.Query<SingleConduitInfo>();

                foreach (var singleConduit in singleConduits)
                {
                    UpdateSingleConduitInfo(singleConduit);
                }
            }
        }

        public bool CheckIfMultiConduitIdExists(Guid id)
        {
            return _multiConduitInfos.ContainsKey(id);
        }

        public bool CheckIfSingleConduitIdExists(Guid id)
        {
            return _singleConduitInfos.ContainsKey(id);
        }

        public bool CheckIfConduitIsCut(Guid conduitId, Guid pointOfInterestId)
        {
            ConduitInfo conduitToCheck = null;

            if (_singleConduitInfos.ContainsKey(conduitId))
                conduitToCheck = _singleConduitInfos[conduitId];
            else if (_multiConduitInfos.ContainsKey(conduitId))
                    conduitToCheck = _multiConduitInfos[conduitId];
            else
            {
                throw new KeyNotFoundException("Cannot find any conduit with id: " + conduitId);
            }

            // Check if conduit is cut
            if (conduitToCheck.Segments.Exists(s => s.FromRouteNodeId == pointOfInterestId || s.ToRouteNodeId == pointOfInterestId))
                return true;
            else
                return false;
        }

        public ConduitInfo GetConduitInfo(Guid id)
        {
            if (_singleConduitInfos.ContainsKey(id))
                return _singleConduitInfos[id];

            if (_multiConduitInfos.ContainsKey(id))
                return _multiConduitInfos[id];


            throw new KeyNotFoundException("Cannot find any conduit with id: " + id);
        }

        public SingleConduitInfo GetSingleConduitInfo(Guid id)
        {
            if (_singleConduitInfos.ContainsKey(id))
                return _singleConduitInfos[id];

           throw new KeyNotFoundException("Cannot find any single conduit info with id: " + id);
        }

        public MultiConduitInfo GetMultiConduitInfo(Guid id)
        {
            if (_multiConduitInfos.ContainsKey(id))
                return _multiConduitInfos[id];

            throw new KeyNotFoundException("Cannot find any multi conduit info with id: " + id);
        }

        public SingleConduitSegmentJunctionInfo GetSingleConduitSegmentJunctionInfo(Guid id)
        {
            if (_singleConduitJuncionInfos.ContainsKey(id))
                return _singleConduitJuncionInfos[id];

            throw new KeyNotFoundException("Cannot find any single conduit segment conduit info with id: " + id);
        }

        public List<SegmentWithRouteNodeRelationInfo> GetConduitSegmentsRelatedToPointOfInterest(Guid pointOfInterestId, string conduitId = null)
        {
            List<SegmentWithRouteNodeRelationInfo> result = new List<SegmentWithRouteNodeRelationInfo>();

            var conduitSegments = _pointOfInterestIndex.GetConduitSegmentsThatEndsInRouteNode(pointOfInterestId);

            foreach (var conduitSegment in conduitSegments)
            {
                // If conduit segment id set, skip until we read conduit segment specified
                if (conduitId != null)
                {
                    var idToCheck = Guid.Parse(conduitId);

                    if (conduitSegment.Id != idToCheck && conduitSegment.Conduit.Id != idToCheck)
                        continue;
                }

                ConduitRelationInfo relInfo = new ConduitRelationInfo();

                result.Add(new SegmentWithRouteNodeRelationInfo() { RouteNodeId = pointOfInterestId, Segment = conduitSegment, RelationType = conduitSegment.RelationType(pointOfInterestId) });
            }

            var conduitSegmentPassBy = _pointOfInterestIndex.GetConduitSegmentsThatPassedByRouteNode(pointOfInterestId);

            foreach (var conduitSegment in conduitSegmentPassBy)
            {
                // If conduit segment id set, skip until we read conduit segment specified
                if (conduitId != null)
                {
                    var idToCheck = Guid.Parse(conduitId);

                    if (conduitSegment.Id != idToCheck && conduitSegment.Conduit.Id != idToCheck)
                        continue;
                }

                result.Add(new SegmentWithRouteNodeRelationInfo() { RouteNodeId = pointOfInterestId, Segment = conduitSegment, RelationType = SegmentRelationTypeEnum.PassThrough });
            }

            return result;
        }


        public List<ConduitRelationInfo> GetConduitSegmentsRelatedToRouteSegment(Guid routeSegmentId, string conduitId = null)
        {
            List<ConduitRelationInfo> result = new List<ConduitRelationInfo>();

            var conduitSegments = _pointOfInterestIndex.GetConduitSegmentsThatPassedByRouteSegment(routeSegmentId);

            foreach (var conduitSegment in conduitSegments)
            {
                // If conduit segment id set, skip until we read conduit segment specified
                if (conduitId != null)
                {
                    var idToCheck = Guid.Parse(conduitId);

                    if (conduitSegment.Id != idToCheck)
                        continue;
                }

                result.Add(new ConduitRelationInfo() { Segment = conduitSegment, Type = ConduitRelationTypeEnum.PassThrough });
            }

            return result;
        }



        #region utility functions that can be used to create derived info objects
       

        #endregion


        #region functions called during projection and snapshot reading

        public void UpdateMultiConduitInfo(MultiConduitInfo multiConduitInfo, bool load = false)
        {
            // Resolve segment references
            ResolveReferences(multiConduitInfo);

            // Update
            if (_multiConduitInfos.ContainsKey(multiConduitInfo.Id))
            {
                var existingMultiConduitInfo = _multiConduitInfos[multiConduitInfo.Id];

                // Update node to segment dictionary
                _pointOfInterestIndex.Update(existingMultiConduitInfo, multiConduitInfo);

                // Save the children
                multiConduitInfo.Children = new List<ILine>();
                multiConduitInfo.Children.AddRange(existingMultiConduitInfo.Children);

                _mapper.Map<MultiConduitInfo, MultiConduitInfo>(multiConduitInfo, existingMultiConduitInfo);


            }
            // Insert
            else
            {
                // Update node to segment dictionary
                _pointOfInterestIndex.Update(null, multiConduitInfo);

                _multiConduitInfos.Add(multiConduitInfo.Id, multiConduitInfo);
            }
        }

        public void UpdateSingleConduitInfo(SingleConduitInfo singleConduitInfo)
        {
            // Parent multi conduit
            if (singleConduitInfo.MultiConduitId != Guid.Empty)
                singleConduitInfo.Parent = _multiConduitInfos[singleConduitInfo.MultiConduitId];

            // Segment references
            ResolveReferences(singleConduitInfo);

            // Update
            if (_singleConduitInfos.ContainsKey(singleConduitInfo.Id))
            {
                var existingSingleConduitInfo = _singleConduitInfos[singleConduitInfo.Id];

                // Update node to segment dictionary
                _pointOfInterestIndex.Update(existingSingleConduitInfo, singleConduitInfo);

                _mapper.Map<SingleConduitInfo, SingleConduitInfo>(singleConduitInfo, existingSingleConduitInfo);
            }
            // Insert
            else
            {
                // Update node to segment dictionary
                _pointOfInterestIndex.Update(null, singleConduitInfo);

                _singleConduitInfos.Add(singleConduitInfo.Id, singleConduitInfo);

                // If part of multi conduit add reference to it from that one
                if (singleConduitInfo.MultiConduitId != Guid.Empty)
                {
                    // Add to multi conduit children
                    if (_multiConduitInfos[singleConduitInfo.MultiConduitId].Children == null)
                        _multiConduitInfos[singleConduitInfo.MultiConduitId].Children = new List<ILine>();

                    _multiConduitInfos[singleConduitInfo.MultiConduitId].Children.Add(singleConduitInfo);
                }
            }
        }
        

        private void ResolveReferences(ConduitInfo condutiInfo)
        {
            var woi = routeNetworkQueryService.GetWalkOfInterestInfo(condutiInfo.GetRootConduit().WalkOfInterestId);

            // Resolve from node
            if (condutiInfo.FromRouteNode == null)
            {
                condutiInfo.FromRouteNode = routeNetworkQueryService.GetRouteNodeInfo(woi.StartNodeId);
            }

            // Resolve to node
            if (condutiInfo.ToRouteNode == null)
            {
                condutiInfo.ToRouteNode = routeNetworkQueryService.GetRouteNodeInfo(woi.EndNodeId);
            }

            // Resolve references inside segment
            foreach (var segment in condutiInfo.Segments.OfType<ConduitSegmentInfo>())
            {
                // Resolve conduit reference
                segment.Conduit = condutiInfo;

                // Resolve conduit segment parent/child relationship
                if (segment.Conduit.Kind == ConduitKindEnum.InnerConduit)
                {
                    // Create parents list if null
                    if (segment.Parents == null)
                        segment.Parents = new List<ISegment>();

                    var innerConduitSegmentWalkOfInterest = woi.SubWalk2(segment.FromRouteNodeId, segment.ToRouteNodeId);

                    var multiConduit = segment.Conduit.Parent;

                    // Go through each segment of the multi conduit to find if someone intersects with the inner conduit segment
                    foreach (var multiConduitSegment in multiConduit.Segments)
                    {
                        // Create childre list if null
                        if (multiConduitSegment.Children == null)
                            multiConduitSegment.Children = new List<ISegment>();

                        var multiConduitSegmentWalkOfInterest = woi.SubWalk2(multiConduitSegment.FromRouteNodeId, multiConduitSegment.ToRouteNodeId);

                        // Create hash set for quick lookup
                        HashSet<Guid> multiConduitSegmentWalkOfInterestSegmetns = new HashSet<Guid>();
                        foreach (var segmentId in multiConduitSegmentWalkOfInterest.AllSegmentIds)
                            multiConduitSegmentWalkOfInterestSegmetns.Add(segmentId);
                        
                        // check if overlap from segments of the inner conduit to the the multi conduit segment
                        foreach (var innerConduitSegmentId in innerConduitSegmentWalkOfInterest.AllSegmentIds)
                        {
                            if (multiConduitSegmentWalkOfInterestSegmetns.Contains(innerConduitSegmentId))
                            {
                                if (!multiConduitSegment.Children.Contains(segment))
                                    multiConduitSegment.Children.Add(segment);

                                if (!segment.Parents.Contains(multiConduitSegment))
                                    segment.Parents.Add(multiConduitSegment);
                            }
                        }
                    }
                }

                // From Junction
                if (segment.FromNodeId != Guid.Empty)
                {
                    if (!_singleConduitJuncionInfos.ContainsKey(segment.FromNodeId))
                    {
                        var newJunction = new SingleConduitSegmentJunctionInfo() { Id = segment.FromNodeId };
                        newJunction.AddToConduitSegment(segment);
                        _singleConduitJuncionInfos.Add(newJunction.Id, newJunction);
                        segment.FromNode = newJunction;
                    }
                    else
                    {
                        var existingJunction = _singleConduitJuncionInfos[segment.FromNodeId];
                        //existingJunction.ToConduitSegments = segment;
                        existingJunction.AddToConduitSegment(segment);
                        segment.FromNode = existingJunction;
                    }
                }

                // To Junction
                if (segment.ToNodeId != Guid.Empty)
                {
                    if (!_singleConduitJuncionInfos.ContainsKey(segment.ToNodeId))
                    {
                        var newJunction = new SingleConduitSegmentJunctionInfo() { Id = segment.ToNodeId };
                        newJunction.AddFromConduitSegment(segment);
                        _singleConduitJuncionInfos.Add(newJunction.Id, newJunction);
                        segment.ToNode = newJunction;
                    }
                    else
                    {
                        var existingJunction = _singleConduitJuncionInfos[segment.ToNodeId];
                        existingJunction.AddFromConduitSegment(segment);
                        segment.ToNode = existingJunction;
                    }
                }
            }

        }

        #endregion

        public void Clean()
        {
            Load();
        }
    }
}
