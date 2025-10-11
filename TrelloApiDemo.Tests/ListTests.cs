using RestSharp;
using System.Reflection;
using TrelloApiDemo.Helpers;

namespace TrelloApiDemo.Tests
{
    [TestClass]
    public class ListTests
    {
        private TrelloClient? _client;
        private string? _boardId;

        [TestInitialize]
        public async Task Setup()
        {
            var configuration = TestConfig.LoadConfiguration();

            Config.Key = configuration["Trello:Key"] ?? throw new InvalidOperationException("Trello:Key is missing in configuration.");
            Config.Token = configuration["Trello:Token"] ?? throw new InvalidOperationException("Trello:Token is missing in configuration.");

            _client = new TrelloClient();

            // Create board for list tests
            var response = await _client.CreateBoardAsync("ListTestBoard_" + Guid.NewGuid());
            _boardId = response.Data?.Id ?? throw new InvalidOperationException("Board ID could not be retrieved.");
        }

        [TestMethod]
        [DynamicData(nameof(ListNameCases), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(GetTestDisplayName))]
        public async Task CreateList_ShouldReturnValidListAsync(string testCase, string listName)
        {
            Assert.IsNotNull(_client, "_client is not initialized.");

            var response = await _client.CreateListAsync(listName, _boardId ?? throw new ArgumentNullException(nameof(_boardId)));

            Assert.AreEqual(200, (int)response.StatusCode, "List creation failed");
            Assert.IsNotNull(response.Data, "List response data is null");
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
        public async Task CreateList_WithEmptyName_ShouldFailAsync()
        {
            Assert.IsNotNull(_client, "_client is not initialized.");

            var response = await _client.CreateListAsync("", _boardId ?? throw new ArgumentNullException(nameof(_boardId)));
            
            Assert.AreNotEqual(200, (int)response.StatusCode, "Expected failure for empty name");
            Assert.AreEqual(400, (int)response.StatusCode, "BadRequest");
            Assert.IsNull(response.Data?.Id, "List ID should be null for empty name");
            Assert.IsTrue(response.Content.Equals("invalid value for name"), "Expected error message for invalid list name");
        }

        [TestMethod]
        public async Task CreateList_WithNullName_ShouldFailAsync()
        {
            Assert.IsNotNull(_client, "_client is not initialized.");

            var response = await _client.CreateListAsync(null, _boardId ?? throw new ArgumentNullException(nameof(_boardId)));

            Assert.AreNotEqual(200, (int)response.StatusCode, "Expected failure for null name");
            Assert.AreEqual(400, (int)response.StatusCode, "BadRequest");
            Assert.IsNull(response.Data?.Id, "List ID should be null for null name");
            Assert.IsTrue(response.Content.Equals("invalid value for name"), "Expected error message for null list name");
        }

        [TestCleanup]
        public async Task CleanupBoardAsync()
        {
            if (!string.IsNullOrEmpty(_boardId))
            {
                var deleteRequest = new RestRequest($"boards/{_boardId}", Method.Delete);
                deleteRequest.AddQueryParameter("key", Config.Key);
                deleteRequest.AddQueryParameter("token", Config.Token);

                await _client.SendRequestAsync(deleteRequest);
            }
        }

        public static string GetTestDisplayName(MethodInfo methodInfo, object[] values)
        {
            var name = (string)values.First();

            return $"{methodInfo.Name}(Case: {name})";
        }
    }
}
