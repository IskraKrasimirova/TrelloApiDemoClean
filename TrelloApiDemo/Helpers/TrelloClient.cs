using RestSharp;
using TrelloApiDemo.Models;

namespace TrelloApiDemo.Helpers
{
    public class TrelloClient
    {
        private readonly RestClient _client;

        public TrelloClient()
        {
            _client = new RestClient(Config.BaseUrl);
        }

        public RestResponse<T> SendRequest<T>(RestRequest request) where T : new()
        {
            var response = _client.Execute<T>(request);

            if (!response.IsSuccessful)
            {
                throw new ApplicationException($"Request failed: {response.StatusCode} - {response.Content}");
            }

            return response;
        }

        public RestResponse SendRequest(RestRequest request)
        {
            var response = _client.Execute(request);

            if (!response.IsSuccessful)
            {
                throw new ApplicationException($"Request failed: {response.StatusCode} - {response.Content}");
            }

            return response;
        }

        public RestResponse<Board> CreateBoard(string? name)
        {
            var request = new RestRequest("boards", Method.Post);
            request.AddQueryParameter("name", name);
            AddAuth(request);
            request.AddQueryParameter("desc", "Created by automated test");

            return _client.Execute<Board>(request);
        }

        public RestResponse<ListModel> CreateList(string? listName, string boardId)
        {
            var request = new RestRequest("lists", Method.Post);
            request.AddQueryParameter("name", listName);
            request.AddQueryParameter("idBoard", boardId);
            AddAuth(request);

            return _client.Execute<ListModel>(request);
        }

        private static void AddAuth(RestRequest request)
        {
            request.AddQueryParameter("key", Config.Key);
            request.AddQueryParameter("token", Config.Token);
        }
    }
}
