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
    public class AddressService
    {
        public void AddAddress(string userId, Address[] addresses, CancellationToken token)
        {
            DataValidator<ArgumentNullException>.Validate<string>(userId);
            DataValidator<ArgumentNullException>.Validate<Address[]>(addresses);

            Parallel.For(0, addresses.Length, (i) =>
            {
                addresses[i].Id = Guid.NewGuid().ToString();
            });

            if (!token.IsCancellationRequested)
                InMemoryProfileStore.AddUserAddress(userId, addresses, token);
        }
    }
}
