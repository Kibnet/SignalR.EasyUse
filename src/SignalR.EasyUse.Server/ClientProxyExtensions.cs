using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SignalR.EasyUse.Interface;

namespace SignalR.EasyUse.Server
{
    /// <summary>
    /// Extentsions for IClientProxy
    /// </summary>
    public static class ClientProxyExtensions
    {
        /// <summary>
        /// Sending messages to clients
        /// </summary>
        /// <typeparam name="T">The method type of the client</typeparam>
        /// <param name="clients">Proxy that determines the message recipients</param>
        /// <param name="payload">Instance of the message method class</param>
        /// <returns></returns>
        public static async Task SendAsync<T>(this IClientProxy clients, T payload) where T : IClientMethod
        {
            await SendNotInheritedMessageAsync(clients, payload);
        }

        /// <summary>
        /// Sending a message to clients, even if the message is not marked with a marker interface
        /// </summary>
        /// <typeparam name="T">The method type of the client</typeparam>
        /// <param name="clients">Proxy that determines the message recipients</param>
        /// <param name="payload">Instance of the message method class</param>
        /// <returns></returns>
        public static async Task SendNotInheritedMessageAsync<T>(this IClientProxy clients, T payload)
        {
            var method = typeof(T).Name;
            var objects = payload.GetPropertiesValues();
            await clients.SendCoreAsync(method, objects);
        }

        /// <summary>
        /// Extract property values from the instance and return them as a list of objects
        /// </summary>
        /// <typeparam name="T">The type of the instance</typeparam>
        /// <param name="instance">Instance for extracting property values</param>
        /// <returns>Property value</returns>
        private static object[] GetPropertiesValues<T>(this T instance)
        {
            var recieveMessage = typeof(T);
            var paramsList = recieveMessage.GetProperties();
            var propertyValues = new object[paramsList.Length];
            for (int i = 0; i < paramsList.Length; i++)
            {
                var propertyInfo = paramsList[i];
                propertyValues[i] = propertyInfo.GetValue(instance);
            }

            return propertyValues;
        }
    }
}