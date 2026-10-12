using System.Text.Json.Serialization;

namespace appStore.Server.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ProjectRole
    {
        None = 0,
        Owner = 1,
        Admin = 2,
        Member = 3,
        Viewer = 4
    }
}