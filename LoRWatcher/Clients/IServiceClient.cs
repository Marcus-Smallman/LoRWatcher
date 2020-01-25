﻿using LoRWatcher.Caches;
using System.Threading;
using System.Threading.Tasks;

namespace LoRWatcher.Clients
{
    public interface IServiceClient
    {
        Task<bool> ReportGameAsync(MatchReport matchReport, CancellationToken cancellationToken);
    }
}
