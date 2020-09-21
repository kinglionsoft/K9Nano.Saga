using System;
using System.Collections.Generic;

namespace K9Nano.Saga
{
    public interface ISagaContext
    {
        bool Success { get; set; }

        IList<string> ExecutedSteps { get; }

        AggregateException? Error { get; }

        void SetError(Exception exception);
    }
}