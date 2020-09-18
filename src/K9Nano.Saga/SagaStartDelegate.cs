using System.Threading.Tasks;

namespace K9Nano.Saga
{
    public delegate ValueTask<TContext> SagaStartDelegate<TContext>();
}