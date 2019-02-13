using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Serilog;

namespace VisualStudioStarter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var localAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VisualStudioStarter");
            if (!Directory.Exists(localAppDataPath)) Directory.CreateDirectory(localAppDataPath);
            var logFile = Path.Combine(localAppDataPath, "VisualStudioStarter-{Date}.log");

            var logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.RollingFile(logFile).CreateLogger();

            if (args == null || args.Length < 1)
            {
                var provider = new PhysicalFileProvider(Directory.GetCurrentDirectory());
                var slnFile = provider.GetDirectoryContents(string.Empty).FirstOrDefault(p => StringComparer.InvariantCultureIgnoreCase.Equals(Path.GetExtension(p.Name), ".sln"));
                if (slnFile == null)
                {
                    logger.Warning($@"Zu wenige Parameter übergeben, keine Projektmappe in ""{provider.Root}"" gefunden.");
                    return;
                }

                args = new[] {$@".\{slnFile.Name}"};
            }
            else if (args.Length != 1)
            {
                logger.Warning("Zu viele Parameter übergeben.");
                return;
            }


            var cfg = new ConfigurationBuilder().SetBasePath(localAppDataPath).AddJsonFile("appsettings.json", false, true).Build();
            var options = cfg.Get<VisualStudioOptions>();


            try
            {
                var result = options.GetExecutable(args.First());
                logger.Debug($"{args.First()} wird gestartet mit {result}.");

                var startInfo = new ProcessStartInfo
                {
                    FileName = result,
                    Arguments = args.First(),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var processTemp = new Process {StartInfo = startInfo, EnableRaisingEvents = true};

                processTemp.Start();
            }
            catch(InvalidOperationException ex)
            {
                logger.Warning(ex.Message);
            }

        }
    }
}
