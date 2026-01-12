using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBRS.Member.Service.Helper
{
    public interface IJwtAuthTokenService
    {
        Task<string> GenerateAccessToken(string role, Guid userId);
    }
}
