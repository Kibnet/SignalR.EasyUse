# SignalR.EasyUse
# [Перейти в русское Readme](README.RU.md)

<img src="https://docs.microsoft.com/sv-se/azure/media/index/azure-signalr.svg" alt="SignalR" width="180"/>

![](https://github.com/Kibnet/SignalR.EasyUse/workflows/NuGet%20Generation/badge.svg?branch=master)
![](https://img.shields.io/github/issues/Kibnet/SignalR.EasyUse.svg?label=Issues)
![](https://img.shields.io/github/tag/Kibnet/SignalR.EasyUse.svg?label=Last%20Version)
![GitHub last commit](https://img.shields.io/github/last-commit/kibnet/SignalR.EasyUse)
![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/kibnet/SignalR.EasyUse?label=Code%20Size)

![GitHub search hit counter](https://img.shields.io/github/search/kibnet/SignalR.EasyUse/SignalR?label=GitHub%20Search%20Hits)
![Nuget](https://img.shields.io/nuget/dt/SignalR.EasyUse.Interface?label=Interface%20Downloads)
![Nuget](https://img.shields.io/nuget/dt/SignalR.EasyUse.Server?label=Server%20Downloads)
![Nuget](https://img.shields.io/nuget/dt/SignalR.EasyUse.Client?label=Client%20Downloads)

## What is it?

SignalR.EasyUse is a framework that helps eliminate contract mismatch errors when developing client and server applications in C# using [SignalR](https://github.com/SignalR/SignalR "SignalR").

## How to use it?

### Create an Interface

1. Create a new class library project in your solution that will contain contracts.
2. Add the [NuGet package](https://www.nuget.org/packages/SignalR.EasyUse.Interface/ "SignalR.EasyUse.Interface") to this project:

```
Install-Package SignalR.EasyUse.Interface
```

#### Define Server Methods

1. Create an interface with server methods. The interface method names will be used as identifiers, and the parameters will be passed directly. The method's return type must be [Task](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task?view=netcore-3.1 "System.Threading.Tasks.Task") or [Task\<TResult\>](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1?view=netcore-3.1 "System.Threading.Tasks.Task-1").
2. Optionally, the interface can inherit from `SignalR.EasyUse.Interface.IServerMethods` to easily distinguish server methods, though this is not required.

Example:

```csharp
public interface IChatHub: IServerMethods
{
    Task SendMessage(string user, string message);
    Task<List<User>> Login(string name, byte[] photo);
}
```

#### Define Client Methods

1. Define client methods (those called by the server) by creating a class for each method. The class name will be used as an identifier, and all public properties of the class will be passed as parameters in the order they are declared.
2. Optionally, these classes can inherit from `SignalR.EasyUse.Interface.IClientMethod`, but it’s not mandatory.

Example:

```csharp
public class ReceiveMessage: IClientMethod
{
    public string User { get; set; }
    public string Message { get; set; }
}
```

### Use Interfaces in the Server Project

1. Add a reference to the project containing the contracts.
2. Add the [NuGet package](https://www.nuget.org/packages/SignalR.EasyUse.Server/ "SignalR.EasyUse.Server") to the server project:

```
Install-Package SignalR.EasyUse.Server
```

3. Implement your server interface. Since the interface defines the contract, your hub will comply with it.
4. Use strongly typed client calls with the extension method to send messages:

```csharp
async Task SendAsync<T>(this IClientProxy clients, T payload)
```

Example:

```csharp
public class ChatHub : Hub, IChatHub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync(new ReceiveMessage
        {
            User = user,
            Message = message,
        });
        
        // Equivalent native call
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}
```

### Use Interfaces in the Client Project

1. Add a reference to the project containing the contracts.
2. Add the [NuGet package](https://www.nuget.org/packages/SignalR.EasyUse.Client/ "SignalR.EasyUse.Client") to the client project:

```
Install-Package SignalR.EasyUse.Client
```

3. Create a connection to the hub as usual:

```csharp
_connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:53353/ChatHub")
                .Build();
```

4. Create a dynamic proxy for the hub based on the interface:

```csharp
var hub = _connection.CreateHub<IChatHub>();
```

5. Use the proxy to call server methods:

```csharp
var users = await hub.Login(UserName, Photo);
await hub.SendMessage(UserName, MessageText);
```

Without EasyUse, the same call would look like this:

```csharp
var users = await connection.InvokeCoreAsync<List<User>>("Login", new object[] { UserName, Photo });
await _connection.InvokeAsync("SendMessage", UserName, MessageText);
```

6. To subscribe to messages from the server, use the extension method:

```csharp
void Subscribe<T>(this HubConnection connection, Action<T> action)
```

Example:

```csharp
_connection.Subscribe<ReceiveMessage>(data =>
{
    var newMessage = $"{data.User}: {data.Message}";
    Messages.Add(newMessage);
});
```

Without EasyUse, the same subscription would look like this:

```csharp
_connection.On<string, string>("ReceiveMessage", (user, message) =>
{
    var newMessage = $"{user}: {message}";
    Messages.Add(newMessage);
});
```

## Get a Sample Project

If you'd like to explore a working project using this framework, check out these sample applications:

- [Simple chat server on .NetCore 3.1 with three clients: WPF(.Net 4.6.1), WPF(.NetCore 3.1), and AvaloniaUI MVVM(.NetCore 3.1)](https://github.com/Kibnet/SignalRChat)
- [A more feature-rich chat with beautiful design using MVVM](https://github.com/Kibnet/SignalChat) (server on .NetCore 3.1, client on WPF MVVM(.Net 4.6.2))

## Communication

Suggestions and feedback are welcome. Feel free to reach out via [Telegram](https://t.me/kibnet).