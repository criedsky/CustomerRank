using CustomerRankWebApi.Controllers;
using CustomerRankWebApi.Model;
using Microsoft.AspNetCore.SignalR;

namespace CustomerRankWebApi.SignalR
{
    public interface ICustomerHubClient
    {
        Task SendLeaderboardToUser(IEnumerable<Customer> customers);
    }

    public class CustomerHub : Hub<ICustomerHubClient>
    {
        private readonly ILogger<CustomerHub> _logger;
        public CustomerHub(ILogger<CustomerHub> logger)
        {
            this._logger = logger;   
        }
        
        public override async Task OnConnectedAsync()
        {
            var clientId = Context.ConnectionId;
            this._logger.LogInformation($"Client:{clientId} is connected....");
            await base.OnConnectedAsync();
        }
    }
    
}
