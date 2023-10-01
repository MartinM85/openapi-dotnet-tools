using OpenApi.Tools.Core;
using System;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace OpenApi.Tools
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            Console.WriteLine($"Open API tools, version={version}");
            var tools = new OpenApiTools();
            var abortTokenSource = new CancellationTokenSource();
            var abortToken = abortTokenSource.Token;
            try
            {
                var cmdExecutor = new ToolsCommandManager(tools, abortToken);
                var rootCmd = cmdExecutor.CreateRootCommand();
                if (args != null && args.Length > 0)
                {
                    await rootCmd.InvokeAsync(args);
                }
                else
                {
                    var consoleJob = Task.Run(async () =>
                    {
                        using var inputStream = Console.OpenStandardInput();
                        using var sr = new StreamReader(inputStream);
                        var abortTask = Task.Delay(Timeout.Infinite, abortToken);
                        Task<int> commandTask = null;
                        while (true)
                        {
                            if (abortToken.IsCancellationRequested) { break; }

                            var readTask = sr.ReadLineAsync();
                            var tasks = new[] { readTask, abortTask };
                            if (commandTask != null)
                            {
                                tasks = tasks.Append(commandTask).ToArray();
                            }
                            var someTask = await Task.WhenAny(tasks);
                            if (abortToken.IsCancellationRequested) { break; }
                            if (someTask == commandTask)
                            {
                                var cmdResult = await commandTask;
                                commandTask.Dispose();
                            }
                            else if (someTask != readTask) { continue; }

                            // process console input
                            var consoleInput = string.Empty;
                            try { consoleInput = await readTask; }
                            catch { }
                            finally { readTask.Dispose(); }
                            consoleInput = consoleInput?.Trim();

                            if (!string.IsNullOrEmpty(consoleInput))
                            {
                                if (string.Compare(consoleInput, "exit", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    abortTokenSource.Cancel();
                                    continue;
                                }

                                try
                                {                                
                                    commandTask = rootCmd.InvokeAsync(consoleInput);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                }
                            }
                        }

                        // finalize helper task
                        try { await abortTask; }
                        catch (TaskCanceledException) { /* consuming expected exception */ }
                        finally { abortTask.Dispose(); }

                        // finalize command task
                        if (commandTask != null)
                        {
                            try { await commandTask; }
                            catch (TaskCanceledException) { }
                            finally { commandTask.Dispose(); }
                        }
                    });

                    await consoleJob;
                }
            }
            finally
            {
                abortTokenSource.Dispose();
            }
        }
    }
}