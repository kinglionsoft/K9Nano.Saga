using System;
using System.Threading.Tasks;

namespace K9Nano.Saga
{
    public interface ISagaStep<TContext>
    {
        /// <summary>
        /// The name of step
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The method of step
        /// </summary>
        SagaDelegate<TContext> Run { get; }

        /// <summary>
        /// The compensating method called when <see cref="Run"/> failed
        /// </summary>
        SagaDelegate<TContext>? Compensate { get; }

        /// <summary>
        /// Weather continue if <see cref="Compensate"/> failed
        /// </summary>
        bool StopIfError { get; }

        /// <summary>
        ///  Setup a way to retry <see cref="Run"/> if failed
        /// </summary>
        ISagaStep<TContext> Retry(int retryCount, Func<int, TimeSpan>? sleepDurationProvider = null);

        /// <summary>
        ///  Setup a way to compensate <see cref="Run"/> if failed
        /// </summary>
        /// <param name="start"></param>
        /// <param name="stopIfError"></param>
        /// <returns></returns>
        ISagaStep<TContext> CompensateWith(Func<TContext, ValueTask> start, bool stopIfError = false);

        /// <summary>
        /// Start next step
        /// </summary>
        /// <param name="start"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        ISagaStep<TContext> Then(Func<TContext, ValueTask> start, string? name = null);

        /// <summary>
        /// Build the saga pipeline
        /// </summary>
        /// <returns></returns>
        ISagaInvoker<TContext> Build();
    }
}