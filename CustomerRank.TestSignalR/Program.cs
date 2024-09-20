using Microsoft.AspNetCore.SignalR.Client;
//here is SignalR Sender URL  
string hubUrl = "http://localhost:9001/leaderboard/realtime";
var hubConnection = new HubConnectionBuilder()
    .WithUrl(hubUrl)
    .Build();

// Register a handler for messages from the SignalR hub  
// "ReceiveStockPrice" is the topic to which SignalR sending the singnals  
hubConnection.On<object>("SendLeaderboardToUser", (result) =>
{
    Console.WriteLine(result);
});

try
{
    // Start the connection  
    hubConnection.StartAsync().Wait();
    Console.WriteLine("SignalR connection started.");
}
catch (Exception ex)
{
    Console.WriteLine($"Error connecting to SignalR: {ex.Message}");
    throw;
}
//Create a cancellation token to stop the connection  
CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
//hubConnection.StopAsync().Wait();  
var cancellationToken = cancellationTokenSource.Token;
// Handle Ctrl+C to gracefully shut down the application  
Console.CancelKeyPress += (sender, a) =>
{
    a.Cancel = true;
    Console.WriteLine("Stopping SignalR connection...");
    cancellationTokenSource.Cancel();
};
try
{
    // Keep the application running until it is cancelled  
    await Task.Delay(Timeout.Infinite, cancellationToken);
}
catch (TaskCanceledException)
{
}
// Stop the connection gracefully  
await hubConnection.StopAsync();

Console.WriteLine("SignalR connection closed.");