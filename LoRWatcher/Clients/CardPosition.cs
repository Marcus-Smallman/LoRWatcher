namespace LoRWatcher.Clients
{
    public class CardPosition
    {
        public long CardId { get; set; }

        public string CardCode { get; set; }

        public int TopLeftX { get; set; }

        public int TopLeftY { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public bool LocalPlayer { get; set; }
    }
}
