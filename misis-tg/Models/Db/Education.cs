using misis_tg.Models.Enum;

namespace misis_tg.Models;

public class Education
{
    public int Id { get; set; }
    public string Name { get; set; }
    public EducationFormat Format { get; set; }
    public string Code { get; set; }
    public BudgetType BudgetType { get; set; }
    
    //Navigation properties
    public List<Student> Students { get; set; } = [];
}