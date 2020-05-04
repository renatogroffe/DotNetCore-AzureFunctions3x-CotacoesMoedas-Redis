using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ServerlessMoedas
{
    public static class CotacoesHttpTrigger
    {
        private static ConnectionMultiplexer _CONEXAO_REDIS =
            ConnectionMultiplexer.Connect(
                Environment.GetEnvironmentVariable("BaseCotacoes"));

        [FunctionName("CotacoesHttpTrigger")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            string moeda = req.Query["moeda"];
            log.LogInformation($"CotacoesHttpTrigger: {moeda}");

            if (!String.IsNullOrWhiteSpace(moeda))
            {
                var resultado = _CONEXAO_REDIS.GetDatabase()
                        .StringGet($"Moeda-{moeda.ToUpper()}");
                return new ContentResult
                {
                    Content = resultado.HasValue ? resultado.ToString() : "{}",
                    ContentType = "application/json"
                };
            }
            else
            {
                return new BadRequestObjectResult(new
                {
                    Sucesso = false,
                    Mensagem = "Informe uma sigla de moeda válida"
                });
            }
        }
    }
}