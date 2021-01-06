using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WorkerMonitoramentoSites.Configuration;
using WorkerMonitoramentoSites.Data;
using WorkerMonitoramentoSites.Models;

namespace WorkerMonitoramentoSites
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ParametrosExecucao _parametrosExecucao;
        private readonly MonitoramentoRepository _repository;
        private readonly JsonSerializerOptions _jsonOptions;

        public Worker(ILogger<Worker> logger,
            ParametrosExecucao parametrosExecucao,
            MonitoramentoRepository repository)
        {
            _logger = logger;
            _parametrosExecucao = parametrosExecucao;
            _repository = repository;

            _jsonOptions = new () { IgnoreNullValues = true };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (string host in _parametrosExecucao.Hosts.Split('|'))
                {
                    var resultado = new ResultadoMonitoramento();
                    resultado.Host = host;
                    resultado.Horario =
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(host);
                        client.DefaultRequestHeaders.Accept.Clear();

                        try
                        {
                            var response = await client.GetAsync("");

                            resultado.Status = (int)response.StatusCode + " " +
                                response.StatusCode;
                            if (response.StatusCode != HttpStatusCode.OK)
                                resultado.Exception = response.ReasonPhrase;
                        }
                        catch (Exception ex)
                        {
                            resultado.Status = "Exception";
                            resultado.Exception = ex.Message;
                        }
                    }

                    await _repository.Insert(resultado);
                    
                    string jsonResultado =
                        JsonSerializer.Serialize(resultado, _jsonOptions);

                    if (resultado.Exception == null)
                        _logger.LogInformation(jsonResultado);
                    else
                        _logger.LogError(jsonResultado);
                }

                _logger.LogInformation(
                    $"{Environment.MachineName} - Worker executando em: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                await Task.Delay(_parametrosExecucao.Intervalo, stoppingToken);
            }
        }
    }
}