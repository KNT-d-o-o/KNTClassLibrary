using AutoMapper;
using KNTToolsAndAccessories;
using KNTCommon.Business.DTOs;
using KNTCommon.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KNTCommon.Business.Repositories
{
    public class UsersAndGroupsRepository : IUsersAndGroupsRepository
    {
        private readonly IDbContextFactory<EdnKntControllerMysqlContext> Factory;
        private readonly IMapper AutoMapper;
        private readonly Tools t = new();

        public UsersAndGroupsRepository(IDbContextFactory<EdnKntControllerMysqlContext> factory, IMapper automapper)
        {
            Factory = factory;
            AutoMapper = automapper;
        }

        public IEnumerable<UserDTO>? GetAllUsers()
        {
            var ret = new List<UserDTO>();
            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    ret = AutoMapper.Map<List<UserDTO>>(context.Users.Where(x => x.UserId != 0 || x.UserId != 1 || x.UserId != 2).ToList());
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.Business.Repositories.UsersAndGroupsRepository #1 " + ex.Message);
            }
            return ret; 
        }

        public IEnumerable<UserDTO>? GetAllUsersFromASingleGroup(int groupId)
        {
            var users = new List<UserDTO>();
            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    users = AutoMapper.Map<List<UserDTO>>(context.Users.Where(u => u.GroupId == groupId).ToList());
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.Business.Repositories.UsersAndGroupsRepository #2 " + ex.Message);
            }
            return users;
        }

        public User CreateUserReturnUser(User newUser, int? groupId)
        {
            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    newUser.GroupId = groupId;
                    context.Users.Add(newUser);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.Business.Repositories.UsersAndGroupsRepository #3 " + ex.Message);
            }
            return newUser;
        }

        public User UpdateUserReturnUser(User chengedUser, int? groupId)
        {
            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    var prevUser = context.Users.Find(chengedUser.UserId);
                    if (prevUser != null)
                    {
                        prevUser.UserName = chengedUser.UserName;
                        prevUser.PasswordHash = chengedUser.PasswordHash;
                        prevUser.InitializationVector = chengedUser.InitializationVector;
                        prevUser.logout = chengedUser.logout;
                        prevUser.GroupId = groupId;

                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.Business.Repositories.UsersAndGroupsRepository #12 " + ex.Message);
            }
            return chengedUser;
        }

        public bool DeleteUser(UserDTO user)
        {
            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    var u = context.Users.First(x => x.UserId == user.UserId);
                    context.Remove(u);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.Business.Repositories.UsersAndGroupsRepository #13 " + ex.Message);
                return false;
            }
            return true;
        }

        public UserDTO GetUserById(int userId)
        {
            UserDTO user = new UserDTO();
            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    user = AutoMapper.Map<UserDTO>(context.Users.Where(x => x.UserId == userId).First());
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.Business.Repositories.UsersAndGroupsRepository #5 " + ex.Message);
            }
            return user;
        }

        public IEnumerable<UserGroupDTO> GetAllGroups()
        {
              var ret = new List<UserGroupDTO>();
              try
              {
                  using (var context = new EdnKntControllerMysqlContext())
                  {
                      ret = AutoMapper.Map<List<UserGroupDTO>>(context.UserGroups.Where(x => x.GroupId != 0 && x.GroupId != 1 && x.GroupId != 2 && x.GroupId != 3));
                  }
              }
              catch (Exception ex)
              {
                  t.LogEvent("KNTCommon.Business.Repositories.UsersAndGroupsRepository #6 " + ex.Message);
              }
              return ret;
        }

        public bool CreateUserGroup(UserGroupDTO newGroup)
        {

            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    context.UserGroups.Add(AutoMapper.Map<UserGroup>(newGroup));
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.Business.Repositories.UsersAndGroupsRepository #8 " + ex.Message);
                return false;
            }
            return true;
        }

        public bool DeleteGroup(UserGroupDTO group)
        {
            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    var g = context.UserGroups.First(x => x.GroupId == group.GroupId);
                    context.Remove(g);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.Business.Repositories.UsersAndGroupsRepository #9 " + ex.Message);
                return false;
            }
            return true;
        }

        public UserGroupDTO GetGroupById(int groupId)
        {
            var ret = new UserGroupDTO();
            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    return AutoMapper.Map<UserGroupDTO>(context.UserGroups.First(x => x.GroupId == groupId));
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.Business.Repositories.UsersAndGroupsRepository #10 " + ex.Message);
            }
            return ret;
        }

        public void ChangeUserGroupsToDefaultOnGroupDelete(int groupId)
        {
            throw new NotImplementedException();
        }

        public UserDTO GetUserByUsername(string username)
        {
            var user = new UserDTO();
            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    user = AutoMapper.Map<UserDTO>(context.Users.Where(x => x.UserName == username).FirstOrDefault());
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.Business.Repositories.UsersAndGroupsRepository #11 " + ex.Message);
            }
            return user;
        }

        public UserGroupDTO GetGroupByName(string name)
        {
            var group = new UserGroupDTO();
            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    group = AutoMapper.Map<UserGroupDTO>(context.UserGroups.Where(x => x.GroupName == name).FirstOrDefault());
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.Business.Repositories.UsersAndGroupsRepository #12 " + ex.Message);
            }
            return group;
        }

        public bool CheckIfUsersExistdInGroup(int groupId)
        {
            var ret = false;
            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    var records = AutoMapper.Map<UserDTO>(context.Users.Where(x => x.GroupId == groupId).FirstOrDefault());
                    if (records != null)
                        ret = true;
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.Business.Repositories.UsersAndGroupsRepository #14 " + ex.Message);
            }

            return ret;
        }
    }
}
