using System.ComponentModel;

namespace misis_tg.Models.Enum;

public enum EducationFormat
{   [Description("Бакалавриат и специалитет")]
    Bachelor = 0, 
    [Description("Магистратура")]
    Master = 1, 
    [Description("Аспирантура")]
    Postgraduate = 2
}