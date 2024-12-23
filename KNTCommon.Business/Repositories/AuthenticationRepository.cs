using KNTToolsAndAccessories;
using KNTCommon.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KNTCommon.Business.Repositories
{
    public class AuthenticationRepository : IAuthenticationRepository
    {
        private readonly IDbContextFactory<EdnKntControllerMysqlContext> ContextFactory;
        private readonly IEncryption Encryption;
        private readonly IUsersAndGroupsRepository UsersRepository;
        private readonly Tools t = new();

        public AuthenticationRepository(IDbContextFactory<EdnKntControllerMysqlContext> factory, IEncryption encryption, IUsersAndGroupsRepository usersRepository)
        {
            ContextFactory = factory;
            //Log = logger;
            Encryption = encryption;
            UsersRepository = usersRepository;
        }

        private User? GetUserByUsername(string username)
        {
            var user = new User();
            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    user = context.Users.FirstOrDefault(x => x.UserName == username);
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.Business.Repositories.AuthenticationRepository #1 " + ex.Message);
            }
            return user;
        }

        //private bool VerifyPasswordWithUser(string hashedPasswordFromDatabase, string enteredPassword, User user)
        //{
        //    // Verify the entered password against the stored hashed password
        //    //var result = Hasher.VerifyHashedPassword(user, hashedPasswordFromDatabase, enteredPassword);
        //    //return result == PasswordVerificationResult.Success;
        //}

        /// <summary>
        /// Use this method on registering a new user
        /// </summary>
        /// <param name="insertedUsername"></param>
        /// <param name="insertedPassword"></param>
        public bool Register(int userId, string insertedUsername, string insertedPassword, int? groupId, int? logout, bool insert)
        {
            try
            {
                int uid = 0;
                if (!insert)
                    uid = userId;
                var iv = Encryption.GenerateRandomIV();
                // getawaiter getresult blocks the current thread until the task is not completed
                var encryptedPassword = Encryption.Encrypt(insertedPassword, iv).GetAwaiter().GetResult();

                var user = new User
                {
                    UserId = uid,
                    UserName = insertedUsername,
                    PasswordHash = encryptedPassword,
                    InitializationVector = iv,
                    logout = logout
                };

                if (insert)
                    UsersRepository.CreateUserReturnUser(user, groupId);
                else
                    UsersRepository.UpdateUserReturnUser(user, groupId);

                if (groupId != null)
                {

                }
                else
                {

                }

                return true;
            }
            catch(Exception ex)
            {
                t.LogEvent("KNTCommon.Business.Repositories.AuthenticationRepository #2 " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Use this method on login
        /// </summary>
        /// <param name="insertedUsername"></param>
        /// <param name="insertedPassword"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool Login(string insertedUsername, string insertedPassword)
        {
            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    var user = context.Users.FirstOrDefault(x => x.UserName == insertedUsername);
                    if (user == null || user.PasswordHash == null || user.InitializationVector == null)
                    {
                        return false;
                    }

                    var password = Encryption.Decrypt(user.PasswordHash, user.InitializationVector).GetAwaiter().GetResult();

                    if (password.Contains("<DT-Sum>"))
                    {
                        string[] passwdPart = password.Split("<DT-Sum>");
                        password = passwdPart[0] + (Convert.ToInt32(DateTime.Now.Year) + Convert.ToInt32(DateTime.Now.Month) + Convert.ToInt32(DateTime.Now.Day)).ToString() + passwdPart[1];
                    }

                    if (password == insertedPassword)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch(Exception ex)
            {
                t.LogEvent("KNTCommon.Business.Repositories.AuthenticationRepository #3 " + ex.Message);
                return false;
            }
        }
    }
}
