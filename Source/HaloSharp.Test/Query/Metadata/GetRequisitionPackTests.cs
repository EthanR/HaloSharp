﻿using HaloSharp.Exception;
using HaloSharp.Extension;
using HaloSharp.Model;
using HaloSharp.Model.Metadata.Common;
using HaloSharp.Query.Metadata;
using HaloSharp.Test.Utility;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;

namespace HaloSharp.Test.Query.Metadata
{
    [TestFixture]
    public class GetRequisitionPackTests
    {
        private IHaloSession _mockSession;
        private RequisitionPack _requisitionPack;

        [SetUp]
        public void Setup()
        {
            _requisitionPack = JsonConvert.DeserializeObject<RequisitionPack>(File.ReadAllText(Config.RequisitionJsonPath));

            var mock = new Mock<IHaloSession>();
            mock.Setup(m => m.Get<RequisitionPack>(It.IsAny<string>()))
                .ReturnsAsync(_requisitionPack);

            _mockSession = mock.Object;
        }

        [Test]
        public void GetConstructedUri_NoParamaters_MatchesExpected()
        {
            var query = new GetRequisitionPack();

            var uri = query.GetConstructedUri();

            Assert.AreEqual("metadata/h5/metadata/requisition-packs/", uri);
        }

        [Test]
        [TestCase("b4d192cb-af29-4976-af83-7746a6f14fce")] //34X Gold +13 Bonus
        [TestCase("3a1614d9-20a4-4817-a189-88cb781e9152")] //Gold
        [TestCase("3ce05b60-a118-4ad1-9617-bc04f64ac4d8")] //Silver
        [TestCase("5f96269a-58f8-473e-9897-42a4deb1bf09")] //Bronze
        public void GetConstructedUri_ForRequisitionPackId_MatchesExpected(string guid)
        {
            var query = new GetRequisitionPack()
                .ForRequisitionPackId(new Guid(guid));

            var uri = query.GetConstructedUri();

            Assert.AreEqual($"metadata/h5/metadata/requisition-packs/{guid}", uri);
        }

        [Test]
        public async Task Query_DoesNotThrow()
        {
            var query = new GetRequisitionPack();

            var result = await _mockSession.Query(query);

            Assert.IsInstanceOf(typeof(RequisitionPack), result);
            Assert.AreEqual(_requisitionPack, result);
        }

        [Test]
        [TestCase("b4d192cb-af29-4976-af83-7746a6f14fce")] //34X Gold +13 Bonus
        [TestCase("3a1614d9-20a4-4817-a189-88cb781e9152")] //Gold
        [TestCase("3ce05b60-a118-4ad1-9617-bc04f64ac4d8")] //Silver
        [TestCase("5f96269a-58f8-473e-9897-42a4deb1bf09")] //Bronze
        public async Task GetRequisitionPack_DoesNotThrow(string guid)
        {
            var query = new GetRequisitionPack()
                .ForRequisitionPackId(new Guid(guid))
                .SkipCache();

            var result = await Global.Session.Query(query);

            Assert.IsInstanceOf(typeof(RequisitionPack), result);
        }

        [Test]
        [TestCase("b4d192cb-af29-4976-af83-7746a6f14fce")] //34X Gold +13 Bonus
        [TestCase("3a1614d9-20a4-4817-a189-88cb781e9152")] //Gold
        [TestCase("3ce05b60-a118-4ad1-9617-bc04f64ac4d8")] //Silver
        [TestCase("5f96269a-58f8-473e-9897-42a4deb1bf09")] //Bronze
        public async Task GetRequisitionPack_SchemaIsValid(string guid)
        {
            var schema = JSchema.Parse(File.ReadAllText(Config.RequisitionPackJsonSchemaPath), new JSchemaReaderSettings
            {
                Resolver = new JSchemaUrlResolver(),
                BaseUri = new Uri(Path.GetFullPath(Config.RequisitionPackJsonSchemaPath))
            });

            var query = new GetRequisitionPack()
                .ForRequisitionPackId(new Guid(guid))
                .SkipCache();

            var jContainer = await Global.Session.Get<JContainer>(query.GetConstructedUri());

            SchemaUtility.AssertSchemaIsValid(schema, jContainer);
        }

        [Test]
        [TestCase("b4d192cb-af29-4976-af83-7746a6f14fce")] //34X Gold +13 Bonus
        [TestCase("3a1614d9-20a4-4817-a189-88cb781e9152")] //Gold
        [TestCase("3ce05b60-a118-4ad1-9617-bc04f64ac4d8")] //Silver
        [TestCase("5f96269a-58f8-473e-9897-42a4deb1bf09")] //Bronze
        public async Task GetRequisitionPack_ModelMatchesSchema(string guid)
        {
            var schema = JSchema.Parse(File.ReadAllText(Config.RequisitionPackJsonSchemaPath), new JSchemaReaderSettings
            {
                Resolver = new JSchemaUrlResolver(),
                BaseUri = new Uri(Path.GetFullPath(Config.RequisitionPackJsonSchemaPath))
            });

            var query = new GetRequisitionPack()
                .ForRequisitionPackId(new Guid(guid))
                .SkipCache();

            var result = await Global.Session.Query(query);

            var json = JsonConvert.SerializeObject(result);
            var jContainer = JsonConvert.DeserializeObject<JContainer>(json);

            SchemaUtility.AssertSchemaIsValid(schema, jContainer);
        }

        [Test]
        [TestCase("b4d192cb-af29-4976-af83-7746a6f14fce")] //34X Gold +13 Bonus
        [TestCase("3a1614d9-20a4-4817-a189-88cb781e9152")] //Gold
        [TestCase("3ce05b60-a118-4ad1-9617-bc04f64ac4d8")] //Silver
        [TestCase("5f96269a-58f8-473e-9897-42a4deb1bf09")] //Bronze
        public async Task GetRequisitionPack_IsSerializable(string guid)
        {
            var query = new GetRequisitionPack()
                .ForRequisitionPackId(new Guid(guid))
                .SkipCache();

            var result = await Global.Session.Query(query);

            SerializationUtility<RequisitionPack>.AssertRoundTripSerializationIsPossible(result);
        }

        [Test]
        [TestCase("00000000-0000-0000-0000-000000000000")]
        public async Task GetRequisitionPack_InvalidGuid(string guid)
        {
            var query = new GetRequisitionPack()
                .ForRequisitionPackId(new Guid(guid))
                .SkipCache();

            try
            {
                await Global.Session.Query(query);
                Assert.Fail("An exception should have been thrown");
            }
            catch (HaloApiException e)
            {
                Assert.AreEqual((int)Enumeration.StatusCode.NotFound, e.HaloApiError.StatusCode);
            }
            catch (System.Exception e)
            {
                Assert.Fail("Unexpected exception of type {0} caught: {1}", e.GetType(), e.Message);
            }
        }

        [Test]
        public async Task GetRequisitionPack_MissingGuid()
        {
            var query = new GetRequisitionPack()
                .SkipCache();

            try
            {
                await Global.Session.Query(query);
                Assert.Fail("An exception should have been thrown");
            }
            catch (HaloApiException e)
            {
                Assert.AreEqual((int)Enumeration.StatusCode.NotFound, e.HaloApiError.StatusCode);
            }
            catch (System.Exception e)
            {
                Assert.Fail("Unexpected exception of type {0} caught: {1}", e.GetType(), e.Message);
            }
        }
    }
}