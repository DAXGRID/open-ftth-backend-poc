﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ReadModel.Network
{
    public interface INetworkElement
    {
        Guid Id { get; }
    }
}