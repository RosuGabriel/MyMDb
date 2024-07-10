using System.Diagnostics;

namespace MyMDb.Helpers
{
    public class TerminalExecutor
    {
        public static async Task<string> VideoConvertorAsync(string inputPath, string outputPath)
        {
            var processStartInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = $"/C ffmpeg -i {inputPath} -codec copy {outputPath}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = processStartInfo })
            {
                process.Start();

                // Citește ieșirile și erorile asincron
                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();

                // Așteaptă finalizarea procesului
                await process.WaitForExitAsync();

                var output = await outputTask;
                var error = await errorTask;

                // Verifică codul de ieșire
                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException($"Command failed with exit code {process.ExitCode}: {error}");
                }

                return output;
            }
        }
    }
}
