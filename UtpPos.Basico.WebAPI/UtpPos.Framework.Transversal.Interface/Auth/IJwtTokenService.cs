using UtpPos.Framework.Domain.Entity.Auth;
using UtpPos.Framework.Transversal.Common.Auth;

namespace UtpPos.Framework.Transversal.Interface.Auth
{
    public interface IJwtTokenService
    {
        AccessTokenResult GenerateAccessToken(AuthUser user, int sessionId);
    }
}
