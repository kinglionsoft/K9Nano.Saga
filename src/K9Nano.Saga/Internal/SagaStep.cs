using System;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;

namespace K9Nano.Saga
{
    internal class SagaStep<TContext> : ISagaStep<TContext>
    {
        private readonly ISagaBuilder<TContext> _builder;
        private readonly SagaDelegate<TContext> _run;

        public SagaStep(SagaDelegate<TContext> step, ISagaBuilder<TContext> builder, string name)
        {
            _run = step;
            Name = name;
            _builder = builder;
            Run = _run;
        }

        public SagaDelegate<TContext> Run { get; private set; }

        public string Name { get; }

        public SagaDelegate<TContext>? Compensate { get; private set; }

        public bool StopIfError { get; private set; }

        public ISagaStep<TContext> Retry(int retryCount, Func<int, TimeSpan>? sleepDurationProvider = null)
        {
            var builder = Polly.Policy.Handle<Exception>();

            var policy = sleepDurationProvider == null
                ? builder.RetryAsync(retryCount)
                : builder.WaitAndRetryAsync(retryCount, sleepDurationProvider);

            Run = async ctx => await policy.ExecuteAsync(async () => await _run.Invoke(ctx));

            return this;
        }

        public ISagaStep<TContext> CompensateWith(Func<TContext, ValueTask> start, bool stopIfError = false)
        {
            Compensate = ctx => start(ctx);
            StopIfError = stopIfError;
            return this;
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