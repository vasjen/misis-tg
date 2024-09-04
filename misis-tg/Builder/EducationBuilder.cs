using Microsoft.EntityFrameworkCore;
using misis_tg.Data;
using misis_tg.Models;
using misis_tg.Services;

namespace misis_tg.Builder;

public class EducationBuilder(AppDbContext db, ILogger<EducationBuilder> logger, ParserService parserService)
{
    private List<Direction> directions = new();

    public  EducationBuilder WithBachelor()
    {
        directions.AddRange(parserService.ParseBachelorAsync().GetAwaiter().GetResult());
        return this;
    }

    public EducationBuilder WithMaster()
    {
        directions.AddRange(parserService.ParseMasterAsync().GetAwaiter().GetResult());
        return this;
    }
    public EducationBuilder WithPostgraduate()
    {
        directions.AddRange(parserService.ParsePostgraduateAsync().GetAwaiter().GetResult());
        return this;
    }

    public async Task InitAsync()
    {
        int counter = 0;
        foreach (Direction direction in directions)
        {
            Education education = new();
            education.Name = direction.Name;
            education.Format = direction.Format;
            education.Code = direction.Code;
            education.BudgetType = direction.BudgetType;
            if (await db.Educations.AnyAsync(e => e.Code == education.Code))
            {
                logger.LogInformation($"{++counter} Already in db => {education.Name} {education.Code} {education.BudgetType} {education.Format}.");
                continue;
            }
            await db.Educations.AddAsync(education);
            logger.LogInformation($"{++counter} Added to db => {education.Name} {education.Code} {education.BudgetType} {education.Format}.");
        }

        await db.SaveChangesAsync();
    }
}
