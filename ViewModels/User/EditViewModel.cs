using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SampleMvcApp.ViewModels.User
{
    public class EditViewModel
    {
        public IdentityUser User { get; set; }
        public IList<string> UserRoles { get; set; }
        public IList<string> Roles { get; set; }

        public EditViewModel(IdentityUser user, IList<string> userRoles, IList<string> roles)
        {
            User = user;
            UserRoles = userRoles;
            Roles = roles;
        }
    }
}
