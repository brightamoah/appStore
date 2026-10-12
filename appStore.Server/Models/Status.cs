using System.Text.Json.Serialization;

namespace appStore.Server.Models
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum Status
	{
		NotStarted,
		InProgress,
		Completed,
		ReadyForTest,
		Closed,
		UnderReview,
		Cancelled,
		OnHold,
		Archived,
		Pending
	}
}