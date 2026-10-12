using appStore.Server.Models;
using appStore.Server.Utils;

using System.ComponentModel.DataAnnotations;

namespace appStore.Server.Dto.Roles
{
	public record UpdateRoleDto(
		[StringLength(100)]
		string? Name,

		[StringLength(256)]
		string? Description
	)
	{
		public void PatchModel( Role role )
		{
			role.Name = GeneralUtilities.ValidateAndNormalizeName( Name ?? role.Name );
			role.Description = Description ?? role.Description;
		}
	};
}