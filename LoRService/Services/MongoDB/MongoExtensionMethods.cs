using LoRService.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace LoRService.Services.MongoDB
{
    public static class MongoExtensionMethods
    {
        public static void AddMongo(this IServiceCollection services)
        {
            services.AddSingleton<MongoConfiguration>(sp => new MongoConfiguration
            {
                Address = "localhost",
                Port = 27017
            });

            services.AddSingleton<IMongoClient>(sp =>
            {
                var mongoConfiguration = sp.GetService<MongoConfiguration>();
                return new MongoClient(new MongoClientSettings
                {
                    Server = new MongoServerAddress(mongoConfiguration.Address, mongoConfiguration.Port)
                });
             });

            services.AddTransient<IMatchReportDatabase, MongoMatchReportDatabase>();
        }
    }
}
