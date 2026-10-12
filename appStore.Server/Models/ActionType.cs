using System.Text.Json.Serialization;

namespace appStore.Server.Models
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum ActionType
	{
		StatusChanged,
		PriorityChanged,
		Assigned,
		Moved,
		CommentAdded
	}
}