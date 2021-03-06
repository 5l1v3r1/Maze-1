using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Maze.Modules.Api.Request;
using Maze.Server.Connection.Commanding;
using Maze.Server.Library.Services;

namespace Maze.Server.Service
{
    public class CommandDistributor : ICommandDistributer
    {
        private readonly IConnectionManager _connectionManager;

        public CommandDistributor(IConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public Task Execute(MazeRequest request, CommandTargetCollection targets,
            CommandExecutionPolicy executionPolicy) =>
            throw new NotImplementedException();

        public async Task<HttpResponseMessage> Execute(HttpRequestMessage request, int clientId, int accountId, CancellationToken cancellationToken)
        {
            if (_connectionManager.ClientConnections.TryGetValue(clientId, out var clientConnection))
            {
                var response = await clientConnection.MazeServer.SendRequest(request, cancellationToken);
                if (response.StatusCode == HttpStatusCode.Created && response.Headers.Location?.Host == "channels")
                {
                    if (_connectionManager.AdministrationConnections.TryGetValue(accountId, out var administrationConnection))
                    {
                        var channelId = int.Parse(response.Headers.Location.AbsolutePath.Trim('/'));

                        clientConnection.MazeServer.AddChannelRedirect(channelId, administrationConnection.MazeServer.DataSocket);
                        administrationConnection.MazeServer.AddChannelRedirect(channelId, clientConnection.MazeServer.DataSocket);
                    }
                }
                return response;
            }

            throw new ClientNotFoundException(clientId);
        }
    }
}