using KNTCommon.Business.DTOs;
using KNTCommon.Data.Models;

namespace KNTCommon.Business.Repositories
{
    public interface IUsersAndGroupsRepository
    {
        IEnumerable<UserDTO>? GetAllUsers();
        IEnumerable<UserDTO>? GetAllUsersFromASingleGroup(int groupId);
        User CreateUserReturnUser(User newUser, int? groupId);
        User UpdateUserReturnUser(User newUser, int? groupId);
        bool DeleteUser(UserDTO user);
        UserDTO GetUserById(int userId);
        UserDTO GetUserByUsername(string username);
        IEnumerable<UserGroupDTO> GetAllGroups();
        UserGroupDTO GetGroupById(int groupId);
        UserGroupDTO GetGroupByName(string username);
        bool CreateUserGroup(UserGroupDTO newGroup);
        bool DeleteGroup(UserGroupDTO group);
        void ChangeUserGroupsToDefaultOnGroupDelete(int groupId);
        bool CheckIfUsersExistdInGroup(int groupId);
    }
}