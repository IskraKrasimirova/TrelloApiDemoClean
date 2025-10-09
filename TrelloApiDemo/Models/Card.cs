using Newtonsoft.Json;

namespace TrelloApiDemo.Models
{
    public class Card
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("idList")]
        public string? IdList { get; set; }

        [JsonProperty("idBoard")]
        public string? IdBoard { get; set; }

        [JsonProperty("desc")]
        public string? Description { get; set; }

        [JsonProperty("due")]
        public string? DueDate { get; set; }

        [JsonProperty("closed")]
        public bool? IsClosed { get; set; }

        [JsonProperty("badges")]
        public Badges? Badges { get; set; }
    }

    public class Badges
    {
        [JsonProperty("description")]
        public bool? HasDescription { get; set; }

        [JsonProperty("due")]
        public string? Due { get; set; }
    }
}
