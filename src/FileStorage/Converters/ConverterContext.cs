using FileStorage.Files;
using System;
using System.Collections.Generic;

namespace FileStorage.Converters
{
    public class ConverterContext
    {
        private readonly Dictionary<string, object> context = new Dictionary<string, object>();

        public IEnumerable<string> Parameters
        {
            get
            {
                return context.Keys;
            }
        }

        public bool HasParameter(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

            return context.ContainsKey(key);
        }

        public ConverterContext AddParameter(string key, object value)
        {
            if (string.IsNullOrEmpty(key) && HasParameter(key))
                return this;

            context.Add(key, value);

            return this;
        }

        public object GetParameter(string key)
        {
            if (context.ContainsKey(key))
                return context[key];
            else
                return null;
        }

        public bool TryGetParameter<T>(string key, out T value)
        {
            if (HasParameter(key) == false)
            {
                value = default(T);
                return false;
            }


            value = (T)context[key];

            return true;
        }
    }

    public static class ConverterContextExtensions
    {
        public static IEnumerable<FileMeta> GetAsFileMeta(this ConverterContext context)
        {
            var result = new HashSet<FileMeta>();

            foreach (var param in context.Parameters)
            {
                result.Add(new FileMeta(param, context.GetParameter(param).ToString()));
            }

            return result;
        }
    }
}
