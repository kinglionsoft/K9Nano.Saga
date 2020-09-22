using System.Threading.Tasks;

namespace K9Nano.Saga
{
    public interface ISagaInvoker<TContext>
    {
        Task<TContext> ExecuteAsync();
    }
}