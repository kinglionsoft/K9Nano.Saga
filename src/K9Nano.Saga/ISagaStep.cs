using System;
using System.Threading.Tasks;

namespace K9Nano.Saga
{
    public interface ISagaStep<TContext>
    {
        string Name { get; }

        SagaDelegate<TContext> Run { get; }

        SagaDelegate<TContext>? Compensate { get; }

        bool StopIfError { get; }

        ISagaBuilder<TContext> CompensateWith(Func<TContext, ValueTask> start, bool stopIfError = false);

        ISagaStep<TContext> Then(Func<TContext, ValueTask> start, string? name = null);

        ISagaInvoker<TContext> Build();
    }
}