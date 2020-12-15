using System.ComponentModel;

namespace AutomationConnectIQ.Lib
{
    internal static partial class Utility
    {
        /// <summary>
        /// enumのDescriptionを文字変換する
        /// </summary>
        public static string GetDescription(this System.Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            if (System.Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute) {
                return attribute.Description;
            }
            else {
                return value.ToString();
            }
        }
    }
}
