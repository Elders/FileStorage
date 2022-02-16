using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace OptionsExtensions
{
    public abstract class OptionsProviderBase<TOptions> :
         IConfigureOptions<TOptions>,
         IOptionsChangeTokenSource<TOptions>,
         IOptionsFactory<TOptions>,
         IPostConfigureOptions<TOptions>
         where TOptions : class, new()
    {
        protected IConfiguration configuration;

        public OptionsProviderBase(IConfiguration configuration, string name)
        {
            this.configuration = configuration;
            this.Name = name;
        }

        public OptionsProviderBase(IConfiguration configuration)
            : this(configuration, Options.DefaultName)
        { }

        public string Name { get; private set; }

        public abstract void Configure(TOptions options);

        public TOptions Create(string name)
        {
            var newOptions = new TOptions();
            Configure(newOptions);
            ValidateByDataAnnotation(newOptions);
            return newOptions;
        }

        public IChangeToken GetChangeToken() => configuration.GetReloadToken();

        public virtual void PostConfigure(string name, TOptions options) { }

        private void ValidateByDataAnnotation(object instance)
        {
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(instance);
            var valid = Validator.TryValidateObject(instance, context, validationResults, true);
            if (valid) return;

            var msg = string.Join("\n", validationResults.Select(r => r.ErrorMessage));
            throw new Exception($"Invalid configuration!':\n{msg}");
        }
    }
}
