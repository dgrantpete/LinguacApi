using System.Text.RegularExpressions;

namespace LinguacApi.Extensions
{
	public partial class KebabCaseParameterTransformer : IOutboundParameterTransformer
	{
		private static readonly string[] prefixes = [ "Get", "Post", "Put", "Delete", "Patch" ];

		public string TransformOutbound(object? value)
		{
			if (value is null)
			{
				return string.Empty;
			}

			string valueString = value.ToString()!;

			valueString = prefixes.FirstOrDefault(prefix => valueString.StartsWith(prefix)) switch
			{
				string prefix => valueString[prefix.Length..],
				_ => valueString
			};

			return WordBreakMatcher().Replace(valueString, "$1-$2").ToLower();
		}

		[GeneratedRegex("([a-z])([A-Z])")]
		private static partial Regex WordBreakMatcher();
	}
}
