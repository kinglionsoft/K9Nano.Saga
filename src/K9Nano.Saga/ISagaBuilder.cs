using System;
using System.Threading.Tasks;

namespace K9Nano.Saga
{
    public interface ISagaBuilder<TContext>
    {
        ISagaBuilder<TContext> StartsWith(Func<ValueTask<TContext>> start);
        ISagaStep<TContext> Then(Func<TContext, ValueTask> start, string? name = null);
        ISagaInvoker<TContext> Build();
    }

    public interface ISagaBuilder : ISagaBuilder<SagaContext>
    {

    }
}