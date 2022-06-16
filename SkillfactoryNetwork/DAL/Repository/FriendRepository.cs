using Microsoft.EntityFrameworkCore;
using SkillfactoryNetwork.DAL.Models.Users;
using SkillfactoryNetwork.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillfactoryNetwork.DAL.Repository
{
    public class FriendRepository : Repository<Friend>
    {
        public FriendRepository(ApplicationDbContext db) : base(db)
        {

        }

        public async Task AddFriend(User target, User Friend)
        {
            var friends = await Set.ToListAsync();
            var friend = friends.FirstOrDefault(x => x.UserId == target.Id && x.CurrentFriendId == Friend.Id);

            if (friend == null)
            {
                var item = new Friend()
                {
                    UserId = target.Id,
                    User = target,
                    CurrentFriend = Friend,
                    CurrentFriendId = Friend.Id,
                };

                await Create(item);
            }
        }

        public async Task<List<User>> GetFriendsByUser(User target)
        {
            var friends = await Set.Where(x => x.UserId == target.Id).Include(x => x.CurrentFriend).ToListAsync();
            var users = friends.Select(x => x.CurrentFriend);
            return users.ToList();
        }

        public async Task DeleteFriend(User target, User Friend)
        {
            var friends = await Set.ToListAsync();
            var friend = friends.FirstOrDefault(x => x.UserId == target.Id && x.CurrentFriendId == Friend.Id);

            if (friends != null)
            {
                await Delete(friend);
            }
        }

    }
}
