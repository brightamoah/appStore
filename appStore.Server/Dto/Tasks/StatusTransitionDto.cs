using System.ComponentModel.DataAnnotations;
using appStore.Server.Models;

namespace appStore.Server.Dto.Tasks
{
    public record StatusTransitionDto(
        [Required] 
        Status TargetStatus
    );
}