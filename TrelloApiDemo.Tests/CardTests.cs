using RestSharp;
using System.Reflection;
using TrelloApiDemo.Helpers;
using TrelloApiDemo.Models;

namespace TrelloApiDemo.Tests
{
    [TestClass]
    public class CardTests
    {
        private TrelloClient? _client;
        private string? _boardId;
        private string? _listId;

        [TestInitialize]
        public void Setup()
        {
            var config = TestConfig.LoadConfiguration();
            Config.Key = config["Trello:Key"] ?? throw new InvalidOperationException("Trello:Key is missing in configuration.");
            Config.Token = config["Trello:Token"] ?? throw new InvalidOperationException("Trello:Token is missing in configuration.");
            Config.BaseUrl = config["Trello:BaseUrl"] ?? throw new InvalidOperationException("Trello:BaseUrl is missing in configuration.");

            _client = new TrelloClient();

            _boardId = _client.CreateBoard("CardTestBoard_" + Guid.NewGuid()).Data?.Id ?? throw new InvalidOperationException("Board creation failed");

            _listId = _client.CreateList("CardTestList", _boardId).Data?.Id ?? throw new InvalidOperationException("List creation failed");
        }

        [TestMethod]
        [DynamicData(nameof(CardNameCases), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(GetTestDisplayName))]
        public void CreateCard_ShouldReturnValidCard(string testCase, string cardName)
        {
            Assert.IsNotNull(_client, "_client is not initialized.");

            var response = _client.CreateCard(cardName, _listId ?? throw new ArgumentNullException(nameof(_listId)));

            Assert.AreEqual(200, (int)response.StatusCode, "Card creation failed");
            Assert.IsNotNull(response.Data, "Card response data is null");
            Assert.IsNotNull(response.Data?.Id, "Card ID is null");
            Assert.AreEqual(cardName, response.Data?.Name, "Card name mismatch");
            Assert.AreEqual(_listId, response.Data?.IdList, "List ID mismatch");
        }

        public static IEnumerable<object[]> CardNameCases()
        {
            yield return new object[] { "Short name", "C" };
            yield return new object[] { "Long name", "ThisIsAVeryLongCardNameThatExceedsNormalLengthhisIsAVeryLongCardNameThatExceedsNormalLengthhisIsAVeryLongCardNameThatExceedsNormalLengthhisIsAVeryLongCardNameThatExceedsNormalLength" };
            yield return new object[] { "Lowercase name", "testcard" };
            yield return new object[] { "Uppercase name", "TESTCARD" };
            yield return new object[] { "Mixed case name", "TESTcard" };
            yield return new object[] { "Numeric name", "1234567890" };
            yield return new object[] { "Symbol-only name", "!@#$%^&*()" };
            yield return new object[] { "Cyrillic name", "ТестоваКарта" };
            yield return new object[] { "Mixed characters name", "Card_№0123%" };
            yield return new object[] { "Empty name", "" };
        }

        [TestMethod]
        public void CreateCard_WithNullName_ShouldReturnValidCardWithEmtyName()
        {
            Assert.IsNotNull(_client, "_client is not initialized.");

            var response = _client.CreateCard(null, _listId ?? throw new ArgumentNullException(nameof(_listId)));
            Console.WriteLine("Response: " + response);
            Console.WriteLine("Data: " + response.Data);
            Console.WriteLine("Response: " + response.Content);

            Assert.AreEqual(200, (int)response.StatusCode, "Card creation failed");
            Assert.IsNotNull(response.Data, "Card response data is null");
            Assert.IsNotNull(response.Data?.Id, "Card ID is null");
            Assert.AreEqual("", response.Data?.Name, "Card name mismatch");
            Assert.AreEqual(_listId, response.Data?.IdList, "List ID mismatch");
        }

        [TestMethod]
        public void CreateCard_WithDescriptionAndDueDate_ShouldReturnValidCard()
        {
            Assert.IsNotNull(_client, "_client is not initialized.");

            var dueDate = DateTime.UtcNow.AddDays(2).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            var response = _client.CreateCard("CardWithDueDate", _listId!, "This is a test card", dueDate);
            var card = _client.Deserialize<Card>(response);

            var expectedDueDate = DateTime.Parse(dueDate);
            var actualDueDate = DateTime.Parse(card?.DueDate ?? throw new InvalidOperationException("Due date missing"));

            Assert.AreEqual(200, (int)response.StatusCode);
            Assert.AreEqual("CardWithDueDate", response.Data?.Name);
            Assert.AreEqual(expectedDueDate, actualDueDate, "Due date mismatch");
            Assert.AreEqual("This is a test card", card?.Description, "Description mismatch");
            Assert.IsTrue(card?.Badges?.HasDescription == true, "Badges does not report description");
            Assert.AreEqual(card.DueDate, card.Badges?.Due, "Badges due mismatch");
        }

        [TestMethod]
        public void UpdateCard_ShouldModifyNameDescriptionAndDueDate()
        {
            Assert.IsNotNull(_client, "_client is not initialized.");

            var originalDueDate = DateTime.UtcNow.AddDays(2).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            var createResponse = _client.CreateCard("OriginalName", _listId!, "Original description", originalDueDate);
            var card = _client.Deserialize<Card>(createResponse);
            var cardId = card?.Id ?? throw new InvalidOperationException("Card ID is missing");

            var newName = "UpdatedName";
            var newDescription = "Updated description";
            var newDueDate = DateTime.UtcNow.AddDays(5).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            var updateResponse = _client.UpdateCard(cardId, newName, newDescription, newDueDate);
            var updatedCard = _client.Deserialize<Card>(updateResponse);

            Assert.AreEqual(200, (int)updateResponse.StatusCode);
            Assert.AreEqual(newName, updatedCard?.Name, "Name was not updated");
            Assert.AreEqual(newDescription, updatedCard?.Description, "Description was not updated");

            var expectedDue = DateTime.Parse(newDueDate);
            var actualDue = DateTime.Parse(updatedCard?.DueDate ?? throw new InvalidOperationException("Due date missing"));
            Assert.AreEqual(expectedDue, actualDue, "Due date was not updated");
        }

        [TestMethod]
        public void UpdateCard_ShouldModifyOnlyName()
        {
            Assert.IsNotNull(_client, "_client is not initialized.");

            var createResponse = _client.CreateCard("InitialName", _listId!, "Initial description");
            var card = _client.Deserialize<Card>(createResponse);
            var cardId = card?.Id ?? throw new InvalidOperationException("Card ID is missing");

            var newName = "UpdatedName";
            var updateResponse = _client.PartialCardUpdate(cardId, newName);
            var updatedCard = _client.Deserialize<Card>(updateResponse);

            Assert.AreEqual(200, (int)updateResponse.StatusCode);
            Assert.AreEqual(newName, updatedCard?.Name, "Name was not updated");
            Assert.AreEqual("Initial description", updatedCard?.Description, "Description should remain unchanged");
        }

        [TestMethod]
        public void UpdateCard_ShouldModifyOnlyDescription()
        {
            Assert.IsNotNull(_client, "_client is not initialized.");

            var createResponse = _client.CreateCard("InitialName", _listId!, "Initial description");
            var card = _client.Deserialize<Card>(createResponse);
            var cardId = card?.Id ?? throw new InvalidOperationException("Card ID is missing");

            var newDescription = "Updated description";
            var updateResponse = _client.PartialCardUpdate(cardId, null, newDescription);
            var updatedCard = _client.Deserialize<Card>(updateResponse);

            Assert.AreEqual(200, (int)updateResponse.StatusCode);
            Assert.AreEqual("InitialName", updatedCard?.Name, "Name should remain unchanged");
            Assert.AreEqual(newDescription, updatedCard?.Description, "Description was not updated");
        }

        [TestMethod]
        public void UpdateCard_WhenDescriptionIsSetToEmpty_ShouldClearDescription()
        {
            Assert.IsNotNull(_client, "_client is not initialized.");

            var initialName = "CardToClearDesc";
            var createResponse = _client.CreateCard(initialName, _listId!, "Temporary description");
            var card = _client.Deserialize<Card>(createResponse);
            var cardId = card?.Id ?? throw new InvalidOperationException("Card ID is missing");

            var updateResponse = _client.PartialCardUpdate(cardId, null, "");
            var updatedCard = _client.Deserialize<Card>(updateResponse);

            Assert.AreEqual(200, (int)updateResponse.StatusCode);
            Assert.AreEqual(initialName, updatedCard?.Name, "Name should remain unchanged");
            Assert.AreEqual("", updatedCard?.Description, "Description should be cleared");
            Assert.IsFalse(updatedCard?.Badges?.HasDescription, "Badges should report no description");
        }

        [TestMethod]
        public void UpdateCard_WhenDueDateIsSetToEmpty_ShouldClearDueDate()
        {
            Assert.IsNotNull(_client, "_client is not initialized.");

            var initialName = "CardToClearDueDate";
            var initialDescription = "Card with due date";
            var initialDueDate = DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            var createResponse = _client.CreateCard(initialName, _listId!, initialDescription, initialDueDate);
            var card = _client.Deserialize<Card>(createResponse);
            var cardId = card?.Id ?? throw new InvalidOperationException("Card ID is missing");

            var updateResponse = _client.PartialCardUpdate(cardId, null, null, "");
            var updatedCard = _client.Deserialize<Card>(updateResponse);

            Assert.AreEqual(200, (int)updateResponse.StatusCode);
            Assert.AreEqual(initialName, updatedCard?.Name, "Name should remain unchanged");
            Assert.AreEqual(initialDescription, updatedCard?.Description, "Description should remain unchanged");
            Assert.IsNull(updatedCard?.DueDate, "Due date should be cleared");
            Assert.IsNull(updatedCard?.Badges?.Due, "Badges should report no due date");
        }

        [TestMethod]
        public void DeleteCard_ShouldRemoveCardSuccessfully()
        {
            Assert.IsNotNull(_client, "_client is not initialized.");

            var createResponse = _client.CreateCard("CardToDelete", _listId!, "This card will be deleted");
            var card = _client.Deserialize<Card>(createResponse);
            var cardId = card?.Id ?? throw new InvalidOperationException("Card ID is missing");

            var deleteResponse = _client.DeleteCard(cardId);
            Assert.AreEqual(200, (int)deleteResponse.StatusCode, "Card deletion failed");

            var getRequest = new RestRequest($"cards/{cardId}", Method.Get);
            getRequest.AddQueryParameter("key", Config.Key);
            getRequest.AddQueryParameter("token", Config.Token);

            var getResponse = _client.SendRequest<Card>(getRequest);

            Assert.AreEqual(404, (int)getResponse.StatusCode, "Card still exists after deletion");
        }

        [TestCleanup]
        public void CleanupBoard()
        {
            if (!string.IsNullOrEmpty(_boardId))
            {
                var deleteRequest = new RestRequest($"boards/{_boardId}", Method.Delete);
                deleteRequest.AddQueryParameter("key", Config.Key);
                deleteRequest.AddQueryParameter("token", Config.Token);

                _client?.SendRequest(deleteRequest);
            }
        }

        public static string GetTestDisplayName(MethodInfo methodInfo, object[] values)
        {
            var name = (string)values.First();

            return $"{methodInfo.Name}(Case: {name})";
        }
    }
}
