using Microsoft.CodeAnalysis.Operations;
using Serilog;

namespace Snapfish.API.API.Constants
{
    public class AuthorizationControllerRoute
    {
        public const string Authorize = ControllerName.Authorization + nameof(Authorize);
        public const string Logout = ControllerName.Authorization + nameof(Logout);
    }
}