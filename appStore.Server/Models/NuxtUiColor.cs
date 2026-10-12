using System.Text.Json.Serialization;

namespace appStore.Server.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum NuxtUiColor
    {
        Primary,
        Secondary,
        Success,
        Info,
        Warning,
        Error,
        Neutral
    }
}