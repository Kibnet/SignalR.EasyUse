# SignalR.EasyUse

<img src="https://docs.microsoft.com/sv-se/azure/media/index/azure-signalr.svg" alt="SignalR" width="180"/>

![](https://github.com/Kibnet/SignalR.EasyUse/workflows/NuGet%20Generation/badge.svg?branch=master)![](https://img.shields.io/github/stars/Kibnet/SignalR.EasyUse.svg) ![](https://img.shields.io/github/forks/Kibnet/SignalR.EasyUse.svg)![](https://img.shields.io/github/issues/Kibnet/SignalR.EasyUse.svg)
 ![](https://img.shields.io/github/tag/Kibnet/SignalR.EasyUse.svg) ![](https://img.shields.io/github/release/Kibnet/SignalR.EasyUse.svg)
[![123](1232 "123")](http://вфы "123")
##What is it?
This is a framework that eliminates a whole class of contract non-compliance errors when writing client and server applications in C# using [SignalR](https://github.com/SignalR/SignalR "SignalR")

## How to use?
### Create interface
- Create in your solution a project in which contracts will lie.
- Add [Nuget Package](https://www.nuget.org/packages/SignalR.EasyUse.Interface/ "Nuget Package") in this project:


    Install-Package SignalR.EasyUse.Interface

####Define server methods
- Create an interface with server methods in this project. The name of the interface methods will be passed as an identifier, and the method parameters will be passed as-is. The type of method return value must be [Task](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task?view=netcore-3.1 "System.Threading.Tasks.Task").
- It can be inherited from `SignalR.EasyUse.Interface.IServerMethods` so that you can easily distinguish them and use them for their intended purpose. But inheritance is not necessary.

Example:
```csharp
public interface IChatHub: IServerMethods
{
    Task SendMessage(string user, string message);
}
```

####Define client methods
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
- Now you need to implement your server interface. Since you are required to follow the interface description, the hub will comply with the contract.
- Wherever you need to send messages to clients, you can now use strongly typed calls using the extension method:
`async Task SendAsync<T>(this IClientProxy clients, T payload)` 

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
- First, create a connection to the hub, as usual.
Example:
```csharp
_connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:53353/ChatHub")
                .Build();
```
- Then create a dynamic proxy for connection based on the interface.
Example:
```csharp
var hub = _connection.CreateHub<IChatHub>();
```
- Use a proxy to call server methods.
Example:
```csharp
await hub.SendMessage(UserName, MessageText);
```
This is what a call looks like without using the EasyUse framework
```csharp
await _connection.InvokeAsync("SendMessage", UserName, MessageText);
```
- To subscribe to a message from the server use the extension method:
`void Subscribe<T>(this HubConnection connection, Action<T> action)`
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
https://github.com/Kibnet/SignalRChat