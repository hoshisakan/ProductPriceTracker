using Quartz;
using System.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace ProductPriceTracker.Infrastructure.Services.Jobs
{
    public class SqlServerBackupJob : IJob
    {
        private readonly ILogger<SqlServerBackupJob> _logger;

        public SqlServerBackupJob(ILogger<SqlServerBackupJob> logger)
        {
            _logger = logger;
        }

        [Obsolete]
        public async Task Execute(IJobExecutionContext context)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            var backupDir = Environment.GetEnvironmentVariable("MSSQL_BACKUP_PATH");
            var server = Environment.GetEnvironmentVariable("MSSQL_HOST_IP");
            var user = Environment.GetEnvironmentVariable("MSSQL_SA_USER");
            var password = Environment.GetEnvironmentVariable("MSSQL_SA_PASSWORD");
            var database = Environment.GetEnvironmentVariable("MSSQL_DATABASE");

            var backupFile = Path.Combine(backupDir, $"{database}_{timestamp}.bak");

            var connectionString = $"Server={server};Database=master;User Id={user};Password={password};TrustServerCertificate=True;";

            var backupSql = $@"
            BACKUP DATABASE [{database}]
            TO DISK = N'{backupFile}'
            WITH FORMAT, INIT, NAME = N'Quartz Backup {database}';";

            try
            {
                Directory.CreateDirectory(backupDir);

                using var connection = new SqlConnection(connectionString);
                using var command = new SqlCommand(backupSql, connection);
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                _logger.LogInformation($"✅ MSSQL 備份成功: {backupFile}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ MSSQL 備份失敗: {ex.Message}");
            }
        }
    }
}
