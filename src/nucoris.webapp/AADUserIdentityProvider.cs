using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace nucoris.webapp
{
    public class AADUserIdentityProvider : application.interfaces.IUserIdentityProvider
    {
        private readonly IHttpContextAccessor _context;
        private readonly ILogger<AADUserIdentityProvider> _logger;
        private string _currentUsername;

        public AADUserIdentityProvider(IHttpContextAccessor context, ILogger<AADUserIdentityProvider> logger)
        {
            _context = context;
            _logger = logger;
        }

        public string CurrentUsername
        {
            get
            {
                if (_currentUsername == null)
                {
                    ExtractUsernameFromContext();
                }

                return _currentUsername;
            }
        }

        private void ExtractUsernameFromContext()
        {
            var claims = _context.HttpContext?.User?.Claims;

            if (claims != null)
            {
                var usernameClaim = claims.FirstOrDefault(i => i.Type == "preferred_username");
                if (usernameClaim != null)
                {
                    _currentUsername = usernameClaim.Value;
                }
                else
                {
                    _logger.LogError("Can't retrieve claim preferred_username from http context");
                }
            }
            else
            {
                _logger.LogError("Can't retrieve claims from http context");
            }
        }
    }
}
