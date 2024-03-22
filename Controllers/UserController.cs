using LinguacApi.Data.Binders;
using LinguacApi.Data.Dtos;
using LinguacApi.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinguacApi.Controllers
{
	[Authorize(Roles = "user")]
	[ApiController]
	[Route("[controller]")]
	public class UserController : ControllerBase
	{
		[HttpGet]
		public ActionResult<UserInfoDto> Get([AuthenticatedUser] User user)
		{
			return Ok(new UserInfoDto(user.Roles) { Email = user.Email });
		}
	}
}
