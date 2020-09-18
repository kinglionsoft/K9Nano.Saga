using System.Threading.Tasks;

namespace K9Nano.Saga
{
    public delegate ValueTask SagaDelegate<in TContext>(TContext context);
}