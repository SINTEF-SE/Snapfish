using SintefSecure.Framework.SintefSecure.AspNetCore;
using Snapfish.API.API.ViewModels;

namespace Snapfish.API.API.Commands
{
    using Microsoft.AspNetCore.JsonPatch;

    public interface IPatchCarCommand : IAsyncCommand<int, JsonPatchDocument<SaveCar>>
    {
    }
}