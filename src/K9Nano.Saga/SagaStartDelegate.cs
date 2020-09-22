using System.Threading.Tasks;

namespace K9Nano.Saga
{
    public delegate Task<TContext> SagaStartDelegate<TContext>();
}