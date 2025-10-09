using Newtonsoft.Json;

namespace TrelloApiDemo.Models
{
    public class Board
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("desc")]
        public string? Desc { get; set; }
    }
}
