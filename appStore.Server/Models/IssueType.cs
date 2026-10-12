using System.Text.Json.Serialization;

namespace appStore.Server.Models
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum IssueType
	{
		Task,
		Bug,
		Story,
		Epic
	}
}