// <copyright file="ServerShell.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

using TestFtpServer.FtpServerShell;

namespace TestFtpServer
{
    internal class ServerShell
    {
        [NotNull]
        private readonly IShellStatus _status;

        [NotNull]
        private readonly FtpShellCommandAutoCompletion _autoCompletionHandler;

        [NotNull]
        private readonly ILogger<FtpServer> _logger;

        public ServerShell(
            [NotNull] IShellStatus status,
            [NotNull] FtpShellCommandAutoCompletion autoCompletionHandler,
            [NotNull] ILogger<FtpServer> logger)
        {
            _status = status;
            _autoCompletionHandler = autoCompletionHandler;
            _logger = logger;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Enter \"exit\" to close the test application.");
            Console.WriteLine("Use \"help\" for more information.");

            ReadLine.AutoCompletionHandler = _autoCompletionHandler;
            ReadLine.HistoryEnabled = true;

            while (!_status.Closed)
            {
                var readTask = Task.Run(
                    () =>
                    {
                        var input = ReadLine.Read("> ");
                        return Task.FromResult(input);
                    });
                var waitTask = await Task.WhenAny(readTask, Task.Delay(-1, cancellationToken))
                   .ConfigureAwait(false);

                if (waitTask != readTask)
                {
                    continue;
                }

                var command = await readTask.ConfigureAwait(false);
                if (command == null)
                {
                    continue;
                }

                var handler = _autoCompletionHandler.GetCommand(command);
                if (handler == null)
                {
                    if (string.IsNullOrEmpty(command))
                    {
                        Console.WriteLine("Use \"help\" for more information.");
                    }
                    else
                    {
                        Console.Error.WriteLine("Cannot execute the command. Handler not found.");
                    }
                }
                else
                {
                    try
                    {
                        await handler.ExecuteAsync(CancellationToken.None)
                           .ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, ex.Message);
                    }
                }
            }
        }
    }
}