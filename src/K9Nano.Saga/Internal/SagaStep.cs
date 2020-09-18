using System;
using System.Threading.Tasks;

namespace K9Nano.Saga
{
    internal class SagaStep<TContext> : ISagaStep<TContext>
    {
        private readonly ISagaBuilder<TContext> _builder;

        public SagaStep(SagaDelegate<TContext> step, ISagaBuilder<TContext> builder, string name)
        {
            Run = step;
            Name = name;
            _builder = builder;
        }

        public SagaDelegate<TContext> Run { get; }

        public string Name { get; }

        public SagaDelegate<TContext>? Compensate { get; private set; }

        public bool StopIfError { get; private set; }

        public ISagaStep<TContext> Retry(int count)
        {
            return this;
        }

        public ISagaBuilder<TContext> CompensateWith(Func<TContext, ValueTask> start, bool stopIfError = false)
        {
            Compensate = ctx => start(ctx);
            StopIfError = stopIfError;
            return _builder;
        }

        public ISagaStep<TContext> Then(Func<TContext, ValueTask> start, string? name = null)
        {
            return _builder.Then(start, name);
        }

        public ISagaInvoker<TContext> Build()
        {
            return _builder.Build();
        }
    }
}