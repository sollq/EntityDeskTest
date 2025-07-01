using Microsoft.Extensions.Logging;
using Serilog;

namespace EntityDesk.Infrastructure.Logging
{
    public static class SerilogConfig
    {
        public static void ConfigureSerilog()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }
    }
} 