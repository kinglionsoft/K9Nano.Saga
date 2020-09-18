using System;
using System.Threading.Tasks;

namespace K9Nano.Saga
{
    public static class SagaHelper
    {
        public static ISagaBuilder<TContext> StartsWith<TContext>(Func<ValueTask<TContext>> start) where TContext : ISagaContext
        {
            var builder = new SagaBuilder<TContext>()
                .StartsWith(start);

            return builder;
        }

        public static ISagaBuilder StartsWith(Func<ValueTask<SagaContext>> start)
        {
            ISagaBuilder builder = new SagaBuilder();
            builder.StartsWith(start);

            return builder;
        }
    }
}