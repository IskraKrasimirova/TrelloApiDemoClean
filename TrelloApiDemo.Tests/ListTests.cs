using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestSharp;
using System;
using System.Reflection;
using TrelloApiDemo.Helpers;
using TrelloApiDemo.Models;

namespace TrelloApiDemo.Tests
{
    [TestClass]
    public class ListTests
    {
        private TrelloClient _client;
        private string _boardId;

        [TestInitialize]
        public void Setup()
        {
            var configuration = TestConfig.LoadConfiguration();

            Config.Key = configuration["Trello:Key"];
            Config.Token = configuration["Trello:Token"];
            Config.BaseUrl = configuration["Trello:BaseUrl"];

            _client = new TrelloClient();

            // Create board for list tests
            var boardResponse = _client.CreateBoard("ListTestBoard_" + Guid.NewGuid());
            Assert.AreEqual(200, (int)boardResponse.StatusCode, "Board creation failed");

            _boardId = boardResponse.Data.Id;
        }

        [TestMethod]
        [DynamicData(nameof(ListNameCases), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(GetTestDisplayName))]
        public void CreateList_ShouldReturnValidList(string testCase, string listName)
        {
            var request = new RestRequest("lists", Method.Post);
            request.AddQueryParameter("name", listName);
            request.AddQueryParameter("idBoard", _boardId);
            request.AddQueryParameter("key", Config.Key);
            request.AddQueryParameter("token", Config.Token);

            var response = _client.SendRequest<ListModel>(request);

            Assert.AreEqual(200, (int)response.StatusCode, "List creation failed");
            Assert.IsNotNull(response.Data.Id, "List ID is null");
            Assert.AreEqual(listName, response.Data.Name, "List name mismatch");
            Assert.AreEqual(_boardId, response.Data.IdBoard, "Board ID mismatch");
        }

        public static IEnumerable<object[]> ListNameCases()
        {
            yield return new object[] { "Short name", "L" };
            yield return new object[] { "Long name", "ThisIsAVeryLongListNameForTestingPurposesThisIsAVeryLongListNameForTestingPurposesThisIsAVeryLongListNameForTestingPurposes" };
            yield return new object[] { "Lowercase", "testlist" };
            yield return new object[] { "Uppercase", "TESTLIST" };
            yield return new object[] { "Mixed case", "TestLIST" };
            yield return new object[] { "Numeric only", "123456" };
            yield return new object[] { "Symbols only", "!@#$%^&*()" };
            yield return new object[] { "Cyrillic letters", "СписъкТест" };
            yield return new object[] { "Mixed chars", "List_№1" };
        }

        [TestMethod]
        public void CreateList_WithEmptyName_ShouldFail()
        {
            var request = new RestRequest("lists", Method.Post);
            request.AddQueryParameter("name", "");
            request.AddQueryParameter("idBoard", _boardId);
            request.AddQueryParameter("key", Config.Key);
            request.AddQueryParameter("token", Config.Token);

            var response = _client.SendRequest<ListModel>(request);

            Assert.AreNotEqual(200, (int)response.StatusCode, "Expected failure for empty name");
            Assert.IsNull(response.Data?.Id, "List ID should be null for empty name");
            Assert.IsTrue(response.Content?.Equals("invalid value for name"), "Expected error message for invalid name");
        }

        [TestMethod]
        public void CreateList_WithNullName_ShouldFail()
        {
            var request = new RestRequest("lists", Method.Post);
            request.AddQueryParameter("name", null);
            request.AddQueryParameter("idBoard", _boardId);
            request.AddQueryParameter("key", Config.Key);
            request.AddQueryParameter("token", Config.Token);

            var response = _client.SendRequest<ListModel>(request);

            Assert.AreNotEqual(200, (int)response.StatusCode, "Expected failure for null name");
            Assert.IsNull(response.Data?.Id, "List ID should be null for null name");
            Assert.IsTrue(response.Content?.Equals("invalid value for name"), "Expected error message for null name");
        }



        [TestCleanup]
        public void CleanupBoard()
        {
            if (!string.IsNullOrEmpty(_boardId))
            {
                var deleteRequest = new RestRequest($"boards/{_boardId}", Method.Delete);
                deleteRequest.AddQueryParameter("key", Config.Key);
                deleteRequest.AddQueryParameter("token", Config.Token);

                _client.SendRequest(deleteRequest);
            }
        }

        public static string GetTestDisplayName(MethodInfo methodInfo, object[] values)
        {
            var name = (string)values.First();

            return $"{methodInfo.Name}(Case: {name})";
        }
    }
}

