using System;
using System.Threading.Tasks;

namespace K9Nano.Saga
{
    public static class SagaHelper
    {
        public static ISagaBuilder<TContext> StartsWith<TContext>(Func<Task<TContext>> start) where TContext : ISagaContext
        {
            var builder = new SagaBuilder<TContext>()
                .StartsWith(start);

            return builder;
        }

        public static ISagaBuilder StartsWith(Func<Task<SagaContext>> start)
        {
            ISagaBuilder builder = new SagaBuilder();
            builder.StartsWith(start);

            return builder;
        }
    }
}