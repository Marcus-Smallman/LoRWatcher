namespace LoRService.Models
{
    public class MatchReport
    {
        public string Id { get; set; }

        public string PlayerName { get; set; }

        public string PlayerDeckCode { get; set; }

        public string OpponentName { get; set; }

        public bool Result { get; set; }
    }
}
