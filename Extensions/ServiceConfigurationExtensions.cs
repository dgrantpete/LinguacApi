namespace LinguacApi.Extensions
{
	public static class ServiceConfigurationExtensions
	{
		public static IServiceCollection AddConfiguration<TOptions>(
			this IServiceCollection services,
			string? configSectionPath = default
		) where TOptions : class
		{
			services.AddOptionsWithValidateOnStart<TOptions>()
				.BindConfiguration(configSectionPath ?? typeof(TOptions).Name)
				.ValidateDataAnnotations();

			return services;
		}

		public static IServiceCollection AddDynamicConfigurations(this IServiceCollection services, string configurationsNamespace)
		{
			var configurationsAssembly = typeof(ServiceConfigurationExtensions).Assembly;

			var configurationTypes = configurationsAssembly.GetTypes()
				.Where(type => type.IsClass && type.Namespace == configurationsNamespace);

			foreach (var configurationType in configurationTypes)
			{
				typeof(ServiceConfigurationExtensions).GetMethod(nameof(AddConfiguration))
					?.MakeGenericMethod(configurationType)
					?.Invoke(null, [services, null]);
			}

			return services;
		}
	}
}
