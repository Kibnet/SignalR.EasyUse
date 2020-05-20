using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SignalR.EasyUse.Interface;

namespace SignalR.EasyUse.Server
{
    /// <summary>
    /// Расширения для IClientProxy
    /// </summary>
    public static class ClientProxyExtensions
    {
        /// <summary>
        /// Отправка сообщения клиентам
        /// </summary>
        /// <typeparam name="T">Тип метода клиента</typeparam>
        /// <param name="clients">Прокси определяющее получатей сообщения</param>
        /// <param name="payload">Экземпляр класса метода сообщения</param>
        /// <returns></returns>
        public static async Task SendAsync<T>(this IClientProxy clients, T payload) where T : IClientMethod
        {
            await SendNotInheritedMessageAsync(clients, payload);
        }

        /// <summary>
        /// Отправка сообщения клиентам, даже если сообщение не помечено интерфейсом-маркером
        /// </summary>
        /// <typeparam name="T">Тип метода клиента</typeparam>
        /// <param name="clients">Прокси определяющее получатей сообщения</param>
        /// <param name="payload">Экземпляр класса метода сообщения</param>
        /// <returns></returns>
        public static async Task SendNotInheritedMessageAsync<T>(this IClientProxy clients, T payload)
        {
            var method = typeof(T).Name;
            var objects = payload.GetPropertiesValues();
            await clients.SendCoreAsync(method, objects);
        }

        /// <summary>
        /// Извлечь из экземпляра значения свойств и вернуть их как список объектов
        /// </summary>
        /// <typeparam name="T">Тип экземпляра</typeparam>
        /// <param name="instance">Экземпляр для извлечение значений свойств</param>
        /// <returns>Значения свойств</returns>
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