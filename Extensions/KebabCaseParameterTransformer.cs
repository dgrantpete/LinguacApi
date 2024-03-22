using System.Text.RegularExpressions;

namespace LinguacApi.Extensions
{
	public partial class KebabCaseParameterTransformer : IOutboundParameterTransformer
	{
		public string TransformOutbound(object? value)
		{
			if (value is null)
			{
				return string.Empty;
			}

			return WordBreakMatcher().Replace(value.ToString()!, "$1-$2").ToLower();
		}

		[GeneratedRegex("([a-z])([A-Z])")]
		private static partial Regex WordBreakMatcher();
	}
}
