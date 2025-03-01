using System;
using System.Collections.Generic;
using System.Text;

namespace DevOpsLibTest
{
    using DevOpsLib;
    using NUnit.Framework;

    [TestFixture]
    public class BuildExtensionTest
    {
        [Test]
        public void TestMasterBranchReporting()
        {
            HashSet<BuildDefinitionId> ids = BuildExtension.BuildDefinitions;

            Assert.AreEqual(17, ids.Count);
            Assert.True(ids.Contains(BuildDefinitionId.CI));
            Assert.True(ids.Contains(BuildDefinitionId.EdgeletCI));
            Assert.True(ids.Contains(BuildDefinitionId.LibiothsmCI));
            Assert.True(ids.Contains(BuildDefinitionId.BuildImages));
            Assert.True(ids.Contains(BuildDefinitionId.EdgeletPackages));
            Assert.True(ids.Contains(BuildDefinitionId.EndToEndTest));
            Assert.True(ids.Contains(BuildDefinitionId.NestedEndToEndTest));
            Assert.True(ids.Contains(BuildDefinitionId.ConnectivityTest));
            Assert.True(ids.Contains(BuildDefinitionId.NestedConnectivityTest));
            Assert.True(ids.Contains(BuildDefinitionId.LonghaulTestEnv1));
            Assert.True(ids.Contains(BuildDefinitionId.LonghaulTestEnv2));
            Assert.True(ids.Contains(BuildDefinitionId.LonghaulTestEnv3));
            Assert.True(ids.Contains(BuildDefinitionId.NestedLonghaulTest));
            Assert.True(ids.Contains(BuildDefinitionId.StressTestEnv1));
            Assert.True(ids.Contains(BuildDefinitionId.StressTestEnv2));
            Assert.True(ids.Contains(BuildDefinitionId.StressTestEnv3));
            Assert.True(ids.Contains(BuildDefinitionId.NestedISA95));
        }

        [Test]
        public void TestDisplayName()
        {
            Assert.AreEqual("Build Images", BuildDefinitionId.BuildImages.DisplayName());
            Assert.AreEqual("CI", BuildDefinitionId.CI.DisplayName());
            Assert.AreEqual("Edgelet CI", BuildDefinitionId.EdgeletCI.DisplayName());
            Assert.AreEqual("Edgelet Packages", BuildDefinitionId.EdgeletPackages.DisplayName());
            Assert.AreEqual("Edgelet Release", BuildDefinitionId.EdgeletRelease.DisplayName());
            Assert.AreEqual("New E2E Test", BuildDefinitionId.EndToEndTest.DisplayName());
            Assert.AreEqual("Image Release", BuildDefinitionId.ImageRelease.DisplayName());
            Assert.AreEqual("Libiothsm CI", BuildDefinitionId.LibiothsmCI.DisplayName());
        }

        [Test]
        public void TestIdString()
        {
            Assert.AreEqual("45137", BuildDefinitionId.CI.IdString());
            Assert.AreEqual("39853", BuildDefinitionId.LibiothsmCI.IdString());
        }
    }
}
