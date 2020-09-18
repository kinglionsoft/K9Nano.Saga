using System;
using System.Collections.Generic;

namespace K9Nano.Saga
{
    public abstract class SagaContextBase : ISagaContext
    {
        protected readonly IList<Exception> Errors;

        protected SagaContextBase()
        {
            Errors = new List<Exception>();
            Success = true;
            ExecutedSteps = new List<string>();
            Current = -1;
        }

        public bool Success { get; set; }

        public AggregateException? Error => Success ? null : new AggregateException(Errors);

        public int Current { get; set; }

        public IList<string> ExecutedSteps { get; }

        public virtual void SetError(Exception exception)
        {
            if (Success) Success = false;
            Errors.Add(exception);
        }
    }
}