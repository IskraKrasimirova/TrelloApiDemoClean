using Newtonsoft.Json;

namespace TrelloApiDemo.Models
{
    public class Board
    {
        [JsonProperty("id")]
        public string Id { get; set; } = default!;

        [JsonProperty("name")]
        public string Name { get; set; } = default!;

        [JsonProperty("desc")]
        public string? Desc { get; set; }
    }
}
