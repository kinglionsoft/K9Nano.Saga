using System;
using System.Runtime.Serialization;

namespace K9Nano.Saga
{
    [Serializable]
    public class SagaException : Exception
    {
        /// <summary>
        /// Name of the step
        /// </summary>
        public string? StepName { get; set; }

        /// <summary>
        /// Weather the error happened in compensating step
        /// </summary>
        public bool IsCompensating { get; set; }

        public SagaException()
        {
        }

        public SagaException(string stepName, bool isCompensating, Exception inner): this(inner.Message, inner)
        {
            StepName = stepName;
            IsCompensating = isCompensating;
        }

        public SagaException(string message) : base(message)
        {
        }

        public SagaException(string message, Exception inner) : base(message, inner)
        {
        }

        protected SagaException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}