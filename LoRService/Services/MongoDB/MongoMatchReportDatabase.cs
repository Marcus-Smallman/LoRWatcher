using LoRService.Configuration;
using LoRService.Models;
using LoRService.Services.MongoDB.Documents;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<(IEnumerable<MatchReport>, string)> GetPlayerMatchReportsAsync(string playerName)
        {
            var playerMatchReports = new List<MatchReport>();
            try
            {
                var matchCollection = this.mongoDatabase.GetCollection<MatchReportDocument>(CollectionName);

                var filter = Builders<MatchReportDocument>.Filter.Eq(m => m.PlayerName, playerName);

                var matchReportDocuments = await matchCollection.Find(filter).ToListAsync();
                foreach (var matchReportDocument in matchReportDocuments)
                {
                    playerMatchReports.Add(new MatchReport
                    {
                        Id = matchReportDocument.MatchId,
                        PlayerName = matchReportDocument.PlayerName,
                        PlayerDeckCode = matchReportDocument.PlayerDeckCode,
                        OpponentName = matchReportDocument.OpponentName,
                        Result = matchReportDocument.Result
                    });
                }
            }
            catch (Exception ex)
            {
                // Add logging

                return (null, ex.Message);
            }

            return (playerMatchReports, null);
        }

        public async Task<(bool, string)> ReportMatchAsync(MatchReport matchReport)
        {
            try
            {
                var matchCollection = this.mongoDatabase.GetCollection<MatchReportDocument>(CollectionName);

                var matchReportDoc = new MatchReportDocument
                {
                    MatchId = matchReport.Id,
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
