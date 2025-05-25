using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ConsultasMedicasOnline.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());
            var attribute = field?.GetCustomAttribute<DisplayAttribute>();
            return attribute?.Name ?? enumValue.ToString();
        }
    }
}
