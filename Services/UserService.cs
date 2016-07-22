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
    public class UserService
    {
        public string AddUser(User user, CancellationToken token)
        {
            DataValidator<ArgumentNullException>.Validate<User>(user);

            return InMemoryProfileStore.AddUser(user, token);
        }
    }
}
