﻿// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.Devices.Edge.Hub.Core
{
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Edge.Hub.Core.Cloud;
    using Microsoft.Azure.Devices.Edge.Hub.Core.Device;
    using Microsoft.Azure.Devices.Edge.Util;

    public interface IConnectionManager
    {
        void AddDeviceConnection(IIdentity identity, IDeviceProxy deviceProxy);

        void RemoveDeviceConnection(string deviceId);

        Task<Try<ICloudProxy>> CreateCloudConnectionAsync(IIdentity identity);

        Task<Try<ICloudProxy>> GetOrCreateCloudConnectionAsync(IIdentity identity);

        Option<IDeviceProxy> GetDeviceConnection(string deviceId);

        Option<ICloudProxy> GetCloudConnection(string deviceId);

        Task<bool> CloseConnectionAsync(string deviceId);
    }
}
