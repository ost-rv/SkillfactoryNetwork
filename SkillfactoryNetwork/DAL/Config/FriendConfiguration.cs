using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillfactoryNetwork.DAL.Models.Users;

namespace SkillfactoryNetwork.DAL.Config
{
    public class FriendConfiguration : IEntityTypeConfiguration<Friend> 
    {
        public void Configure(EntityTypeBuilder<Friend> builder)
        {
            builder.ToTable("UserFriends").HasKey(p => p.Id);
            builder.Property(x => x.Id).UseIdentityColumn();
        }
    }
}
