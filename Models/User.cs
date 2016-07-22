using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelinePattern.Models
{
    [Serializable]
    public class Profile
    {
        public User User { get; set; }
        public Address[] Address { get; set; }
        public Role[] Roles { get; set; }
    }

    [Serializable]
    public class User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Id { get; set; }
    }

    [Serializable]
    public class Address
    {
        public string Id { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
    }

    [Serializable]
    public class Role
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

    }

    public static class DataValidator<T> where T : Exception, new()
    {
        public static void Validate<U>(U input)
        {
            if (input == null)
            {
                throw new ArgumentException("argument cannot be null or empty");
            }
        }
    }
}
