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
            _success = true;
            ExecutedSteps = new List<string>();
        }

        private bool _success;

        public bool Success
        {
            get => _success;
            set
            {
                if (_success)
                {
                    if (!value)
                    {
                        _success = false;
                    }
                }
                else
                {
                    if (!value)
                    {
                        throw new SagaException("Can not change Success from false to true");
                    }
                }
            }
        }

        public AggregateException? Error => Success ? null : new AggregateException(Errors);
        
        public IList<string> ExecutedSteps { get; }

        public virtual void SetError(Exception exception)
        {
            if (Success) Success = false;
            Errors.Add(exception);
        }
    }
}