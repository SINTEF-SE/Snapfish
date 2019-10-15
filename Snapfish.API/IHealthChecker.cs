namespace Snapfish.API
{
    using System.Threading.Tasks;

    public interface IHealthChecker
    {
        Task CheckHealth();
    }
}
