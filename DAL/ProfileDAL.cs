using PipelinePattern.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PipelinePattern.DAL
{
    public static class JsonFileStore
    {
        public static bool Store<T>(T data, string fileName)
        {
            try
            {
                var jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(data);

                using (var fs = new FileStream(fileName, FileMode.CreateNew))
                {
                    var swr = new StreamWriter(fs);
                    swr.WriteLine(jsonData);
                    swr.Flush();
                }
                return true;
            }
            catch (Exception exception)
            {
                Trace.Write(exception);
                return false;
            }
        }
    }

    public static class InMemoryProfileStore
    {
        static ConcurrentDictionary<string, Profile> _profileStore = new ConcurrentDictionary<string, Profile>();

        static ConcurrentDictionary<string, User> _userStore = new ConcurrentDictionary<string, User>();

        static ConcurrentDictionary<string, Address[]> _userAddressStore = new ConcurrentDictionary<string, Address[]>();

        static ConcurrentDictionary<string, string[]> _userAddresses = new ConcurrentDictionary<string, string[]>();

        static ConcurrentDictionary<string, Role[]> _userRoles = new ConcurrentDictionary<string, Role[]>();

        static string path = @"C:\Users\Saran\Documents\visual studio 2013\Projects\WinFormApps\PipelinePattern\PipelinePattern\";

        public static void HardPersist()
        {
            JsonFileStore.Store(_profileStore.Values, path + "profiles.json");
            JsonFileStore.Store(_userStore.Values, path + "users.json");
            JsonFileStore.Store(_userAddressStore.Values, path + "userAddress.json");
            JsonFileStore.Store(_userAddresses, path + "userAddressMap.json");
            JsonFileStore.Store(_userRoles, path + "userRoles.json");
        }

        public static string AddUser(User user, CancellationToken token)
        {
            if (DALCancellationTokenHandler.handle(token))
                return null;
            user.Id = Guid.NewGuid().ToString();
            if (_userStore.TryAdd(user.Id, user))
                return user.Id;
            return null;
        }

        public static void AddUserAddress(string userId, Address[] addresses, CancellationToken token)
        {
            if (DALCancellationTokenHandler.handle(token))
                return;
            ValidateUser(userId);

            if (_userAddressStore.TryAdd(userId, addresses) && _userAddresses.TryAdd(userId, addresses.Select(x => x.Id).ToArray()))
                return;
            throw new ArgumentException("could not add the address for the user " + userId);
        }

        private static void ValidateUser(string userId)
        {
            if (!_userStore.ContainsKey(userId))
                throw new ArgumentException("User not found");
        }

        public static void AddUserRoles(string userId, Role[] roles, CancellationToken token)
        {
            if (DALCancellationTokenHandler.handle(token))
                return;
            ValidateUser(userId);

            if (_userRoles.TryAdd(userId, roles))
                return;
            throw new ArgumentException("could not add the roles for the user " + userId);
        }
    }

    public static class DALCancellationTokenHandler
    {
        public static Func<CancellationToken, bool> handle = (token) =>
        {
            return token.IsCancellationRequested;
        };
    }
}