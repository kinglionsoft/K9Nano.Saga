using System;
using System.Threading.Tasks;

namespace K9Nano.Saga
{
    internal class SagaInvoker<TContext> : ISagaInvoker<TContext> where TContext : ISagaContext
    {
        private readonly SagaContainer<TContext> _container;

        public SagaInvoker(SagaContainer<TContext> container)
        {
            _container = container;
        }

        public async ValueTask<TContext> ExecuteAsync()
        {
            var context = await _container.Start();
            if (context == null)
            {
                throw new SagaException("Can not start with a null context");
            }
            while (_container.TryGetNext(context, out ISagaStep<TContext>? next))
            {
                if (context.Success)
                {
                    context.Current++;

#pragma warning disable CS8602 // 取消引用可能出现的空引用。
                    context.ExecutedSteps.Add(next.Name);
                    try
                    {
                        await next.Run.Invoke(context);
                    }
                    catch (Exception ex)
                    {
                        context.SetError(new SagaException(next.Name, context.Current, false, ex));
                        // Increase Current for compensating itself
                        context.Current++;
                    }

#pragma warning restore CS8602 // 取消引用可能出现的空引用。

                }
                else
                {
                    context.Current--;
#pragma warning disable CS8602 // 取消引用可能出现的空引用。
                    if (next.Compensate != null)
                    {
                        context.ExecutedSteps.Add(next.Name);
#pragma warning restore CS8602 // 取消引用可能出现的空引用。
                        try
                        {
                            await next.Compensate.Invoke(context);
                        }
                        catch (Exception ex)
                        {
                            var sagaEx = new SagaException(next.Name, context.Current, true, ex);
                            context.SetError(sagaEx);
                            if (next.StopIfError)
                            {
                                throw sagaEx;
                            }
                        }
                    }
                }
            }

            return context;
        }
    }
}