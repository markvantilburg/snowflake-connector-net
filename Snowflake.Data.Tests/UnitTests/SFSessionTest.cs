/*
 * Copyright (c) 2012-2019 Snowflake Computing Inc. All rights reserved.
 */

using Snowflake.Data.Configuration;
using Snowflake.Data.Log;

namespace Snowflake.Data.Tests.UnitTests
{
    using Snowflake.Data.Core;
    using NUnit.Framework;
    using System;

    [TestFixture]
    class SFSessionTest
    {
        // Mock test for session gone
        [Test]
        public void TestSessionGoneWhenClose()
        {
            Mock.MockCloseSessionGone restRequester = new Mock.MockCloseSessionGone();
            SFSession sfSession = new SFSession("account=test;user=test;password=test", null, restRequester);
            sfSession.Open();
            sfSession.close(); // no exception is raised.
        }

        [Test]
        public void TestUpdateDatabaseAndSchema()
        {
            string databaseName = "DB_TEST";
            string schemaName = "SC_TEST";
            
            SFSession sfSession = new SFSession("account=test;user=test;password=test", null);
            sfSession.UpdateDatabaseAndSchema(databaseName, schemaName);

            Assert.AreEqual(databaseName, sfSession.database);
            Assert.AreEqual(schemaName, sfSession.schema);

            // when database or schema name is missing in the response,
            // the cached value should keep unchanged
            sfSession.UpdateDatabaseAndSchema(null, null);
            Assert.AreEqual(databaseName, sfSession.database);
            Assert.AreEqual(schemaName, sfSession.schema);
        }

        [Test]
        [TestCase(null)]
        [TestCase("/some-path/config.json")]
        [TestCase("C:\\some-path\\config.json")]
        public void TestThatConfiguresEasyLogging(string configPath)
        {
            // arrange
            var easyLoggingStarter = new Moq.Mock<EasyLoggingStarter>();
            var simpleConnectionString = "account=test;user=test;password=test;";
            var connectionString = configPath == null
                ? simpleConnectionString
                : $"{simpleConnectionString}client_config_file={configPath};";
            
            // act
            new SFSession(connectionString, null, easyLoggingStarter.Object);
            
            // assert
            easyLoggingStarter.Verify(starter => starter.Init(configPath));
        }

        [Test]
        public void TestThatIdTokenIsStoredWhenCachingIsEnabled()
        {
            // arrange
            var expectedIdToken = "mockIdToken";
            var connectionString = $"account=account;user=user;password=test;allow_sso_token_caching=true";
            var session = new SFSession(connectionString, null);
            LoginResponse authnResponse = new LoginResponse
            {
                data = new LoginResponseData()
                {
                    idToken = expectedIdToken,
                    authResponseSessionInfo = new SessionInfo(),
                },
                success = true
            };

            // act
            session.ProcessLoginResponse(authnResponse);

            // assert
            Assert.AreEqual(expectedIdToken, session._idToken);
        }

        [Test]
        public void TestThatRetriesAuthenticationForInvalidIdToken()
        {
            // arrange
            var connectionString = "account=test;user=test;password=test;allow_sso_token_caching=true";
            var session = new SFSession(connectionString, null);
            LoginResponse authnResponse = new LoginResponse
            {
                code = SFError.ID_TOKEN_INVALID.GetAttribute<SFErrorAttr>().errorCode,
                message = "",
                success = false
            };

            // assert
            Assert.Throws<NullReferenceException>(() => session.ProcessLoginResponse(authnResponse));
        }
    }
}
