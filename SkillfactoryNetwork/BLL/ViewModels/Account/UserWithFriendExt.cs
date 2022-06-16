using SkillfactoryNetwork.DAL.Models.Users;

namespace SkillfactoryNetwork.BLL.ViewModels.Account
{
    public class UserWithFriendExt : User
    {
        public bool IsFriendWithCurrent { get; set; }
    }
}
