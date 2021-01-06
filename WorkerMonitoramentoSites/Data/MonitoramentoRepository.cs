using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos.Table;
using WorkerMonitoramentoSites.Configuration;
using WorkerMonitoramentoSites.Models;
using WorkerMonitoramentoSites.Entities;

namespace WorkerMonitoramentoSites.Data
{
    public class MonitoramentoRepository
    {
        private readonly ILogger<MonitoramentoRepository> _logger;
        private readonly CloudTable _monitoramentoTable;

        public MonitoramentoRepository(ILogger<MonitoramentoRepository> logger,
            ParametrosExecucao parametrosExecucao)
        {
            _logger = logger;

            var storageAccount = CloudStorageAccount
                .Parse(parametrosExecucao.ConnectionStringStorage);
            _monitoramentoTable = storageAccount
                .CreateCloudTableClient().GetTableReference("MonitoramentoSites");
            if (_monitoramentoTable.CreateIfNotExistsAsync().Result)
                _logger.LogInformation("Criada a tabela Monitoramento...");
        }

        public async Task Insert(ResultadoMonitoramento resultado)
        {
            MonitoramentoEntity dadosMonitoramento =
                new MonitoramentoEntity(
                    resultado.Status, resultado.Horario);
            dadosMonitoramento.Host = resultado.Host;
            dadosMonitoramento.Local = Environment.MachineName;
            dadosMonitoramento.Erro = resultado.Exception;
            
            var insertOperation = TableOperation.Insert(dadosMonitoramento);
            await _monitoramentoTable.ExecuteAsync(insertOperation);
        }
    }
}