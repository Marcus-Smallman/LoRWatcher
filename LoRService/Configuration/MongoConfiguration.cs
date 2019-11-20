namespace LoRService.Configuration
{
    public class MongoConfiguration
    {
        public string Address { get; set; }

        public int Port { get; set; }

        public static string DatabaseName = "LoRService";
    }
}
