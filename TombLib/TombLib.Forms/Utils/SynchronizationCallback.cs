// Code from: https://blogs.msdn.microsoft.com/kaelr/2007/09/05/synchronizationcallback/

using System;
using System.Security.Permissions;

namespace System.Threading
{
    /// <summary>
    /// Represents a callback method to be executed with a specific <see cref="SynchronizationContext"/>.
    /// </summary>
    public sealed class SynchronizationCallback
    {
        /// <summary>
        /// Creates an instance of a <see cref="SynchronizationCallback"/> using the default <see cref="SynchronizationContext"/> and a standard callback that accepts a single user object as an argument.
        /// </summary>
        /// <param name="callback">The delegate to invoke.</param>
        private SynchronizationCallback(Delegate callback)
            : this(callback, SynchronizationContext.Current ?? new SynchronizationContext())
        {
        }

        /// <summary>
        /// Creates an instance of a <see cref="SynchronizationCallback"/> using the default <see cref="SynchronizationContext"/> and a callback that accepts no arguments.
        /// </summary>
        /// <param name="callback">The delegate to invoke.</param>
        public SynchronizationCallback(Action callback)
            : this(callback, SynchronizationContext.Current ?? new SynchronizationContext())
        {
        }

        /// <summary>
        /// Creates an instance of a <see cref="SynchronizationCallback"/> using the default <see cref="SynchronizationContext"/> and a callback that accepts a user object as arguments.
        /// </summary>
        /// <param name="callback">The delegate to invoke.</param>
        public SynchronizationCallback(Action<object> callback)
            : this(callback, SynchronizationContext.Current ?? new SynchronizationContext())
        {
        }

        /// <summary>
        /// Creates an instance of a <see cref="SynchronizationCallback"/> using the default <see cref="SynchronizationContext"/> and a callback that accepts the <see cref="SynchronizationContext"/> and a user object as arguments.
        /// </summary>
        /// <param name="callback">The delegate to invoke.</param>
        public SynchronizationCallback(Action<SynchronizationContext, object> callback)
            : this(callback, SynchronizationContext.Current ?? new SynchronizationContext())
        {
        }

        /// <summary>
        /// Creates an instance of a <see cref="SynchronizationCallback"/> using a specified <see cref="SynchronizationContext"/> and a standard callback that accepts a single user object as an argument.
        /// </summary>
        /// <param name="callback">The delegate to invoke.</param>
        /// <param name="context">The synchronization context to set while invoking the delegate.</param>
        private SynchronizationCallback(Delegate callback, SynchronizationContext context)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            var parameters = callback.Method.GetParameters();
            if (parameters.Length != 0 && (parameters[0].ParameterType != typeof(object) || parameters.Length > 1))
                throw new ArgumentException("The delegate must have exactly one parameter of type object.", "callback");

            Callback = callback;
            CallbackParameters = parameters.Length;
            Context = context;
        }

        /// <summary>
        /// Creates an instance of a <see cref="SynchronizationCallback"/> using a specified <see cref="SynchronizationContext"/> and a callback that accepts the <see cref="SynchronizationContext"/> and a user object as arguments.
        /// </summary>
        /// <param name="callback">The delegate to invoke.</param>
        /// <param name="context">The synchronization context to set while invoking the delegate.</param>
        public SynchronizationCallback(Action<SynchronizationContext, object> callback, SynchronizationContext context)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            Callback = callback;
            CallbackParameters = 2;
            Context = context;
        }

        /// <summary>
        /// Gets the inner callback to be invoked by the <see cref="Invoke"/> method.
        /// </summary>
        /// <value>
        /// The <see cref="Delegate"/> that is associated with this callback.
        /// </value>
        private Delegate Callback
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the number of parameters in <see cref="Callback"/>.
        /// </summary>
        private int CallbackParameters
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the synchronization context that is set while invoking <see cref="Callback"/>.
        /// </summary>
        /// <value>
        /// The <see cref="SynchronizationContext"/> associated with this callback.
        /// </value>
        private SynchronizationContext Context
        {
            get;
            set;
        }

        /// <summary>
        /// Dynamically invokes (late-bound) the method represented by this callback.
        /// </summary>
        /// <param name="state">An object containing information to be used by the callback method.</param>
        [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.ControlPolicy)]
        public void Invoke()
        {
            Invoke(null);
        }

        /// <summary>
        /// Dynamically invokes (late-bound) the method represented by this callback with the given user state object.
        /// </summary>
        /// <param name="state">An object containing information to be used by the callback method.</param>
        [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.ControlPolicy)]
        public void Invoke(object state)
        {
            SynchronizationContext current = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(Context);
            try
            {
                switch (CallbackParameters)
                {
                    case 0:
                        Callback.DynamicInvoke(null);
                        break;
                    case 1:
                        Callback.DynamicInvoke(state);
                        break;
                    case 2:
                        Callback.DynamicInvoke(Context, state);
                        break;
                    default:
                        throw new InvalidProgramException("Delegate has an invalid number of callback parameters.");
                }
            }
            finally
            {
                if (SynchronizationContext.Current == Context)
                    SynchronizationContext.SetSynchronizationContext(current);
            }
        }

        /// <summary>
        /// Wraps a <see cref="WaitCallback"/> in a <see cref="SynchronizationCallback"/> using the default <see cref="SynchronizationContext"/>.
        /// </summary>
        /// <param name="callback">The callback to wrap.</param>
        /// <returns>A <see cref="SynchronizationCallback"/> that wraps <paramref name="callback"/>.</returns>
        public static implicit operator SynchronizationCallback(WaitCallback callback)
        {
            return new SynchronizationCallback(callback);
        }

        /// <summary>
        /// Wraps a <see cref="TimerCallback"/> in a <see cref="SynchronizationCallback"/> using the default <see cref="SynchronizationContext"/>.
        /// </summary>
        /// <param name="callback">The callback to wrap.</param>
        /// <returns>A <see cref="SynchronizationCallback"/> that wraps <paramref name="callback"/>.</returns>
        public static implicit operator SynchronizationCallback(TimerCallback callback)
        {
            return new SynchronizationCallback(callback);
        }

        /// <summary>
        /// Wraps a <see cref="SendOrPostCallback"/> in a <see cref="SynchronizationCallback"/> using the default <see cref="SynchronizationContext"/>.
        /// </summary>
        /// <param name="callback">The callback to wrap.</param>
        /// <returns>A <see cref="SynchronizationCallback"/> that wraps <paramref name="callback"/>.</returns>
        public static implicit operator SynchronizationCallback(SendOrPostCallback callback)
        {
            return new SynchronizationCallback(callback);
        }

        /// <summary>
        /// Wraps a <see cref="ThreadStart"/> routine in a <see cref="SynchronizationCallback"/> using the default <see cref="SynchronizationContext"/>.
        /// </summary>
        /// <param name="callback">The callback to wrap.</param>
        /// <returns>A <see cref="SynchronizationCallback"/> that wraps <paramref name="callback"/>.</returns>
        public static implicit operator SynchronizationCallback(ThreadStart callback)
        {
            return new SynchronizationCallback(callback);
        }

        /// <summary>
        /// Wraps a <see cref="ParameterizedThreadStart"/> routine in a <see cref="SynchronizationCallback"/> using the default <see cref="SynchronizationContext"/>.
        /// </summary>
        /// <param name="callback">The callback to wrap.</param>
        /// <returns>A <see cref="SynchronizationCallback"/> that wraps <paramref name="callback"/>.</returns>
        public static implicit operator SynchronizationCallback(ParameterizedThreadStart callback)
        {
            return new SynchronizationCallback(callback);
        }

        /// <summary>
        /// Wraps an <see cref="Action"/> in a <see cref="SynchronizationCallback"/> using the default <see cref="SynchronizationContext"/>.
        /// </summary>
        /// <param name="callback">The callback to wrap.</param>
        /// <returns>A <see cref="SynchronizationCallback"/> that wraps <paramref name="callback"/>.</returns>
        public static implicit operator SynchronizationCallback(Action callback)
        {
            return new SynchronizationCallback(callback);
        }

        /// <summary>
        /// Wraps an <see cref="Action"/> in a <see cref="SynchronizationCallback"/> using the default <see cref="SynchronizationContext"/>.
        /// </summary>
        /// <param name="callback">The callback to wrap.</param>
        /// <returns>A <see cref="SynchronizationCallback"/> that wraps <paramref name="callback"/>.</returns>
        public static implicit operator SynchronizationCallback(Action<object> callback)
        {
            return new SynchronizationCallback(callback);
        }

        /// <summary>
        /// Converts a <see cref="SynchronizationCallback"/> into a <see cref="WaitCallback"/> that can be invoked.
        /// </summary>
        /// <param name="callback">The callback to convert.</param>
        /// <returns>A <see cref="WaitCallback"/> that invokes <paramref name="callback"/>.</returns>
        public static implicit operator WaitCallback(SynchronizationCallback callback)
        {
            return callback.Invoke;
        }

        /// <summary>
        /// Converts a <see cref="SynchronizationCallback"/> into a <see cref="TimerCallback"/> that can be invoked.
        /// </summary>
        /// <param name="callback">The callback to convert.</param>
        /// <returns>A <see cref="TimerCallback"/> that invokes <paramref name="callback"/>.</returns>
        public static implicit operator TimerCallback(SynchronizationCallback callback)
        {
            return callback.Invoke;
        }

        /// <summary>
        /// Converts a <see cref="SynchronizationCallback"/> into a <see cref="SendOrPostCallback"/> that can be invoked.
        /// </summary>
        /// <param name="callback">The callback to convert.</param>
        /// <returns>A <see cref="SendOrPostCallback"/> that invokes <paramref name="callback"/>.</returns>
        public static implicit operator SendOrPostCallback(SynchronizationCallback callback)
        {
            return callback.Invoke;
        }

        /// <summary>
        /// Converts a <see cref="SynchronizationCallback"/> into a <see cref="ThreadStart"/> routine that can be invoked.
        /// </summary>
        /// <param name="callback">The callback to convert.</param>
        /// <returns>A <see cref="ThreadStart"/> routine that invokes <paramref name="callback"/>.</returns>
        public static implicit operator ThreadStart(SynchronizationCallback callback)
        {
            return callback.Invoke;
        }

        /// <summary>
        /// Converts a <see cref="SynchronizationCallback"/> into a <see cref="ParameterizedThreadStart"/> routine that can be invoked.
        /// </summary>
        /// <param name="callback">The callback to convert.</param>
        /// <returns>A <see cref="ParameterizedThreadStart"/> routine that invokes <paramref name="callback"/>.</returns>
        public static implicit operator ParameterizedThreadStart(SynchronizationCallback callback)
        {
            return callback.Invoke;
        }

        /// <summary>
        /// Converts a <see cref="SynchronizationCallback"/> into an <see cref="Action"/> that can be invoked.
        /// </summary>
        /// <param name="callback">The callback to convert.</param>
        /// <returns>An <see cref="Action"/> that invokes <paramref name="callback"/>.</returns>
        public static implicit operator Action(SynchronizationCallback callback)
        {
            return callback.Invoke;
        }

        /// <summary>
        /// Converts a <see cref="SynchronizationCallback"/> into an <see cref="Action"/> that can be invoked.
        /// </summary>
        /// <param name="callback">The callback to convert.</param>
        /// <returns>An <see cref="Action"/> that invokes <paramref name="callback"/>.</returns>
        public static implicit operator Action<object>(SynchronizationCallback callback)
        {
            return callback.Invoke;
        }

        /// <summary>
        /// Converts a <see cref="SynchronizationCallback"/> into a <see cref="WaitCallback"/> that can be invoked.
        /// </summary>
        /// <param name="callback">The callback to convert.</param>
        /// <returns>A <see cref="WaitCallback"/> that invokes this callback.</returns>
        public WaitCallback ToWaitCallback()
        {
            return Invoke;
        }

        /// <summary>
        /// Converts a <see cref="SynchronizationCallback"/> into a <see cref="TimerCallback"/> that can be invoked.
        /// </summary>
        /// <param name="callback">The callback to convert.</param>
        /// <returns>A <see cref="TimerCallback"/> that invokes this callback.</returns>
        public TimerCallback ToTimerCallback()
        {
            return Invoke;
        }

        /// <summary>
        /// Converts a <see cref="SynchronizationCallback"/> into a <see cref="SendOrPostCallback"/> that can be invoked.
        /// </summary>
        /// <returns>A <see cref="SendOrPostCallback"/> that invokes this callback.</returns>
        public SendOrPostCallback ToSendOrPostCallback()
        {
            return Invoke;
        }

        /// <summary>
        /// Converts a <see cref="SynchronizationCallback"/> into a <see cref="ThreadStart"/> routine that can be invoked.
        /// </summary>
        /// <returns>A <see cref="ThreadStart"/> routine that invokes this callback.</returns>
        public ThreadStart ToThreadStart()
        {
            return Invoke;
        }

        /// <summary>
        /// Converts a <see cref="SynchronizationCallback"/> into a <see cref="ParameterizedThreadStart"/> routine that can be invoked.
        /// </summary>
        /// <returns>A <see cref="ParameterizedThreadStart"/> routine that invokes this callback.</returns>
        public ParameterizedThreadStart ToParameterizedThreadStart()
        {
            return Invoke;
        }

        /// <summary>
        /// Converts a <see cref="SynchronizationCallback"/> into an <see cref="Action"/> that can be invoked.
        /// </summary>
        /// <returns>An <see cref="Action"/> that invokes this callback.</returns>
        public Action ToAction()
        {
            return Invoke;
        }

        /// <summary>
        /// Converts a <see cref="SynchronizationCallback"/> into an <see cref="Action"/> that can be invoked.
        /// </summary>
        /// <returns>An <see cref="Action"/> that invokes this callback.</returns>
        public Action<object> ToObjectAction()
        {
            return Invoke;
        }
    }
}
