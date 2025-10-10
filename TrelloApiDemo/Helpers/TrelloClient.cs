using Newtonsoft.Json;
using RestSharp;
using System.Net;
using TrelloApiDemo.Models;

namespace TrelloApiDemo.Helpers
{
    public class TrelloClient
    {
        private readonly RestClient _client;
        private static readonly object _rateLock = new();
        private static DateTime _lastRequestTime = DateTime.MinValue;
        private static readonly TimeSpan _minInterval = TimeSpan.FromMilliseconds(200);

        public TrelloClient()
        {
            _client = new RestClient(Config.BaseUrl);
        }

        public async Task<RestResponse<T>> SendRequestAsync<T>(RestRequest request) where T : new()
        {
            var response = await _client.ExecuteAsync<T>(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return response;
            }    

            if (!response.IsSuccessful)
            {
                throw new ApplicationException($"Request failed: {response.StatusCode} - {response.Content}");
            }

            return response;
        }

        public async Task<RestResponse> SendRequestAsync(RestRequest request)
        {
            var response = await _client.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                throw new ApplicationException($"Request failed: {response.StatusCode} - {response.Content}");
            }

            return response;
        }

        public T Deserialize<T>(RestResponse response)
        {
            return JsonConvert.DeserializeObject<T>(response.Content!)!;
        }

        public async Task<RestResponse<Board>> CreateBoardAsync(string? name)
        {
            var request = new RestRequest("boards", Method.Post);
            request.AddQueryParameter("name", name);
            AddAuth(request);
            request.AddQueryParameter("desc", "Created by automated test");

            return await _client.ExecuteAsync<Board>(request);
        }

        public async Task<RestResponse<ListModel>> CreateListAsync(string? listName, string boardId)
        {
            EnforceRateLimit();
            var request = new RestRequest("lists", Method.Post);
            request.AddQueryParameter("name", listName);
            request.AddQueryParameter("idBoard", boardId);
            AddAuth(request);

            return await _client.ExecuteAsync<ListModel>(request);
        }

        public async Task<RestResponse<Card>> CreateCardAsync(string? name, string listId, string? description = null, string? dueDate = null)
        {
            EnforceRateLimit();
            var request = new RestRequest("cards", Method.Post);
            request.AddQueryParameter("name", name);
            request.AddQueryParameter("idList", listId);
            request.AddQueryParameter("desc", description ?? string.Empty);
            if (!string.IsNullOrEmpty(dueDate))
                request.AddQueryParameter("due", dueDate);

            AddAuth(request);

            Console.WriteLine(request.Resource);
            foreach (var param in request.Parameters)
            {
                Console.WriteLine($"{param.Name} = {param.Value}");
            }

            return await _client.ExecuteAsync<Card>(request);
        }

        public async Task<RestResponse<Card>> UpdateCardAsync(string cardId, string? newName = null, string? newDescription = null, string? newDueDate = null)
        {
            if (string.IsNullOrEmpty(cardId))
                throw new ArgumentException("Card ID cannot be null or empty.", nameof(cardId));

            var request = new RestRequest($"cards/{cardId}", Method.Put);

            request.AddQueryParameter("name", newName);
            request.AddQueryParameter("desc", newDescription);
            request.AddQueryParameter("due", newDueDate);
            AddAuth(request);

            return await _client.ExecuteAsync<Card>(request);
        }

        public async Task<RestResponse<Card>> PartialCardUpdateAsync(string cardId, string? newName = null, string? newDescription = null, string? newDueDate = null)
        {
            if (string.IsNullOrEmpty(cardId))
                throw new ArgumentException("Card ID cannot be null or empty.", nameof(cardId));

            var request = new RestRequest($"cards/{cardId}", Method.Put);

            if (newName != null)
                request.AddQueryParameter("name", newName);
            if (newDescription != null)
                request.AddQueryParameter("desc", newDescription);
            if (newDueDate != null)
                request.AddQueryParameter("due", newDueDate);
          
            AddAuth(request);

            return await _client.ExecuteAsync<Card>(request);
        }

        public async Task<RestResponse> DeleteCardAsync(string cardId)
        {
            if (string.IsNullOrEmpty(cardId))
                throw new ArgumentException("Card ID cannot be null or empty.", nameof(cardId));

            var request = new RestRequest($"cards/{cardId}", Method.Delete);
            AddAuth(request);

            return await _client.ExecuteAsync(request);
        }

        private static void AddAuth(RestRequest request)
        {
            request.AddQueryParameter("key", Config.Key);
            request.AddQueryParameter("token", Config.Token);
        }

        private static void EnforceRateLimit()
        {
            lock (_rateLock)
            {
                var now = DateTime.UtcNow;
                var waitTime = _minInterval - (now - _lastRequestTime);

                if (waitTime > TimeSpan.Zero)
                    Thread.Sleep(waitTime);

                _lastRequestTime = DateTime.UtcNow;
            }
        }
    }
}
