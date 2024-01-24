using Microsoft.EntityFrameworkCore;

namespace LearnEnglish.Authentication
{
    public class LearnEnglishAuthenitcationContext: MasaDbContext<LearnEnglishAuthenitcationContext>
    {
        public LearnEnglishAuthenitcationContext(MasaDbContextOptions<LearnEnglishAuthenitcationContext> options):base(options)
        {
            
        }
    }
}
