using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace LBRS.Member.Service.Helper
{
    public class HttpClaimContext : IHttpClaimContext
    {
        public Guid UserId { get; }
        public string Role { get; }

        public HttpClaimContext(IHttpContextAccessor accessor)
        {
            var user = accessor.HttpContext!.User;

            var claimData = user.FindFirstValue(ClaimTypes.NameIdentifier);

            UserId = claimData != null ? Guid.Parse(claimData) : Guid.Empty;

            Role = user.FindFirstValue(ClaimTypes.Role)!;
        }
    }
}
