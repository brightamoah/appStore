using System.ComponentModel.DataAnnotations;
using appStore.Server.Models;
using appStore.Server.Utils;

namespace appStore.Server.Dto.Priorities
{
	public record UpdatePriorityDto(
		[Required]
		[StringLength(50)]
		string Name
	)
	{
		public void PatchModel(Priority priority)
		{
			priority.Name = GeneralUtilities.ValidateAndNormalizeName( Name ?? priority.Name );
		}
	};
}