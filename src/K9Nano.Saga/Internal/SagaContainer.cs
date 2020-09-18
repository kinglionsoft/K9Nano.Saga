using System;
using System.Collections.Generic;
using System.Linq;

namespace K9Nano.Saga
{
    internal class SagaContainer<TContext> where TContext : ISagaContext
    {
        protected readonly IList<ISagaStep<TContext>> Sagas = new List<ISagaStep<TContext>>();

        public SagaStartDelegate<TContext> Start { get; }

        public SagaContainer(SagaStartDelegate<TContext> start)
        {
            Start = start;
        }

        public bool IsEmpty => Sagas.Count == 0;

        public ISagaStep<TContext> Add(SagaDelegate<TContext> step, ISagaBuilder<TContext> builder, string? name)
        {
            var stepName = name ?? ("step-" + Sagas.Count);
            if (Sagas.Any(x => x.Name == stepName))
            {
                throw new SagaException($"Step with name ({stepName}) is already exists.");
            }
            var saga = new SagaStep<TContext>(step, builder, stepName);
            Sagas.Add(saga);
            return saga;
        }

        public bool TryGetNext(TContext context, out ISagaStep<TContext>? next)
        {
            if (context.Current < -1 || context.Current >= Sagas.Count)
            {
                throw new SagaException("Current of ISagaContext is invalid");
            }

            if (context.Success)
            {
                if (context.Current == Sagas.Count - 1)
                {
                    next = null;
                    return false;
                }

                next = Sagas[context.Current + 1];
                return true;
            }

            if (context.Current == 0)
            {
                next = null;
                return false;
            }

            next = Sagas[context.Current - 1];
            return true;
        }
    }
}