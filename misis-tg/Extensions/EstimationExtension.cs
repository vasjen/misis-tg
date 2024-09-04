using misis_tg.Models;

namespace misis_tg.Extensions;

public static class EstimationExtension
{
    public static EstimationResponseDto StudentInfoToDto (this Student student)
    {
        return new EstimationResponseDto(student.RegistrationNumber, student.Snils, student.Education.BudgetType.GetDescription(), student.Education.Format.GetDescription(), student.Education.Name);
    }
    
    public static string FormatEstimationResponse(this EstimationResponseDto dto)
    {
        return $@"
<b>Детали зачисления:</b>
<pre>
Регистрационный номер: {dto.RegistrationNumber}
СНИЛС: {dto.Snils ?? "Не указан"}
Форма обучения: {dto.BudgetType}
Уровень образования: {dto.Format}
Название направления: {dto.EducationName}
</pre>";
    }

}