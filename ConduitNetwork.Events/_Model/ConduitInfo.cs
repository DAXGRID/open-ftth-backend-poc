﻿using Asset.Model;
using ConduitNetwork.Events.Model;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ConduitNetwork.Events.Model
{
    public class ConduitInfo
    {
        public Guid Id { get; set; }
        public Guid WalkOfInterestId { get; set; }
        public ConduitKindEnum Kind { get; set; }
        public ConduitShapeKindEnum Shape { get; set; }
        public ConduitColorEnum Color { get; set; }
        public string Name { get; set; }
        public int OuterDiameter { get; set; }
        public int InnerDiameter { get; set; }
        public string TextMarking { get; set; }
        public ConduitColorEnum ColorMarking { get; set; }
        public int Position { get; set; }
        public AssetInfo AssetInfo { get; set; }
        public List<ConduitSegmentInfo> Segments { get; set; }


        #region Properties that should not be persisted

        [IgnoreDataMember]
        public List<ConduitInfo> Children { get; set; }

        [IgnoreDataMember]
        public ConduitInfo Parent { get; set; }

        #endregion


        public virtual ConduitInfo GetRootConduit()
        {
            if (Parent != null)
                return Parent;
            else
                return this;
        }

        
    }
}