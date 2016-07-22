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

namespace PipelinePattern.Services
{
    public class ProfileService
    {
        private static UserService userService = new UserService();
        private static AddressService addressService = new AddressService();
        private static RoleService roleService = new RoleService();

        public void AddProfile(string path, CancellationToken token)
        {
            DataValidator<ArgumentNullException>.Validate(path);

            int initialBufferSize = 32;
            var intialBuffer = new BlockingCollection<Profile>(initialBufferSize);
            var stream = new FileStream(path, FileMode.Open, FileAccess.Read);

            LoadImportData(intialBuffer, stream, token);

            var bufferForAddress = new BlockingCollection<Profile>(initialBufferSize);
            var bufferForRoles = new BlockingCollection<Tuple<string, Role[]>>(initialBufferSize);

            var f = new TaskFactory(TaskCreationOptions.LongRunning, TaskContinuationOptions.None);

            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(token))
            {
                try
                {
                    var userAddTask
                        = f.StartNew(() => ManageUser(intialBuffer, cts, bufferForAddress));
                    var addressAddTask
                        = f.StartNew(() => ManageAddresses(bufferForAddress, cts, bufferForRoles));
                    var rolesAddTask
                        = f.StartNew(() => ManageUserRoles(bufferForRoles, cts));

                    Task.WaitAll(userAddTask, addressAddTask, rolesAddTask);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                    cts.Cancel();
                    if (!(ex is OperationCanceledException)) throw;
                }
            }
        }

        private static void LoadImportData(BlockingCollection<Profile> intialBuffer, FileStream stream, CancellationToken token)
        {
            try
            {
                using (var reader = new StreamReader(stream))
                {
                    while (reader.Read() != 0)
                    {
                        if (reader.EndOfStream) break;
                        string profile = reader.ReadLine();

                        if (string.IsNullOrEmpty(profile) || profile == Environment.NewLine || profile.Trim().Length < 1) continue;

                        var userProfile = profile.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        //order is as follows,
                        // fn, ln, un, mail,city,st,zip,roleid
                        var objProfile = new Profile
                        {
                            User = new User
                            {
                                Email = userProfile[3],
                                FirstName = userProfile[0],
                                LastName = userProfile[1],
                                UserName = userProfile[2]
                            },
                            Address = new Address[]
                        {
                            new Address{
                                City = userProfile[4],
                                State = userProfile[5],
                                Zip = userProfile[6]
                            }
                        },
                            Roles = new Role[]{
                            new Role{
                                Id = userProfile[7]
                            }
                        }
                        };
                        intialBuffer.Add(objProfile, token);
                    }
                }
            }
            finally
            {
                intialBuffer.CompleteAdding();
            }
        }

        private static void ManageUserRoles(BlockingCollection<Tuple<string, Role[]>> rolesBuffer, CancellationTokenSource cts)
        {
            try
            {
                foreach (var userrole in rolesBuffer.GetConsumingEnumerable())
                {
                    if (cts.IsCancellationRequested) break;
                    roleService.AddRoles(userrole.Item1, userrole.Item2, cts.Token);
                }
            }
            catch (OperationCanceledException operationCanceledException)
            {
                Trace.WriteLine(operationCanceledException);
                Trace.WriteLine("Operation was cancelled");
            }
            catch (Exception ex)
            {
                cts.Cancel();
                Trace.WriteLine(ex);
            }
        }

        private static void ManageAddresses(BlockingCollection<Profile> addressBuffer, CancellationTokenSource cts, BlockingCollection<Tuple<string, Role[]>> bufferforRoles)
        {
            try
            {
                foreach (var address in addressBuffer.GetConsumingEnumerable())
                {
                    if (cts.IsCancellationRequested) break;
                    addressService.AddAddress(address.User.Id, address.Address, cts.Token);
                    bufferforRoles.Add(new Tuple<string, Role[]>(address.User.Id, address.Roles));
                }
            }
            catch (OperationCanceledException operationCanceledException)
            {
                Trace.WriteLine(operationCanceledException);
                Trace.WriteLine("Operation was cancelled");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                cts.Cancel();
            }
            finally
            {
                bufferforRoles.CompleteAdding();
            }
        }

        private static void ManageUser(BlockingCollection<Profile> profiles, CancellationTokenSource cts, BlockingCollection<Profile> addressBuffer)
        {
            try
            {
                if (cts.IsCancellationRequested) return;

                foreach (var profile in profiles.GetConsumingEnumerable())
                {
                    string userId = userService.AddUser(profile.User, cts.Token);
                    if (!string.IsNullOrEmpty(userId))
                    {
                        profile.User.Id = userId;
                        addressBuffer.Add(profile);
                    }
                }
            }
            catch (OperationCanceledException operationCanceledException)
            {
                Trace.WriteLine(operationCanceledException);
                Trace.WriteLine("Operation was cancelled");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                cts.Cancel();
            }
            finally
            {
                addressBuffer.CompleteAdding();
            }
        }
    }
}
