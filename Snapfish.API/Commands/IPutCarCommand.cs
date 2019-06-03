using SintefSecure.Framework.SintefSecure.AspNetCore;
using Snapfish.API.API.ViewModels;

namespace Snapfish.API.API.Commands
{
    public interface IPutCarCommand : IAsyncCommand<int, SaveCar>
    {
    }
}