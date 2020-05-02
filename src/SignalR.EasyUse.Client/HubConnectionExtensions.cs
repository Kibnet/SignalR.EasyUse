using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using SignalR.EasyUse.Interface;

namespace SignalR.EasyUse.Client
{
    public static class HubConnectionExtensions
    {
        /// <summary>
        /// Создать реализацию хаба из интерфейса, для вызова серверных методов
        /// </summary>
        /// <typeparam name="T">Интерфейс серверного хаба</typeparam>
        /// <param name="hubConnection">Cоединение с хабом</param>
        /// <returns>Реализация интерфейса</returns>
        public static T CreateHub<T>(this HubConnection hubConnection) where T : class, IServerMethods
        {
            var hub = HubDecorator<T>.Create(async (s, objects) => { await hubConnection.InvokeCoreAsync(s, objects); });
            return hub;
        }

        /// <summary>
        /// Подписаться на метод клиента, чтобы при получении сообщения вызывался экшн
        /// </summary>
        /// <typeparam name="T">Тип клиентского метода</typeparam>
        /// <param name="connection">Cоединение с хабом, на которое подписываемся</param>
        /// <param name="action">Действие, которое будет выполняться при вызове метода клиента</param>
        public static void Subscribe<T>(this HubConnection connection, Action<T> action) where T : IClientMethod
        {
            var recieveMessage = typeof(T);
            var methodName = recieveMessage.Name;
            var paramsList = recieveMessage.GetProperties()
                .Select((info) => info.PropertyType)
                .ToArray();

            connection.On(methodName, paramsList,
                objects =>
                {
                    var instance = objects.CreateInstance<T>();
                    return Task.FromResult(action.DynamicInvoke(instance));
                });
        }

        /// <summary>
        /// Cоздать экземпляр типа и заполнить его свойства списком объектов
        /// </summary>
        /// <typeparam name="T">Тип создаваемого экземпляра</typeparam>
        /// <param name="propertyValues">Значения cвойств</param>
        /// <returns>Созданный экземпляр</returns>
        public static T CreateInstance<T>(this object[] propertyValues)
        {
            var recieveMessage = typeof(T);
            var paramsList = recieveMessage.GetProperties();
            var instance = Activator.CreateInstance<T>();
            for (int i = 0; i < paramsList.Length; i++)
            {
                var propertyInfo = paramsList[i];
                var propertyValue = propertyValues.ElementAtOrDefault(i);
                propertyInfo.SetValue(instance, propertyValue);
            }

            return instance;
        }
    }
}