using Masa.BuildingBlocks.Ddd.Domain.Entities.Full;

namespace LearnEnglish.Authentication.Entities
{
    public class User : FullAggregateRoot<Guid,Guid>
    {
        public User()
        {
           
        }
    }
}
