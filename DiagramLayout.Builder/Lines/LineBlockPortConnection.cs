﻿using System;
using System.Collections.Generic;
using System.Text;
using DiagramLayout.Builder.Drawing;
using DiagramLayout.Model;
using NetTopologySuite.Geometries;

namespace DiagramLayout.Builder.Lines
{
    public class LineBlockPortConnection
    {
        public LineShapeTypeEnum LineShapeType { get; set; }
        public BlockPort FromPort { get; set; }
        public BlockPort ToPort { get; set; }

        public string Label { get; set; }
        public string Style { get; set; }

        private Guid _refId;
        private string _refClass;

        public void SetReference(Guid refId, string refClass)
        {
            this._refId = refId;
            this._refClass = refClass;
        }

        internal IEnumerable<DiagramObject> CreateDiagramObjects()
        {
            List<DiagramObject> result = new List<DiagramObject>();

            List<Coordinate> pnts = new List<Coordinate>();

            pnts.Add(new Coordinate(GeometryBuilder.Convert(FromPort.PortStartX), GeometryBuilder.Convert(FromPort.PortStartY)));
            pnts.Add(new Coordinate(GeometryBuilder.Convert(FromPort.PortEndX), GeometryBuilder.Convert(FromPort.PortEndY)));
            pnts.Add(new Coordinate(GeometryBuilder.Convert(ToPort.PortEndX), GeometryBuilder.Convert(ToPort.PortEndY)));
            pnts.Add(new Coordinate(GeometryBuilder.Convert(ToPort.PortStartX), GeometryBuilder.Convert(ToPort.PortStartY)));
            pnts.Add(new Coordinate(GeometryBuilder.Convert(FromPort.PortStartX), GeometryBuilder.Convert(FromPort.PortStartY)));

            var ring = new LinearRing(pnts.ToArray());

            var poly = new Polygon(ring);

            result.Add(new DiagramObject()
            {
                Style = Style is null ? "Cable" : Style,
                Label = this.Label,
                Geometry = poly,
                IdentifiedObject = _refClass == null ? null : new IdentifiedObjectReference() { RefId = _refId, RefClass = _refClass }
            });
            return result;

        }
    }
}
