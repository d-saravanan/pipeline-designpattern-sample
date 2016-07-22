using PipelinePattern.DAL;
using PipelinePattern.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PipelinePattern.Services
{
    public class RoleService
    {
        public void AddRoles(string userId, Role[] roles, CancellationToken token)
        {
            DataValidator<ArgumentNullException>.Validate<string>(userId);
            DataValidator<ArgumentNullException>.Validate<Role[]>(roles);

            if (!token.IsCancellationRequested)
                InMemoryProfileStore.AddUserRoles(userId, roles, token);
        }
    }
}

