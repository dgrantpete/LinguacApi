using Microsoft.AspNetCore.Mvc;

namespace LinguacApi.Data.Binders
{
	public class AuthenticatedUserAttribute() : ModelBinderAttribute(typeof(AuthenticatedUserBinder))
	{
	}
}
