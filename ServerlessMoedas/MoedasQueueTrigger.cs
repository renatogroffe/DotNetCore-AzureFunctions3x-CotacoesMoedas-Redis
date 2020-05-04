using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ServerlessMoedas.Models;
using StackExchange.Redis;

namespace ServerlessMoedas
{
    public static class MoedasQueueTrigger
    {
        private static ConnectionMultiplexer _CONEXAO_REDIS =
            ConnectionMultiplexer.Connect(
                Environment.GetEnvironmentVariable("BaseCotacoes"));

        [FunctionName("MoedasQueueTrigger")]
        public static void Run([QueueTrigger("queue-cotacoes", Connection = "AzureWebJobsStorage")]string myQueueItem, ILogger log)
        {
            var cotacao =
                JsonSerializer.Deserialize<Cotacao>(myQueueItem);

            if (!String.IsNullOrWhiteSpace(cotacao.Sigla) &&
                cotacao.Valor.HasValue && cotacao.Valor > 0)
            {
                cotacao.UltimaCotacao = DateTime.Now;
                var dbRedis = _CONEXAO_REDIS.GetDatabase();
                dbRedis.StringSet(
                    "Moeda-" + cotacao.Sigla.ToUpper(),
                    JsonSerializer.Serialize(cotacao),
                    expiry: null);

                log.LogInformation($"MoedasQueueTrigger: {myQueueItem}");
            }
            else
                log.LogError($"MoedasQueueTrigger - Erro validação: {myQueueItem}");
        }
    }
}