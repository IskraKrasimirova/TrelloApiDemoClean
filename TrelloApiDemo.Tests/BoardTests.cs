using RestSharp;
using System.Reflection;
using TrelloApiDemo.Helpers;
using TrelloApiDemo.Models;

namespace TrelloApiDemo.Tests
{

    [TestClass]
    public class BoardTests
    {
        private TrelloClient? _client;

        [TestInitialize]
        public void Setup()
        {
            var configuration = TestConfig.LoadConfiguration();

            Config.Key = configuration["Trello:Key"] ?? throw new InvalidOperationException("Trello:Key is missing in configuration.");
            Config.Token = configuration["Trello:Token"] ?? throw new InvalidOperationException("Trello:Token is missing in configuration.");
            Config.BaseUrl = configuration["Trello:BaseUrl"] ?? throw new InvalidOperationException("Trello:BaseUrl is missing in configuration.");

            _client = new TrelloClient();
        }

        [TestMethod]
        [DynamicData(nameof(BoardNameCases), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(GetTestDisplayName))]
        public void CreateBoard_WithVariousNames_ShouldReturnValidBoard(string testCase, string boardName)
        {
            Assert.IsNotNull(_client, "_client is not initialized.");

            var response = _client.CreateBoard(boardName);

            Assert.AreEqual(200, (int)response.StatusCode, $"Failed for name: {boardName}");
            Assert.IsNotNull(response.Data?.Id, $"Board ID is null for name: {boardName}");
            Assert.AreEqual(boardName, response.Data?.Name, $"Name mismatch for: {boardName}");
        }

        public static IEnumerable<object[]> BoardNameCases()
        {
            yield return new object[] { "Short name", "A" };
            yield return new object[] { "Long name", "ThisIsAVeryLongBoardNameThatExceedsNormalLengthThisIsAVeryLongBoardNameThatExceedsNormalLengthThisIsAVeryLongBoardNameThatExceedsNormalLength" };
            yield return new object[] { "Lowercase name", "demoboard" };
            yield return new object[] { "Uppercase name", "DEMOBOARD" };
            yield return new object[] { "Mixed case name", "DEMOboard" };
            yield return new object[] { "Numeric name", "123456" };
            yield return new object[] { "Symbol-only name", "!@#$%^&*()" };
            yield return new object[] { "Cyrillic name", "ТестоваДъска" };
            yield return new object[] { "Mixed characters name", "Board_№1" };
        }

        [TestMethod]
        public void CreateAndGetBoard_ShouldReturnSameBoard()
        {
            Assert.IsNotNull(_client, "_client is not initialized.");

            var createResponse = _client.CreateBoard("Smoke_" + Guid.NewGuid());
            Assert.AreEqual(200, (int)createResponse.StatusCode);

            var boardId = createResponse.Data?.Id;
            Assert.IsNotNull(boardId, "Board ID is null after creation.");

            var getResponse = _client.SendRequest<Board>(new RestRequest($"boards/{boardId}", Method.Get)
                .AddQueryParameter("key", Config.Key)
                .AddQueryParameter("token", Config.Token));

            Assert.AreEqual(200, (int)getResponse.StatusCode);
            Assert.AreEqual(boardId, getResponse.Data?.Id);
        }

        [TestMethod]
        public void CreateBoard_WithEmptyName_ShouldFail()
        {
            Assert.IsNotNull(_client, "_client is not initialized.");

            var response = _client.CreateBoard("" ?? throw new ArgumentNullException());

            Assert.AreNotEqual(200, (int)response.StatusCode, "Expected failure for empty name");
            Assert.AreEqual(400, (int)response.StatusCode, "BadRequest");
            Assert.IsNull(response.Data?.Id, "Board ID should be null for empty name");
            Assert.IsTrue(response.Content?.Contains("invalid value for name"), "Expected error message for invalid board name");
        }

        [TestMethod]
        public void CreateBoard_WithNullName_ShouldFail()
        {
            Assert.IsNotNull(_client, "_client is not initialized.");

            var response = _client.CreateBoard(null);
            
            Assert.AreNotEqual(200, (int)response.StatusCode, "Expected failure for null name");
            Assert.AreEqual(400, (int)response.StatusCode, "BadRequest");
            Assert.IsNull(response.Data?.Id, "Board ID should be null for null name");
            Assert.IsTrue(response.Content?.Contains("invalid value for name"), "Expected error message for null board name");
        }

        [ClassCleanup]
        public static void CleanupBoards()
        {
            var client = new TrelloClient();

            var request = new RestRequest("members/me/boards", Method.Get);
            request.AddQueryParameter("key", Config.Key);
            request.AddQueryParameter("token", Config.Token);

            var response = client.SendRequest<List<Board>>(request);

            if (response.Data != null)
            {
                foreach (var board in response.Data)
                {
                    if (board.Desc == "Created by automated test")
                    {
                        var deleteRequest = new RestRequest($"boards/{board.Id}", Method.Delete);
                        deleteRequest.AddQueryParameter("key", Config.Key);
                        deleteRequest.AddQueryParameter("token", Config.Token);

                        client.SendRequest(deleteRequest);
                    }
                }
            }
        }

        public static string GetTestDisplayName(MethodInfo methodInfo, object[] values)
        {
            var name = (string)values.First();

            return $"{methodInfo.Name}(Case: {name})";
        }
    }
}
