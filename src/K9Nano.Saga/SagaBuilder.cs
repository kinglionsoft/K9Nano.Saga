using System;
using System.Threading.Tasks;

namespace K9Nano.Saga
{
    public class SagaBuilder<TContext> : ISagaBuilder<TContext> where TContext : ISagaContext
    {
#pragma warning disable 8618
        private SagaContainer<TContext> _container;
#pragma warning restore 8618

        public ISagaBuilder<TContext> StartsWith(Func<Task<TContext>> start)
        {
            _container = new SagaContainer<TContext>(StartDelegate);

            return this;

            Task<TContext> StartDelegate() => start();
        }

        public ISagaStep<TContext> Then(Func<TContext, Task> then, string? name = null)
        {
            return _container.Add(Next, this, name);

            Task Next(TContext context) => then(context);
        }

        public ISagaInvoker<TContext> Build()
        {
            if (_container.IsEmpty)
            {
                throw new SagaException("Step container is empty");
            }
            return new SagaInvoker<TContext>(_container);
        }
    }

    public class SagaBuilder : SagaBuilder<SagaContext>, ISagaBuilder
    {

    }
}