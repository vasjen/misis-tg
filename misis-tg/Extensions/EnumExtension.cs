using System.ComponentModel;
using System.Reflection;

namespace misis_tg.Extensions;

public static class EnumExtension
{
    public static string GetDescription(this Enum value)
    { 
        FieldInfo field = value.GetType().GetField(value.ToString()); 
        if (field != null)
        {
                DescriptionAttribute? attribute = (DescriptionAttribute)field.GetCustomAttribute(typeof(DescriptionAttribute))!;
                return attribute.Description;
        } 
        
        return value.ToString();
    }
}