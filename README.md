# SignalR.EasyUse

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
This is a framework that eliminates a whole class of contract non-compliance errors when writing client and server applications in C# using [SignalR](https://github.com/SignalR/SignalR "SignalR")

## How to use?
### Create interface
- Create a project in your solution that will contain contracts.
- Add [Nuget Package](https://www.nuget.org/packages/SignalR.EasyUse.Interface/ "Nuget Package") in this project:
```
    Install-Package SignalR.EasyUse.Interface
```

#### Define server methods
- Create an interface with server methods in this project. The name of the interface methods will be passed as an identifier, and the method parameters will be passed as-is. The type of method return value must be [Task](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task?view=netcore-3.1 "System.Threading.Tasks.Task").
- It can be inherited from `SignalR.EasyUse.Interface.IServerMethods` so that you can easily distinguish them and use them for their intended purpose. But inheritance is not necessary.

Example:
```csharp
public interface IChatHub: IServerMethods
{
    Task SendMessage(string user, string message);
}
```

#### Define client methods
- The client methods that the server calls are defined slightly differently. You need to create a class for each method. The class name will be passed as an identifier. All its public properties will be passed as parameters in the order in which they are declared in the class.
- These classes can be inherited from `SignalR.EasyUse.Interface.IClientMethod` so that you can easily distinguish them and use them for their intended purpose. But inheritance is not necessary.

Example:
```csharp
public class ReceiveMessage: IClientMethod
{
    public string User { get;set; }
    public string Message{ get;set; }
}
```

### Use interfaces in server project
- Add a link to the project with contracts.
- Now you need to implement your server interface. Since you are required to follow the interface description, the hub will comply with the contract.
- Wherever you need to send messages to clients, you can now use strongly typed calls using the extension method:
```csharp
async Task SendAsync<T>(this IClientProxy clients, T payload)
```

Example:
```csharp
//This class implements the IChatHub interface
public class ChatHub : Hub, IChatHub
{
    //This method from IChatHub interface
    public async Task SendMessage(string user, string message)
    {
        //EasyUse send message
        await Clients.All.SendAsync(new ReceiveMessage
        {
            User = user,
            Message = message,
		});
		
		//The code above corresponds to this
        //Native send message
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}
```

### Use interfaces in client project
- Add a link to the project with contracts.
- Сreate a connection to the hub, as usual:
```csharp
_connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:53353/ChatHub")
                .Build();
```
- Then create a dynamic proxy for connection based on the interface:
```csharp
var hub = _connection.CreateHub<IChatHub>();
```
- Use a proxy to call server methods:
```csharp
await hub.SendMessage(UserName, MessageText);
```
This is what a call looks like without using the EasyUse framework:
```csharp
await _connection.InvokeAsync("SendMessage", UserName, MessageText);
```
- To subscribe to a message from the server use the extension method:
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
This is what a subscribe looks like without using the EasyUse framework:
```csharp
_connection.On<string, string>("ReceiveMessage", (user, message) =>
{
	var newMessage = $"{user}: {message}";
	Messages.Add(newMessage);
});
```

## Get a sample!
If you want to touch the working project right away, see examples of ready-made applications that use the framework:
- https://github.com/Kibnet/SignalRChat - Simple chat server on .NetCore 3.1 and 3 clients: WPF(Net 4.6.1), WPF(NetCore 3.1) and AvaloniaUI MVVM(NetCore 3.1)
- https://github.com/Kibnet/SignalChat - Chat with a little more functionality and a beautiful design using MVVM. Server on .NetCore 3.1 and client on WPF MVVM(Net 4.6.2)

## Communication
Any suggestions and comments are welcome. If you want to contact me, use [Telegram](https://t.me/kibnet)
