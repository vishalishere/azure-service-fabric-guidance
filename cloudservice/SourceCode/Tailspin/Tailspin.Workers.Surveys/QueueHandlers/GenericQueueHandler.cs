namespace Tailspin.Workers.Surveys.QueueHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Tailspin.Web.Survey.Shared.Helpers;
    using Tailspin.Web.Survey.Shared.Stores.AzureStorage;

    public abstract class GenericQueueHandler<T> where T : AzureQueueMessage
    {
        protected static void ProcessMessages(IAzureQueue<T> queue, IEnumerable<T> messages, Func<T, bool> action)
        {
            if (queue == null)
            {
                throw new ArgumentNullException("queue");
            }

            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            if (messages == null)
            {
                throw new ArgumentNullException("messages");
            }

            foreach (var message in messages)
            {
                var allowDelete = false;
                var corruptMessage = false;

                try
                {
                    allowDelete = action(message);
                }
                catch (Exception ex)
                {
                    TraceHelper.TraceWarning(ex.TraceInformation());
                    allowDelete = false;
                    corruptMessage = true;
                }
                finally
                {
                    if (allowDelete || (corruptMessage && message.GetMessageReference().DequeueCount > 5))
                    {
                        queue.DeleteMessage(message);
                    }
                }
            }
        }

        protected virtual void Sleep(TimeSpan interval)
        {
            Thread.Sleep(interval);
        }
    }
}