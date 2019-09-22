using Microsoft.Extensions.DependencyInjection;
using AutoFixture;
using ConduitNetwork.Business.Commands;
using ConduitNetwork.Business.Tests.Common;
using Infrastructure.EventSourcing;
using MediatR;
using System;
using Xunit;
using ConduitNetwork.Business.Aggregates;
using System.Linq;
using RouteNetwork.Business.Commands;
using RouteNetwork.Events.Model;
using System.Collections.Generic;
using RouteNetwork.QueryService;
using ConduitNetwork.ReadModel;
using ConduitNetwork.QueryService;
using ConduitNetwork.Business.Specifications;

namespace ConduitNetwork.Business.Tests
{
    public class MultiConduitContainerFixture : ContainerFixtureBase
    {
        public MultiConduitContainerFixture() : base("multi_conduit_tests") { }
    }

    public class MultiConduitTests : IClassFixture<MultiConduitContainerFixture>
    {
        private MultiConduitContainerFixture serviceContext;

        private Guid testCabinet1;
        private Guid testJunction1;
        private Guid testJunction2;
        private Guid testSdu1;
        private Guid testSdu2;
        private Guid testSdu3;

        private Guid testCabinet1ToJunction1;
        private Guid testJunction1ToJunction2;
        private Guid testJunction1ToSdu1;
        private Guid testJunction2ToSdu2;
        private Guid testJunction2ToSdu3;

        public MultiConduitTests(MultiConduitContainerFixture containerFixture)
        {
            this.serviceContext = containerFixture;


            // Create a little network for testing
            //
            // cabinet1 -- junction1 -- junction2 - sdu3
            //                 |            |           
            //                sdu1         sdu2
            var fixture = new Fixture();

            // Cabinet 1
            var cabinet1Cmd = fixture.Build<AddNodeCommand>()
                .With(x => x.Name, "cabinet 1")
                .With(x => x.Geometry, new Geometry("Point", "[10, 10]"))
                .With(x => x.NodeKind, RouteNodeKindEnum.CabinetSmall)
                .With(x => x.NodeFunctionKind, RouteNodeFunctionKindEnum.SplicePoint)
                .Create();

            containerFixture.CommandBus.Send(cabinet1Cmd).Wait();
            testCabinet1 = cabinet1Cmd.Id;

            // Junction 1
            var junction1Cmd = fixture.Build<AddNodeCommand>()
                .With(x => x.Name, "cabinet 1")
                .With(x => x.Geometry, new Geometry("Point", "[11, 10]"))
                .With(x => x.NodeKind, RouteNodeKindEnum.CabinetSmall)
                .With(x => x.NodeFunctionKind, RouteNodeFunctionKindEnum.SplicePoint)
                .Create();

            containerFixture.CommandBus.Send(junction1Cmd).Wait();
            testJunction1 = junction1Cmd.Id;

            // Junction 2
            var junction2Cmd = fixture.Build<AddNodeCommand>()
                .With(x => x.Name, "cabinet 1")
                .With(x => x.Geometry, new Geometry("Point", "[12, 10]"))
                .With(x => x.NodeKind, RouteNodeKindEnum.CabinetSmall)
                .With(x => x.NodeFunctionKind, RouteNodeFunctionKindEnum.SplicePoint)
                .Create();

            containerFixture.CommandBus.Send(junction2Cmd).Wait();
            testJunction2 = junction2Cmd.Id;

            // SDU 1
            var sdu1cmd = fixture.Build<AddNodeCommand>()
              .With(x => x.Name, "sdu 1")
              .With(x => x.Geometry, new Geometry("Point", "[11, 11]"))
              .With(x => x.NodeKind, RouteNodeKindEnum.SingleDwellingUnit)
              .With(x => x.NodeFunctionKind, RouteNodeFunctionKindEnum.ServiceDeliveryPoint)
              .Create();

            containerFixture.CommandBus.Send(sdu1cmd).Wait();
            testSdu1 = sdu1cmd.Id;

            // SDU 2
            var sdu2cmd = fixture.Build<AddNodeCommand>()
              .With(x => x.Name, "sdu 2")
              .With(x => x.Geometry, new Geometry("Point", "[12, 11]"))
              .With(x => x.NodeKind, RouteNodeKindEnum.SingleDwellingUnit)
              .With(x => x.NodeFunctionKind, RouteNodeFunctionKindEnum.ServiceDeliveryPoint)
              .Create();

            containerFixture.CommandBus.Send(sdu2cmd).Wait();
            testSdu2 = sdu2cmd.Id;

            // SDU 3
            var sdu3cmd = fixture.Build<AddNodeCommand>()
              .With(x => x.Name, "sdu 3")
              .With(x => x.Geometry, new Geometry("Point", "[13, 10]"))
              .With(x => x.NodeKind, RouteNodeKindEnum.SingleDwellingUnit)
              .With(x => x.NodeFunctionKind, RouteNodeFunctionKindEnum.ServiceDeliveryPoint)
              .Create();

            containerFixture.CommandBus.Send(sdu3cmd).Wait();
            testSdu3 = sdu2cmd.Id;


            // Add segment between cabinet 1 and junction 1
            var addSegment1Cmd = fixture.Build<AddSegmentCommand>()
              .With(x => x.FromNodeId, cabinet1Cmd.Id)
              .With(x => x.ToNodeId, junction1Cmd.Id)
              .With(x => x.Geometry, new Geometry("LineString", "[[10,10],[11,10]]"))
              .With(x => x.SegmentKind, RouteSegmentKindEnum.Underground)
              .Create();

            containerFixture.CommandBus.Send(addSegment1Cmd).Wait();
            testCabinet1ToJunction1 = addSegment1Cmd.Id;

            // Add segment between junction 1 and junction 2
            var addSegment2Cmd = fixture.Build<AddSegmentCommand>()
              .With(x => x.FromNodeId, junction1Cmd.Id)
              .With(x => x.ToNodeId, junction2Cmd.Id)
              .With(x => x.Geometry, new Geometry("LineString", "[[11,10],[12,10]]"))
              .With(x => x.SegmentKind, RouteSegmentKindEnum.Underground)
              .Create();

            containerFixture.CommandBus.Send(addSegment2Cmd).Wait();
            testJunction1ToJunction2 = addSegment2Cmd.Id;

            // Add segment between junction 1 and sdu 1
            var addSegment3Cmd = fixture.Build<AddSegmentCommand>()
              .With(x => x.FromNodeId, junction1Cmd.Id)
              .With(x => x.ToNodeId, sdu1cmd.Id)
              .With(x => x.Geometry, new Geometry("LineString", "[[11,10],[11,11]]"))
              .With(x => x.SegmentKind, RouteSegmentKindEnum.Underground)
              .Create();

            containerFixture.CommandBus.Send(addSegment3Cmd).Wait();
            testJunction1ToSdu1 = addSegment3Cmd.Id;

            // Add segment between junction 2 and sdu 2
            var addSegment4Cmd = fixture.Build<AddSegmentCommand>()
              .With(x => x.FromNodeId, junction2Cmd.Id)
              .With(x => x.ToNodeId, sdu2cmd.Id)
              .With(x => x.Geometry, new Geometry("LineString", "[[12,10],[12,11]]"))
              .With(x => x.SegmentKind, RouteSegmentKindEnum.Underground)
              .Create();

            containerFixture.CommandBus.Send(addSegment4Cmd).Wait();
            testJunction2ToSdu2 = addSegment4Cmd.Id;

            // Add segment between junction 2 and sdu 3
            var addSegment5Cmd = fixture.Build<AddSegmentCommand>()
              .With(x => x.FromNodeId, junction2Cmd.Id)
              .With(x => x.ToNodeId, sdu3cmd.Id)
              .With(x => x.Geometry, new Geometry("LineString", "[[12,10],[13, 10]]"))
              .With(x => x.SegmentKind, RouteSegmentKindEnum.Underground)
              .Create();

            containerFixture.CommandBus.Send(addSegment5Cmd).Wait();
            testJunction2ToSdu3 = addSegment5Cmd.Id;

        }

        [Fact]
        public async void PlaceMultiConduitTest()
        {
            var conduitNetworkQueryService = serviceContext.ServiceProvider.GetService<IConduitNetworkQueryService>();
            var conduitSpecRepo = serviceContext.ServiceProvider.GetService<IConduitSpecificationRepository>();

            var fixture = new Fixture();

            // Register walk from cabinet 1 -> junction 1
            var registerWalkCmd = fixture.Build<RegisterWalkOfInterestCommand>()
                .With(x => x.RouteElementIds, new List<Guid> { testCabinet1, testCabinet1ToJunction1, testJunction1 })
                .Create();

            serviceContext.CommandBus.Send(registerWalkCmd).Wait();

            // Find GM Plast Flatliner 12x12/8 
            var conduitSpec = conduitSpecRepo.GetConduitSpecifications().Find(s => s.Kind == Events.Model.ConduitKindEnum.MultiConduit && s.ProductModels.Exists(m => m.Name.Contains("Flatliner 12")));
                       
            // Create command
            var command1 = fixture.Build<PlaceMultiConduitCommand>()
                .With(x => x.WalkOfInterestId, registerWalkCmd.WalkOfInterestId)
                .With(x => x.Name, string.Empty)
                .With(x => x.DemoDataSpec, String.Empty)
                .With(x => x.ConduitSpecificationId, conduitSpec.Id) // Flatliner with 12 inner conduits, conduit name = 1, red marking
                .Create();

            await serviceContext.CommandBus.Send(command1);

            // Load multi conduit aggregate to check if command and events has been processed correctly
            var multiConduit = serviceContext.AggregateRepository.Load<MultiConduit>(command1.MultiConduitId);

            // Check multi conduit properties
            Assert.Equal(command1.MultiConduitId, multiConduit.Id);
            Assert.Equal(command1.MultiConduitId, multiConduit.OuterConduit.Id);
            Assert.Equal("GM Plast", multiConduit.AssetInfo.Manufacturer.Name);
            Assert.Equal("Flatliner 12x12/8", multiConduit.AssetInfo.Model.Name);

            var multiConduitInfo = conduitNetworkQueryService.GetMultiConduitInfo(command1.MultiConduitId);

            // Check that number of inner conduits is 12
            Assert.Equal(12, multiConduitInfo.Children.Count);

            // Check that inner conduit number 12 is Aqua
            Assert.Equal(Events.Model.ConduitColorEnum.Aqua, multiConduitInfo.Children.Find(c => c.Position == 12).Color);

            // Check various inner conduit properties
            Assert.Equal(1, multiConduitInfo.Children.Count(c => c.Color == Events.Model.ConduitColorEnum.Red));
            Assert.Equal(1, multiConduitInfo.Children.Count(c => c.Color == Events.Model.ConduitColorEnum.Green));
            Assert.Equal(1, multiConduitInfo.Children.Count(c => c.Name.Contains("8")));
        }

        [Fact]
        public async void AddMultiConduitWithSameIdTwiceTest()
        {
            var fixture = new Fixture();

            // Register walk from cabinet 1 -> junction 1
            var registerWalkCmd = fixture.Build<RegisterWalkOfInterestCommand>()
                .With(x => x.RouteElementIds, new List<Guid> { testCabinet1, testCabinet1ToJunction1, testJunction1 })
                .Create();

            serviceContext.CommandBus.Send(registerWalkCmd).Wait();

            // Create a GM Plast Flatliner 12x12/8 using demo data builder
            var command1 = fixture.Build<PlaceMultiConduitCommand>()
                .With(x => x.WalkOfInterestId, registerWalkCmd.WalkOfInterestId)
                .With(x => x.Name, string.Empty)
                .With(x => x.DemoDataSpec, "G12F-1-RD") // Flatliner with 12 inner conduits, conduit name = 1, red marking
                .Create();

            await serviceContext.CommandBus.Send(command1);

            // Create a GM Plast Flatliner 12x12/8 using demo data builder
            var command2 = fixture.Build<PlaceMultiConduitCommand>()
                .With(x => x.WalkOfInterestId, registerWalkCmd.WalkOfInterestId)
                .With(x => x.MultiConduitId, command1.MultiConduitId)
                .With(x => x.Name, string.Empty)
                .With(x => x.DemoDataSpec, "G12F-1-RD") // Flatliner with 12 inner conduits, conduit name = 1, red marking
                .Create();

            Exception ex = await Assert.ThrowsAsync<ArgumentException>(() => serviceContext.CommandBus.Send(command2));
        }

        [Fact]
        public async void AddMultiConduitWithEmptyId()
        {
            var fixture = new Fixture();

            // Create a GM Plast Flatliner 12x12/8 using demo data builder
            var command1 = fixture.Build<PlaceMultiConduitCommand>()
                .With(x => x.MultiConduitId, Guid.Empty)
                .With(x => x.DemoDataSpec, "G12F-1-RD") // Flatliner with 12 inner conduits, conduit name = 1, red marking
                .Create();

            Exception ex = await Assert.ThrowsAsync<ArgumentException>(() => serviceContext.CommandBus.Send(command1));
        }

        [Fact]
        public async void TryCutAtInvalidEnd()
        {
            var conduitNetworkQueryService = serviceContext.ServiceProvider.GetService<IConduitNetworkQueryService>();

            var fixture = new Fixture();

            // Register walk from cabinet 1 -> junction 1 -> junction 2 -> sdu 2
            var registerWalkCmd = fixture.Build<RegisterWalkOfInterestCommand>()
                .With(x => x.RouteElementIds, new List<Guid> { testCabinet1, testCabinet1ToJunction1, testJunction1, testJunction1ToJunction2, testJunction2, testJunction2ToSdu2, testSdu2 })
                .Create();

            await serviceContext.CommandBus.Send(registerWalkCmd);

            // Create a GM Plast Flatliner 10x12/8 using demo data builder
            var createCmd = fixture.Build<PlaceMultiConduitCommand>()
                .With(x => x.WalkOfInterestId, registerWalkCmd.WalkOfInterestId)
                .With(x => x.Name, string.Empty)
                .With(x => x.DemoDataSpec, "G10F-2-BL") // Flatliner with 10 inner conduits, conduit name = 2, black marking
                .Create();

            await serviceContext.CommandBus.Send(createCmd);


            // Cut inner conduit 2 at sdu 2 (cannot be done)
            var cutCmd = fixture.Build<CutInnerConduitCommand>()
                .With(x => x.MultiConduitId, createCmd.MultiConduitId)
                .With(x => x.PointOfInterestId, testSdu2)
                .With(x => x.InnerConduitSequenceNumber, 2)
                .Create();

            Exception ex = await Assert.ThrowsAsync<ArgumentException>(() => serviceContext.CommandBus.Send(cutCmd));
        }

        [Fact]
        public async void CutOuterPlusInnerConduitTest()
        {
            var conduitNetworkQueryService = serviceContext.ServiceProvider.GetService<IConduitNetworkQueryService>();

            var fixture = new Fixture();

            // Register walk from cabinet 1 -> junction 1 -> junction 2 -> sdu 2
            var registerWalkCmd = fixture.Build<RegisterWalkOfInterestCommand>()
                .With(x => x.RouteElementIds, new List<Guid> { testCabinet1, testCabinet1ToJunction1, testJunction1, testJunction1ToJunction2, testJunction2, testJunction2ToSdu2, testSdu2 })
                .Create();

            serviceContext.CommandBus.Send(registerWalkCmd).Wait();

            // Create a GM Plast Flatliner 10x12/8 using demo data builder
            var createCmd = fixture.Build<PlaceMultiConduitCommand>()
                .With(x => x.WalkOfInterestId, registerWalkCmd.WalkOfInterestId)
                .With(x => x.Name, string.Empty)
                .With(x => x.DemoDataSpec, "G10F-2-BL") // Flatliner with 10 inner conduits, conduit name = 2, black marking
                .Create();

            await serviceContext.CommandBus.Send(createCmd);

            var multiConduit = conduitNetworkQueryService.GetMultiConduitInfo(createCmd.MultiConduitId);

            // Cut outer conduit at junction 2
            var cutOuterConduitCmd1 = fixture.Build<CutOuterConduitCommand>()
                .With(x => x.MultiConduitId, createCmd.MultiConduitId)
                .With(x => x.PointOfInterestId, testJunction2)
                .Create();

            await serviceContext.CommandBus.Send(cutOuterConduitCmd1);


            // Cut inner conduit 2 at junction 2
            var cutInnerConduitCmd1 = fixture.Build<CutInnerConduitCommand>()
                .With(x => x.MultiConduitId, createCmd.MultiConduitId)
                .With(x => x.PointOfInterestId, testJunction2)
                .With(x => x.InnerConduitSequenceNumber, 2)
                .Create();

            await serviceContext.CommandBus.Send(cutInnerConduitCmd1);

            // Check that read model has been updated to reflect the new reality
            var innerConduitFromMultiConduit = multiConduit.Children.Find(c => c.Position == 2);
            Assert.Equal(2, innerConduitFromMultiConduit.Segments.Count);

            var innerConduit = conduitNetworkQueryService.GetSingleConduitInfo(innerConduitFromMultiConduit.Id);
            Assert.Equal(2, innerConduit.Segments.Count);

            Assert.Equal(testCabinet1, innerConduit.Segments[0].FromNodeId);
            Assert.Equal(testJunction2, innerConduit.Segments[0].ToNodeId);

            Assert.Equal(testJunction2, innerConduit.Segments[1].FromNodeId);
            Assert.Equal(testSdu2, innerConduit.Segments[1].ToNodeId);

            // Cut outer conduit at junction 1
            var cutOuterConduitCmd2 = fixture.Build<CutOuterConduitCommand>()
                .With(x => x.MultiConduitId, createCmd.MultiConduitId)
                .With(x => x.PointOfInterestId, testJunction1)
                .Create();

            await serviceContext.CommandBus.Send(cutOuterConduitCmd2);

            // Cut inner conduit 2 at junction 1
            var cutInnerConduitCmd2 = fixture.Build<CutInnerConduitCommand>()
                .With(x => x.MultiConduitId, createCmd.MultiConduitId)
                .With(x => x.PointOfInterestId, testJunction1)
                .With(x => x.InnerConduitSequenceNumber, 2)
                .Create();

            await serviceContext.CommandBus.Send(cutInnerConduitCmd2);

            // Check that read model has been updated to reflect the new reality
            innerConduitFromMultiConduit = multiConduit.Children.Find(c => c.Position == 2);
            Assert.Equal(3, innerConduitFromMultiConduit.Segments.Count);

            innerConduit = conduitNetworkQueryService.GetSingleConduitInfo(innerConduitFromMultiConduit.Id);
            Assert.Equal(3, innerConduit.Segments.Count);

            Assert.Equal(testCabinet1, innerConduit.Segments[0].FromNodeId);
            Assert.Equal(testJunction1, innerConduit.Segments[0].ToNodeId);

            Assert.Equal(testJunction1, innerConduit.Segments[1].FromNodeId);
            Assert.Equal(testJunction2, innerConduit.Segments[1].ToNodeId);

            Assert.Equal(testJunction2, innerConduit.Segments[2].FromNodeId);
            Assert.Equal(testSdu2, innerConduit.Segments[2].ToNodeId);

        }

        [Fact]
        public async void ConnectInnerConduitTest()
        {
            var conduitNetworkQueryService = serviceContext.ServiceProvider.GetService<IConduitNetworkQueryService>();
            var conduitSpecRepo = serviceContext.ServiceProvider.GetService<IConduitSpecificationRepository>();
            
            var fixture = new Fixture();

            // Register walk from cabinet 1 -> junction 1 -> junction 2 -> sdu 2
            var multiConduitRegisterWalkCmd = fixture.Build<RegisterWalkOfInterestCommand>()
                .With(x => x.RouteElementIds, new List<Guid> { testCabinet1, testCabinet1ToJunction1, testJunction1, testJunction1ToJunction2, testJunction2, testJunction2ToSdu2, testSdu2 })
                .Create();

            await serviceContext.CommandBus.Send(multiConduitRegisterWalkCmd);


            // Create a GM Plast Flatliner 10x12/8 using demo data builder
            var createMultiConduitCmd = fixture.Build<PlaceMultiConduitCommand>()
                .With(x => x.WalkOfInterestId, multiConduitRegisterWalkCmd.WalkOfInterestId)
                .With(x => x.Name, string.Empty)
                .With(x => x.DemoDataSpec, "G10F-2-BL") // Flatliner with 10 inner conduits, conduit name = 2, black marking
                .Create();

            await serviceContext.CommandBus.Send(createMultiConduitCmd);

            // Register walk from junction 2 -> sdu 2
            var singleConduitRegisterWalkCmd = fixture.Build<RegisterWalkOfInterestCommand>()
                .With(x => x.RouteElementIds, new List<Guid> { testJunction2, testJunction2ToSdu2, testSdu2 })
                .Create();

            await serviceContext.CommandBus.Send(singleConduitRegisterWalkCmd);


            // Create a �12 
            var conduitSpec = conduitSpecRepo.GetConduitSpecifications().Find(s => s.Kind == Events.Model.ConduitKindEnum.SingleConduit && s.Color == Events.Model.ConduitColorEnum.Orange);
            
            var createSingleConduitCmd = fixture.Build<PlaceSingleConduitCommand>()
                .With(x => x.WalkOfInterestId, singleConduitRegisterWalkCmd.WalkOfInterestId)
                .With(x => x.Name, string.Empty)
                .With(x => x.DemoDataSpec, string.Empty)
                .With(x => x.ConduitSpecificationId, conduitSpec.Id)
                .Create();

            await serviceContext.CommandBus.Send(createSingleConduitCmd);

            // Cut outer conduit at junction 2
            var cutOuterConduitCmd = fixture.Build<CutOuterConduitCommand>()
                .With(x => x.MultiConduitId, createMultiConduitCmd.MultiConduitId)
                .With(x => x.PointOfInterestId, testJunction2)
                .Create();

            await serviceContext.CommandBus.Send(cutOuterConduitCmd);


            // Cut inner conduit 2 at junction 2
            var cutInnerConduitCmd = fixture.Build<CutInnerConduitCommand>()
                .With(x => x.MultiConduitId, createMultiConduitCmd.MultiConduitId)
                .With(x => x.PointOfInterestId, testJunction2)
                .With(x => x.InnerConduitSequenceNumber, 2)
                .Create();

            await serviceContext.CommandBus.Send(cutInnerConduitCmd);

            // The junction we're going to connected the cut inner conduit to, as well as the conduit going to SDU
            Guid ourConduitJunctionId = Guid.NewGuid();

            // Connect inner conduit 2 at junction 2 to a conduit junction
            var connectCmd1 = fixture.Build<ConnectInnerConduitToJunction>()
                .With(x => x.PointOfInterestId, testJunction2)
                .With(x => x.MultiConduitId, createMultiConduitCmd.MultiConduitId)
                .With(x => x.InnerConduitSequenceNumber, 2)
                .With(x => x.ConnectedJunctionId, ourConduitJunctionId)
                .With(x => x.ConnectedEndKind, Events.Model.ConduitEndKindEnum.Incomming)
                .Create();

            await serviceContext.CommandBus.Send(connectCmd1);


            // Connect SDU conduit to junction
            var connectCmd2 = fixture.Build<ConnectSingleConduitToJunction>()
                .With(x => x.PointOfInterestId, testJunction2)
                .With(x => x.SingleConduitId, createSingleConduitCmd.SingleConduitId)
                .With(x => x.ConnectedJunctionId, ourConduitJunctionId)
                .With(x => x.ConnectedEndKind, Events.Model.ConduitEndKindEnum.Outgoing)
                .Create();

            await serviceContext.CommandBus.Send(connectCmd2);


            //////////////////////////////////////////////////////////////////////////////////////
            // Check that stuff is connected to each other as expected
            //////////////////////////////////////////////////////////////////////////////////////
            var multiConduitInfo = conduitNetworkQueryService.GetMultiConduitInfo(createMultiConduitCmd.MultiConduitId);
            var singleConduitJunction = conduitNetworkQueryService.GetSingleConduitSegmentJunctionInfo(ourConduitJunctionId);

            // The inner conduit must be connected to our conduit junction
            var innerConduitInfo = multiConduitInfo.Children.Find(c => c.Position == 2);
            Assert.Single(innerConduitInfo.Segments.Where(s => s.NeighborElements.Contains(singleConduitJunction)));

            // The SDU conduit must be connected to our conduit junction as well
            var mduConduitInfo = conduitNetworkQueryService.GetSingleConduitInfo(createSingleConduitCmd.SingleConduitId);
            Assert.Single(mduConduitInfo.Segments.Where(s => s.NeighborElements.Contains(singleConduitJunction)));

            // Traverse the inner duct segment, should result in three elements (the two segments and the junction inbetween)
            var result = innerConduitInfo.Segments[0].UndirectionalDFS<SingleConduitSegmentJunctionInfo, SingleConduitSegmentInfo>();
            Assert.Equal(3, result.Count());


            //////////////////////////////////////////////////////////////////////////////////////
            // Check that conduit query service works as expected
            //////////////////////////////////////////////////////////////////////////////////////


            // In cabinet 1, expect all segments to be outgoing
            Assert.Equal(11, conduitNetworkQueryService.GetConduitSegmentsRelatedToPointOfInterest(testCabinet1).Count());
            Assert.Equal(11, conduitNetworkQueryService.GetConduitSegmentsRelatedToPointOfInterest(testCabinet1).Count(r => r.Type == ConduitRelationTypeEnum.Outgoing));

            // In junction 1, axpect all segments be pass through
            Assert.Equal(11, conduitNetworkQueryService.GetConduitSegmentsRelatedToPointOfInterest(testJunction1).Count());
            Assert.Equal(11, conduitNetworkQueryService.GetConduitSegmentsRelatedToPointOfInterest(testJunction1).Count(r => r.Type == ConduitRelationTypeEnum.PassThrough));

            // In junction 2, expect 9 segments to be pass through
            Assert.Equal(9, conduitNetworkQueryService.GetConduitSegmentsRelatedToPointOfInterest(testJunction2).Count(r => r.Type == ConduitRelationTypeEnum.PassThrough));

            // In junction 2, expect 2 segments to be incomming
            Assert.Equal(2, conduitNetworkQueryService.GetConduitSegmentsRelatedToPointOfInterest(testJunction2).Count(r => r.Type == ConduitRelationTypeEnum.Incomming));

            // In sdu 2, expect 12 segments to be incomming (because both multi duct and single duct end there)
            Assert.Equal(12, conduitNetworkQueryService.GetConduitSegmentsRelatedToPointOfInterest(testSdu2).Count(r => r.Type == ConduitRelationTypeEnum.Incomming));

            // In sdu 1, expect none segments
            Assert.Empty(conduitNetworkQueryService.GetConduitSegmentsRelatedToPointOfInterest(testSdu1));

        }

    }
}