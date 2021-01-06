using Microsoft.Azure.Cosmos.Table;

namespace WorkerMonitoramentoSites.Entities
{
    public class MonitoramentoEntity : TableEntity
    {
        public MonitoramentoEntity(string status, string horario)
        {
            PartitionKey = status;
            RowKey = horario;
        }

        public MonitoramentoEntity() { }

        public string Host { get; set; }
        public string Local { get; set; }
        public string Erro { get; set; }
    }
}