namespace Pochtachi.Application.Common;

/// <summary>Type: NoAuth | Basic | Bearer | ApiKey | OAuth2 | Digest. Values — turga qarab
/// (Bearer: token; Basic: username,password; ApiKey: key,value,addTo=header|query).</summary>
public record AuthDto(string Type, Dictionary<string, string> Values);
