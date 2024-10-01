# SignalR.EasyUse
# [Switch to English Readme](README.md)

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

## Что это?

SignalR.EasyUse — это фреймворк, который помогает избежать ошибок несоответствия контрактов при создании клиентских и серверных приложений на C# с использованием [SignalR](https://github.com/SignalR/SignalR "SignalR").

## Как использовать?

### Создание интерфейса

1. Создайте новый проект-библиотеку в своем решении, который будет содержать контракты.
2. Добавьте [пакет NuGet](https://www.nuget.org/packages/SignalR.EasyUse.Interface/ "SignalR.EasyUse.Interface") в этот проект:

```
Install-Package SignalR.EasyUse.Interface
```

#### Определение серверных методов

1. Создайте интерфейс с методами сервера. Имена методов интерфейса будут использоваться как идентификаторы, а параметры — передаваться как есть. Возвращаемое значение метода должно быть типа [Task](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task?view=netcore-3.1 "System.Threading.Tasks.Task") или [Task\<TResult\>](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1?view=netcore-3.1 "System.Threading.Tasks.Task-1").
2. Интерфейс может наследоваться от `SignalR.EasyUse.Interface.IServerMethods` для удобного различения, но это не обязательно.

Пример:

```csharp
public interface IChatHub: IServerMethods
{
    Task SendMessage(string user, string message);
    Task<List<User>> Login(string name, byte[] photo);
}
```

#### Определение клиентских методов

1. Для методов клиента создайте класс для каждого метода. Имя класса будет использоваться как идентификатор, а все его публичные свойства будут передаваться как параметры в порядке их объявления.
2. Эти классы могут наследоваться от `SignalR.EasyUse.Interface.IClientMethod`, но это не обязательно.

Пример:

```csharp
public class ReceiveMessage: IClientMethod
{
    public string User { get; set; }
    public string Message { get; set; }
}
```

### Использование интерфейсов на сервере

1. Добавьте ссылку на проект с контрактами.
2. Добавьте [пакет NuGet](https://www.nuget.org/packages/SignalR.EasyUse.Server/ "SignalR.EasyUse.Server") в проект сервера:

```
Install-Package SignalR.EasyUse.Server
```

3. Реализуйте интерфейс сервера. Благодаря этому реализации будут соответствовать описанным контрактам.
4. Для отправки сообщений клиентам можно использовать строго типизированные вызовы с помощью метода расширения:

```csharp
async Task SendAsync<T>(this IClientProxy clients, T payload)
```

Пример:

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
        
        // Эквивалентный нативный вызов
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}
```

### Использование интерфейсов на клиенте

1. Добавьте ссылку на проект с контрактами.
2. Добавьте [пакет NuGet](https://www.nuget.org/packages/SignalR.EasyUse.Client/ "SignalR.EasyUse.Client") в проект клиента:

```
Install-Package SignalR.EasyUse.Client
```

3. Создайте подключение к хабу:

```csharp
_connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:53353/ChatHub")
                .Build();
```

4. Создайте динамический прокси для хаба на основе интерфейса:

```csharp
var hub = _connection.CreateHub<IChatHub>();
```

5. Используйте прокси для вызова методов сервера:

```csharp
var users = await hub.Login(UserName, Photo);
await hub.SendMessage(UserName, MessageText);
```

Пример без использования EasyUse:

```csharp
var users = await connection.InvokeCoreAsync<List<User>>("Login", new object[] { UserName, Photo });
await _connection.InvokeAsync("SendMessage", UserName, MessageText);
```

6. Для подписки на сообщения от сервера используйте метод расширения:

```csharp
void Subscribe<T>(this HubConnection connection, Action<T> action)
```

Пример:

```csharp
_connection.Subscribe<ReceiveMessage>(data =>
{
    var newMessage = $"{data.User}: {data.Message}";
    Messages.Add(newMessage);
});
```

Пример без использования EasyUse:

```csharp
_connection.On<string, string>("ReceiveMessage", (user, message) =>
{
    var newMessage = $"{user}: {message}";
    Messages.Add(newMessage);
});
```

## Получить пример

Для ознакомления с работающими проектами, использующими этот фреймворк, перейдите по следующим ссылкам:

- [Простой чат на .NetCore 3.1 и три клиента: WPF(.Net 4.6.1), WPF(.NetCore 3.1) и AvaloniaUI MVVM(.NetCore 3.1)](https://github.com/Kibnet/SignalRChat)
- [Чат с более сложным функционалом и красивым дизайном на MVVM](https://github.com/Kibnet/SignalChat) (сервер на .NetCore 3.1, клиент на WPF MVVM(.Net 4.6.2))

## Связь

Буду рад вашим предложениям и комментариям. Для связи используйте [Telegram](https://t.me/kibnet).