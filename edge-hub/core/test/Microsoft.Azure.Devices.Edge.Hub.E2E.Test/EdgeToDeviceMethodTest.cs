// Copyright (c) Microsoft. All rights reserved.
namespace Microsoft.Azure.Devices.Edge.Hub.E2E.Test
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Edge.Util.Test.Common;
    using Newtonsoft.Json;
    using Xunit;
    using Xunit.Abstractions;
    using EdgeHubConstants = Microsoft.Azure.Devices.Edge.Hub.Service.Constants;
    using IotHubConnectionStringBuilder = Microsoft.Azure.Devices.IotHubConnectionStringBuilder;

    [Integration]
    [Collection("Microsoft.Azure.Devices.Edge.Hub.E2E.Test")]
    public class EdgeToDeviceMethodTest : IDisposable
    {
        TestConsoleLogger logger;

        public EdgeToDeviceMethodTest(ITestOutputHelper testOutputHelper)
        {
            this.logger = new TestConsoleLogger(testOutputHelper);
        }

        public void Dispose()
        {
            this.logger.Dispose();
        }

        [Theory]
        [MemberData(nameof(TestSettings.TransportSettings), MemberType = typeof(TestSettings))]
        public async Task InvokeMethodOnModuleTest(ITransportSettings[] transportSettings)
        {
            // Arrange
            string iotHubConnectionString = await SecretsHelper.GetSecretFromConfigKey("iotHubConnStrKey");
            IotHubConnectionStringBuilder connectionStringBuilder = IotHubConnectionStringBuilder.Create(iotHubConnectionString);
            RegistryManager rm = RegistryManager.CreateFromConnectionString(iotHubConnectionString);
            ModuleClient receiver = null;

            string edgeDeviceConnectionString = ConfigHelper.TestConfig[EdgeHubConstants.ConfigKey.IotHubConnectionString];
            Client.IotHubConnectionStringBuilder edgeHubConnectionStringBuilder = Client.IotHubConnectionStringBuilder.Create(edgeDeviceConnectionString);
            string edgeDeviceId = edgeHubConnectionStringBuilder.DeviceId;

            var request = new TestMethodRequest("Prop1", 10);
            var response = new TestMethodResponse("RespProp1", 20);
            TestMethodRequest receivedRequest = null;

            Task<MethodResponse> MethodHandler(MethodRequest methodRequest, object context)
            {
                receivedRequest = JsonConvert.DeserializeObject<TestMethodRequest>(methodRequest.DataAsJson);
                return Task.FromResult(
                    new MethodResponse(
                        Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)),
                        200));
            }

            string receiverModuleName = "method-module";
            try
            {
                ServiceClient sender = ServiceClient.CreateFromConnectionString(iotHubConnectionString);

                string receiverModuleConnectionString = await RegistryManagerHelper.CreateModuleIfNotExists(rm, connectionStringBuilder.HostName, edgeDeviceId, receiverModuleName);
                receiver = ModuleClient.CreateFromConnectionString(receiverModuleConnectionString, transportSettings);
                await receiver.OpenAsync();
                await receiver.SetMethodHandlerAsync("poke", MethodHandler, null);

                var waitStart = DateTime.Now;
                var isConnected = false;

                while (!isConnected && (DateTime.Now - waitStart) < TimeSpan.FromSeconds(30))
                {
                    var connectedDevice = await rm.GetModuleAsync(edgeDeviceId, receiverModuleName);
                    isConnected = connectedDevice.ConnectionState == DeviceConnectionState.Connected;
                }

                Assert.True(isConnected);

                // Need longer sleep to ensure receiver is completely initialized
                await Task.Delay(TimeSpan.FromSeconds(10));

                // Act
                CloudToDeviceMethodResult cloudToDeviceMethodResult = await sender.InvokeDeviceMethodAsync(
                    edgeDeviceId,
                    receiverModuleName,
                    new CloudToDeviceMethod("poke").SetPayloadJson(JsonConvert.SerializeObject(request)));

                // Assert
                Assert.NotNull(cloudToDeviceMethodResult);
                Assert.NotNull(receivedRequest);
                Assert.Equal(receivedRequest.RequestProp1, request.RequestProp1);
                Assert.Equal(receivedRequest.RequestProp2, request.RequestProp2);

                Assert.Equal(200, cloudToDeviceMethodResult.Status);
                var receivedResponse = JsonConvert.DeserializeObject<TestMethodResponse>(cloudToDeviceMethodResult.GetPayloadAsJson());
                Assert.NotNull(receivedResponse);
                Assert.Equal(receivedResponse.ResponseProp1, response.ResponseProp1);
                Assert.Equal(receivedResponse.ResponseProp2, response.ResponseProp2);
            }
            finally
            {
                await rm.CloseAsync();

                if (receiver != null)
                {
                    await receiver.CloseAsync();
                }

                try
                {
                    await RegistryManagerHelper.RemoveModule(edgeDeviceId, receiverModuleName, rm);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            // wait for the connection to be closed on the Edge side
            await Task.Delay(TimeSpan.FromSeconds(10));
        }

        [Theory]
        [MemberData(nameof(TestSettings.TransportSettings), MemberType = typeof(TestSettings))]
        public async Task InvokeMethodOnDeviceTest(ITransportSettings[] transportSettings)
        {
            // Arrange
            string deviceName = string.Format("deviceMethodTest-{0}", transportSettings.First().GetTransportType().ToString("g"));
            string iotHubConnectionString = await SecretsHelper.GetSecretFromConfigKey("iotHubConnStrKey");
            RegistryManager rm = RegistryManager.CreateFromConnectionString(iotHubConnectionString);
            DeviceClient receiver = null;

            string edgeDeviceConnectionString = ConfigHelper.TestConfig[EdgeHubConstants.ConfigKey.IotHubConnectionString];
            Client.IotHubConnectionStringBuilder edgeHubConnectionStringBuilder = Client.IotHubConnectionStringBuilder.Create(edgeDeviceConnectionString);
            string edgeDeviceId = edgeHubConnectionStringBuilder.DeviceId;
            Device edgeDevice = await rm.GetDeviceAsync(edgeDeviceId);

            var request = new TestMethodRequest("Prop1", 10);
            var response = new TestMethodResponse("RespProp1", 20);
            TestMethodRequest receivedRequest = null;

            Task<MethodResponse> MethodHandler(MethodRequest methodRequest, object context)
            {
                receivedRequest = JsonConvert.DeserializeObject<TestMethodRequest>(methodRequest.DataAsJson);
                return Task.FromResult(
                    new MethodResponse(
                        Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)),
                        200));
            }

            (string deviceId, string receiverModuleConnectionString) = await RegistryManagerHelper.CreateDevice(deviceName, iotHubConnectionString, rm, scope: edgeDevice.Scope);
            try
            {
                ServiceClient sender = ServiceClient.CreateFromConnectionString(iotHubConnectionString);

                receiver = DeviceClient.CreateFromConnectionString(receiverModuleConnectionString, transportSettings);
                await receiver.OpenAsync();
                await receiver.SetMethodHandlerAsync("poke", MethodHandler, null);

                var waitStart = DateTime.Now;
                var isConnected = false;

                while (!isConnected && (DateTime.Now - waitStart) < TimeSpan.FromSeconds(30))
                {
                    var connectedDevice = await rm.GetDeviceAsync(deviceId);
                    isConnected = connectedDevice.ConnectionState == DeviceConnectionState.Connected;
                }

                Assert.True(isConnected);

                // Need longer sleep to ensure receiver is completely initialized
                await Task.Delay(TimeSpan.FromSeconds(10));

                // Act
                CloudToDeviceMethodResult cloudToDeviceMethodResult = await sender.InvokeDeviceMethodAsync(
                    deviceId,
                    new CloudToDeviceMethod("poke").SetPayloadJson(JsonConvert.SerializeObject(request)));

                // Assert
                Assert.NotNull(cloudToDeviceMethodResult);
                Assert.NotNull(receivedRequest);
                Assert.Equal(receivedRequest.RequestProp1, request.RequestProp1);
                Assert.Equal(receivedRequest.RequestProp2, request.RequestProp2);

                Assert.Equal(200, cloudToDeviceMethodResult.Status);
                var receivedResponse = JsonConvert.DeserializeObject<TestMethodResponse>(cloudToDeviceMethodResult.GetPayloadAsJson());
                Assert.NotNull(receivedResponse);
                Assert.Equal(receivedResponse.ResponseProp1, response.ResponseProp1);
                Assert.Equal(receivedResponse.ResponseProp2, response.ResponseProp2);
            }
            finally
            {
                await rm.CloseAsync();

                if (receiver != null)
                {
                    await receiver.CloseAsync();
                }

                try
                {
                    await RegistryManagerHelper.RemoveDevice(deviceId, rm);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            // wait for the connection to be closed on the Edge side
            await Task.Delay(TimeSpan.FromSeconds(10));
        }

        class TestMethodRequest
        {
            [JsonConstructor]
            public TestMethodRequest(string requestProp1, int requestProp2)
            {
                this.RequestProp1 = requestProp1;
                this.RequestProp2 = requestProp2;
            }

            [JsonProperty("requestProp1")]
            public string RequestProp1 { get; }

            [JsonProperty("requestProp2")]
            public int RequestProp2 { get; }
        }

        class TestMethodResponse
        {
            [JsonConstructor]
            public TestMethodResponse(string responseProp1, int responseProp2)
            {
                this.ResponseProp1 = responseProp1;
                this.ResponseProp2 = responseProp2;
            }

            [JsonProperty("responseProp1")]
            public string ResponseProp1 { get; }

            [JsonProperty("responseProp2")]
            public int ResponseProp2 { get; }
        }
    }
}
