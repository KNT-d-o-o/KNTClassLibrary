namespace KNTCommon.Business.Repositories
{
    public interface IAuthenticationRepository
    {
        bool Register(int userId, string insertedUsername, string insertedPassword, int? groupId, int? logout, bool insert);
        bool Login(string insertedUsername, string insertedPassword);
    }
}