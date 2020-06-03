using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.AspNetCore.SignalR.Client;

namespace SignalR.EasyUse.Client
{
    /// <summary>
    /// Decorator for hub
    /// </summary>
    /// <typeparam name="T">Type of hub</typeparam>
    public class HubDecorator<T> : DispatchProxy
    {
        private HubConnection _hubConnectionn;
        private MethodInfo _genericMethodInfo;

        /// <inheritdoc/>
        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            var returnType = targetMethod.ReturnType;
            if (returnType.IsGenericType && returnType.Name == "Task`1")
            {
                var makedGenericMethod = _genericMethodInfo.MakeGenericMethod(returnType.GenericTypeArguments);
                return makedGenericMethod.Invoke(null, new object[]{_hubConnectionn, targetMethod.Name, args, default(CancellationToken)});
            }

            if (!returnType.IsGenericType && returnType.Name == "Task")
            {
                return _hubConnectionn.InvokeCoreAsync(targetMethod.Name, args);
            }

            throw new InvalidCastException("Type must be a Task or Task<>");
        }

        /// <summary>
        /// Create a decorator
        /// </summary>
        /// <param name="hubConnection">Hub connection</param>
        /// <returns>Decorated the implementation</returns>
        public static T Create(HubConnection hubConnection)
        {
            object proxy = Create<T, HubDecorator<T>>();
            ((HubDecorator<T>)proxy).SetParameters(hubConnection);

            return (T)proxy;
        }

        /// <summary>
        /// Specify the parameter
        /// </summary>
        /// <param name="hubConnection">Hub connection</param>
        private void SetParameters(HubConnection hubConnection)
        {
            _hubConnectionn = hubConnection;

            //Retrieving the generic method for a call
            var extension = typeof(Microsoft.AspNetCore.SignalR.Client.HubConnectionExtensions);
            var methods = extension.GetMethods();
            _genericMethodInfo = methods.First(info => info.Name == "InvokeCoreAsync" && info.IsGenericMethod);
        }
    }
}