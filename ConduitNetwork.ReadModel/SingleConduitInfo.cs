﻿using Asset.Model;
using ConduitNetwork.Events.Model;
using Core.ReadModel.Network;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ConduitNetwork.ReadModel
{
    public class SingleConduitInfo : ConduitInfo
    {
        public Guid MultiConduitId { get; set; }

        public override LineKindEnum LineKind
        {
            get
            {
                if (this.MultiConduitId == null || this.MultiConduitId == Guid.Empty)
                    return LineKindEnum.SingleConduit;
                else
                    return LineKindEnum.InnerConduit;
            }
        }

        public override string ToString()
        {
            string result = Name;

            if (SequenceNumber != 0)
                result += " (" + SequenceNumber + ")";

            if (Parent != null)
                result += " -> " + Parent.ToString();

            return result;
        }

      
    }
}
