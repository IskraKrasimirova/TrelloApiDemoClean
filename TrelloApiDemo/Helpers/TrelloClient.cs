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
            return _client.Execute<T>(request);
        }

        public RestResponse SendRequest(RestRequest request)
        {
            return _client.Execute(request);
        }

        public RestResponse<Board> CreateBoard(string name)
        {
            var request = new RestRequest("boards", Method.Post);
            request.AddQueryParameter("name", name);
            request.AddQueryParameter("key", Config.Key);
            request.AddQueryParameter("token", Config.Token);
            request.AddQueryParameter("desc", "Created by automated test");

            return _client.Execute<Board>(request);
        }
    }
}
