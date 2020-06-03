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
        /// Create a hub implementation from the interface to call server methods
        /// </summary>
        /// <typeparam name="T">Server hub interface</typeparam>
        /// <param name="hubConnection">Connection to the hub</param>
        /// <returns>Interface implementation</returns>
        public static T CreateHub<T>(this HubConnection hubConnection) where T : IServerMethods
        {
            return CreateNotInheritedHub<T>(hubConnection);
        }
        
        /// <summary>
        /// Create a hub implementation from the interface to call server methods,
        /// even if the interface is not marked with a marker
        /// </summary>
        /// <typeparam name="T">Server hub interface</typeparam>
        /// <param name="hubConnection">Connection to the hub</param>
        /// <returns>Interface implementation</returns>
        public static T CreateNotInheritedHub<T>(this HubConnection hubConnection)
        {
            var hub = HubDecorator<T>.Create(hubConnection);
            return hub;
        }

        /// <summary>
        /// Subscribe to the client method so that an action is called when a message is received
        /// </summary>
        /// <typeparam name="T">Type of client method</typeparam>
        /// <param name="connection">Connection to the hub that we subscribe to</param>
        /// <param name="action">The action that will be executed when you call the customer</param>
        public static void Subscribe<T>(this HubConnection connection, Action<T> action) where T : IClientMethod
        {
            SubscribeNotInherited<T>(connection, action);
        }
        
        /// <summary>
        /// Subscribe to the client method so that an action is called when a message is received,
        /// even if the message is not marked with a marker interface
        /// </summary>
        /// <typeparam name="T">Type of client method</typeparam>
        /// <param name="connection">Connection to the hub that we subscribe to</param>
        /// <param name="action">The action that will be executed when you call the customer</param>
        public static void SubscribeNotInherited<T>(this HubConnection connection, Action<T> action)
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
        /// Create an instance of the type and fill in its properties with a list of objects
        /// </summary>
        /// <typeparam name="T">Type of instance to create</typeparam>
        /// <param name="propertyValues">Property values</param>
        /// <returns>Created instance</returns>
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