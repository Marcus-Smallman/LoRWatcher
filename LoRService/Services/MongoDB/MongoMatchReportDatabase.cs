using LoRService.Configuration;
using LoRService.Models;
using LoRService.Services.MongoDB.Documents;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace LoRService.Services.MongoDB
{
    public class MongoMatchReportDatabase
        : IMatchReportDatabase
    {
        private const string CollectionName = "MatchReports";

        private readonly IMongoDatabase mongoDatabase;

        public MongoMatchReportDatabase(IMongoClient mongoClient)
        {
            this.mongoDatabase = mongoClient.GetDatabase(MongoConfiguration.DatabaseName);
        }

        public async Task<(bool, string)> ReportMatchAsync(MatchReport matchReport)
        {
            try
            {
                var matchCollection = this.mongoDatabase.GetCollection<MatchReportDocument>(CollectionName);

                var matchReportDoc = new MatchReportDocument
                {
                    Id = matchReport.Id,
                    PlayerName = matchReport.PlayerName,
                    PlayerDeckCode = matchReport.PlayerDeckCode,
                    OpponentName = matchReport.OpponentName,
                    Result = matchReport.Result
                };

                await matchCollection.InsertOneAsync(matchReportDoc);
            }
            catch (Exception ex)
            {
                // Add logging

                return (false, ex.Message);
            }

            return (true, null);
        }
    }
}
