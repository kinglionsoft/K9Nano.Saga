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

        public async Task<TContext> ExecuteAsync()
        {
            var context = await _container.Start();
            if (context == null)
            {
                throw new SagaException("Can not start with a null context");
            }
            while (_container.MoveNext(context))
            {
                var next = _container.NextStep;

                if (context.Success)
                {
                    context.ExecutedSteps.Add(next.Name);
                    try
                    {
                        await next.Run.Invoke(context);
                    }
                    catch (Exception ex)
                    {
                        context.SetError(new SagaException(next.Name, false, ex));
                    }

                }
                else
                {
                    if (next.Compensate != null)
                    {
                        context.ExecutedSteps.Add(next.Name);
                        try
                        {
                            await next.Compensate.Invoke(context);
                        }
                        catch (Exception ex)
                        {
                            var sagaEx = new SagaException(next.Name, true, ex);
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