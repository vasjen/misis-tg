namespace misis_tg.Models;

public class Student
{
    public int Id { get; set; }
    public string? Snils { get; set; }
    public int RegistrationNumber { get; set; }
    
    //Navigation properties
    public int EducationId { get; set; }
    public Education Education { get; set; }
}