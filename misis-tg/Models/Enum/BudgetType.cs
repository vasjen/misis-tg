using System.ComponentModel;

namespace misis_tg.Models.Enum;

public enum BudgetType
{
    [Description("Бюджет")]
    Budget = 0, 
    [Description("Целевой прием")]
    Direction = 1, 
    [Description("Платно")]
    Paid = 2
}