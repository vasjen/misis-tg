using Microsoft.EntityFrameworkCore;
using misis_tg.Builder;
using misis_tg.Data;

namespace misis_tg.Services;

public class InitialService(EducationBuilder educationBuilder, AppDbContext db)
{
    
    public async Task InitDb()
    {
        await db.Database.MigrateAsync();
        if (!db.Educations.Any())
        {
            await educationBuilder
                    .WithBachelor()
                    .WithMaster()
                    .WithPostgraduate()
                    .InitAsync();
        }
    }
}