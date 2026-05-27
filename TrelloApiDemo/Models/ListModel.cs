using Newtonsoft.Json;

namespace TrelloApiDemo.Models
{
    public class ListModel
    {
        [JsonProperty("id")]
        public string Id { get; set; } = default!;

        [JsonProperty("name")]
        public string Name { get; set; } = default!;

        [JsonProperty("idBoard")]
        public string IdBoard { get; set; }= default!;
    }
}
