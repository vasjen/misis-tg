using Microsoft.EntityFrameworkCore;
using misis_tg.Data;
using misis_tg.Extensions;
using misis_tg.Models;

namespace misis_tg.Services;

public class EnrolledService(AppDbContext db, ParserService parserService,  UrlsConfig urls)
{
    
    
    public async Task UpdateEnrolledAsync()
    {
        List<Education> educations = await db.Educations.ToListAsync();
        foreach (Education education in educations)
        {
            string url = GetEducationLink(education);
            List<EstimationResult?> students = await parserService.ParseAsync(url);
            await AddToDb(students, education);
        }
    }

    public async Task<EstimationResponseDto?> CheckStudentAsync(int registrationNumber)
    {
        var student = await db.Students
            .Include(s => s.Education)
            .FirstOrDefaultAsync(s => s.RegistrationNumber == registrationNumber);
        return student?.StudentInfoToDto();
    }
    
    private async Task AddToDb(List<EstimationResult?> students, Education education)
    {
        foreach (EstimationResult? student in students)
        {
            if (db.Students.Any(p => p.RegistrationNumber == student.RegistrationNumber))
            {
                Console.WriteLine($"Student with registration number {student.RegistrationNumber} already exists");
                continue;
            }

            if (student != null)
            {
                await db.Students.AddAsync(new Student
                {
                    Snils = student?.Snils,
                    RegistrationNumber = student.RegistrationNumber,
                    Education = education
                });
            }
        }
        await db.SaveChangesAsync();
    }
    private string GetEducationLink(Education education)
    {
        Console.WriteLine($"{education.Name} - {education.Format} - {education.BudgetType} - {education.Code}");
        if (urls.UrlList.ContainsKey(education.Format.ToString()))
        {
            var url = $"{urls.UrlList[education.Format.ToString()]}list/?id={education.Code}";
            return url;
        }
       
        throw new ArgumentException($"No such education in config {education.Format.ToString()}");
        
    }
}

