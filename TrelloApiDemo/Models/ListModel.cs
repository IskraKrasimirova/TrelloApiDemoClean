using Newtonsoft.Json;

namespace TrelloApiDemo.Models
{
    public class ListModel
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("idBoard")]
        public string? IdBoard { get; set; }
    }
}
