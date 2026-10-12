using appStore.Server.Models;

namespace appStore.Server.Dto.Priorities
{
    public record PriorityDto(
        int Id,
        string Name
    )
    {
        public static PriorityDto FromModel(Priority priority)
        {
            return new PriorityDto(
                priority.Id,
                priority.Name
            );
        }
    };
}