using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CarpenterApi.Models
{
    internal class CarpenterUser
    {
        public string userId;

        public static CarpenterUser GetCurrentUser(ClaimsPrincipal claimsPrincipal)
        {
            string userId = "00000000000000000000000000000000";

            if (claimsPrincipal != null)
            {
                Claim nameIdentifierClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);

                if (nameIdentifierClaim != null)
                {
                    userId = nameIdentifierClaim.Value;
                }
            }

            return new() { userId = userId };
        }
    }
}
