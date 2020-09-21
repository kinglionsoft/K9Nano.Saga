using System;
using System.Collections.Generic;
using System.Linq;

namespace K9Nano.Saga
{
    internal class SagaContainer<TContext> where TContext : ISagaContext
    {
        private bool _firstCompensate = true;

        protected readonly LinkedList<ISagaStep<TContext>> Sagas = new LinkedList<ISagaStep<TContext>>();

        protected LinkedListNode<ISagaStep<TContext>>? Current { get; set; }

        public SagaContainer(SagaStartDelegate<TContext> start)
        {
            Start = start;
        }

        public bool IsEmpty => Sagas.Count == 0;

        public SagaStartDelegate<TContext> Start { get; }

        public ISagaStep<TContext> NextStep => Current?.Value ?? throw new SagaException("Please make sure that MoveNext has returned true");

        public ISagaStep<TContext> Add(SagaDelegate<TContext> step, ISagaBuilder<TContext> builder, string? name)
        {
            var stepName = name ?? ("step-" + Sagas.Count);
            if (Sagas.Any(x => x.Name == stepName))
            {
                throw new SagaException($"Step with name ({stepName}) is already exists.");
            }
            var saga = new SagaStep<TContext>(step, builder, stepName);
            Sagas.AddLast(saga);
            return saga;
        }

        public bool MoveNext(TContext context)
        {
            if (Current == null)
            {
                if (context.Success)
                {
                    Current = Sagas.First;
                    return true;
                }
                return false;
            }

            if (context.Success)
            {
                // First
                if (Current == null)
                {
                    Current = Sagas.First;
                    return true;
                }

                if (Current.Next == null)
                {
                    Current = null;
                    return false;
                }

                Current = Current.Next;
                return true;
            }

            // Compensating

            if (_firstCompensate)
            {
                // Compensating itself
                _firstCompensate = false;
                return true;
            }

            if (Current.Previous == null)
            {
                Current = null;
                return false;
            }

            Current = Current.Previous;
            return true;
        }
    }
}