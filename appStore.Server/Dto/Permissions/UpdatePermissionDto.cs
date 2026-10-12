using appStore.Server.Models;
using appStore.Server.Utils;

using System.ComponentModel.DataAnnotations;

namespace appStore.Server.Dto.Permissions
{
	public record UpdatePermissionDto(
		[StringLength(50)]
		string? Name,

		[StringLength(256)]
		string? Description
	)
	{
		public void PatchModel( Permission permission )
		{
			if (Name is not null)
			{
				permission.Name = GeneralUtilities.ValidateAndNormalizeName( Name );
			}

			if (Description is not null)
			{
				permission.Description = Description.Trim();
			}
		}
	}
}