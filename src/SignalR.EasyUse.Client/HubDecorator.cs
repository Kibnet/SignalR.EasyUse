using System;
using System.Reflection;
using System.Threading.Tasks;

namespace SignalR.EasyUse.Client
{
    /// <summary>
    /// Декоратор для хаба
    /// </summary>
    /// <typeparam name="T">Тип хаба</typeparam>
    public class HubDecorator<T> : DispatchProxy
    {
        private Func<string, object[], object> _invokeAction;

        /// <inheritdoc/>
        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            var result =_invokeAction.Invoke(targetMethod.Name, args);
            return Task.FromResult(result);
        }

        /// <summary>
        /// Создать декоратор
        /// </summary>
        /// <param name="invokeAction">Действие, которое будет вызываться при вызове методов</param>
        /// <returns>Декорированная реализация</returns>
        public static T Create(Func<string, object[], Task<object>> invokeAction)
        {
            object proxy = Create<T, HubDecorator<T>>();
            ((HubDecorator<T>)proxy).SetParameters(invokeAction);

            return (T)proxy;
        }
        
        /// <summary>
        /// Задать параметры
        /// </summary>
        /// <param name="invokeAction">Действие, для вызова</param>
        private void SetParameters(Func<string, object[], object> invokeAction)
        {
            _invokeAction = invokeAction;
        }
    }
}