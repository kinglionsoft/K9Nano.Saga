using System.Threading.Tasks;

namespace K9Nano.Saga
{
    public delegate Task SagaDelegate<in TContext>(TContext context);
}