﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ReadModel.Network
{
    public interface ILineSegment
    {
        LineSegmentKindEnum LineSegmentKind { get; }
    }
}