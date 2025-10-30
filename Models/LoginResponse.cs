using System.Text.Json.Serialization;
namespace Luley_Integracion_Net.Models;

public class LoginResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("response")]
    public LoginData Response { get; set; } = new();

    [JsonPropertyName("bypass_rollback")]
    public bool BypassRollback { get; set; }
}

public class LoginData
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;
}
