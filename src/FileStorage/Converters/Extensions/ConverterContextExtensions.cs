using System;

namespace FileStorage.Converters.Extensions
{
    public static class ConverterContextExtensions
    {
        public static T ParseEnum<T>(this ConverterContext context, string key)
        {
            var param = context.GetParameter(key);

            if (ReferenceEquals(null, param) == false)
                return (T)Enum.Parse(typeof(T), param.ToString(), true);
            else
            {
                return default(T);
            }
        }
    }
}


