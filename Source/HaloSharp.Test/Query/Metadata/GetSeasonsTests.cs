﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HaloSharp.Extension;
using HaloSharp.Model.Metadata;
using HaloSharp.Query.Metadata;
using HaloSharp.Test.Utility;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using NUnit.Framework;

namespace HaloSharp.Test.Query.Metadata
{
    [TestFixture]
    public class GetSeasonsTests
    {
        private IHaloSession _mockSession;
        private List<Season> _seasons;

        [SetUp]
        public void Setup()
        {
            _seasons = JsonConvert.DeserializeObject<List<Season>>(File.ReadAllText(Config.SeasonsJsonPath));
            
            var mock = new Mock<IHaloSession>();
            mock.Setup(m => m.Get<List<Season>>(It.IsAny<string>()))
                .ReturnsAsync(_seasons);

            _mockSession = mock.Object;
        }

        [Test]
        public void GetConstructedUri_NoParameters_MatchesExpected()
        {
            var query = new GetSeasons();

            var uri = query.GetConstructedUri();

            Assert.AreEqual("metadata/h5/metadata/seasons", uri);
        }

        [Test]
        public async Task Query_DoesNotThrow()
        {
            var query = new GetSeasons()
                .SkipCache();

            var result = await _mockSession.Query(query);

            Assert.IsInstanceOf(typeof (List<Season>), result);
            Assert.AreEqual(_seasons, result);
        }

        [Test]
        public async Task GetSeasons_DoesNotThrow()
        {
            var query = new GetSeasons()
                .SkipCache();

            var result = await Global.Session.Query(query);

            Assert.IsInstanceOf(typeof(List<Season>), result);
        }

        [Test]
        public async Task GetSeasons_SchemaIsValid()
        {
            var seasonsSchema = JSchema.Parse(File.ReadAllText(Config.SeasonsJsonSchemaPath), new JSchemaReaderSettings
            {
                Resolver = new JSchemaUrlResolver(),
                BaseUri = new Uri(Path.GetFullPath(Config.SeasonsJsonSchemaPath))
            });

            var query = new GetSeasons()
               .SkipCache();

            var jArray = await Global.Session.Get<JArray>(query.GetConstructedUri());

            SchemaUtility.AssertSchemaIsValid(seasonsSchema, jArray);
        }

        [Test]
        public async Task GetSeasons_ModelMatchesSchema()
        {
            var schema = JSchema.Parse(File.ReadAllText(Config.SeasonsJsonSchemaPath), new JSchemaReaderSettings
            {
                Resolver = new JSchemaUrlResolver(),
                BaseUri = new Uri(Path.GetFullPath(Config.SeasonsJsonSchemaPath))
            });

            var query = new GetSeasons()
                .SkipCache();

            var result = await Global.Session.Query(query);

            var json = JsonConvert.SerializeObject(result);
            var jContainer = JsonConvert.DeserializeObject<JContainer>(json);

            SchemaUtility.AssertSchemaIsValid(schema, jContainer);
        }

        [Test]
        public async Task GetSeasons_IsSerializable()
        {
            var query = new GetSeasons()
               .SkipCache();

            var result = await Global.Session.Query(query);

            SerializationUtility<List<Season>>.AssertRoundTripSerializationIsPossible(result);
        }
    }
}