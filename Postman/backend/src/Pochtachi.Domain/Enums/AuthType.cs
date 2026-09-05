namespace Pochtachi.Domain.Enums;

public enum AuthType
{
    NoAuth = 0,
    Basic = 1,
    Bearer = 2,
    ApiKey = 3,
    OAuth2 = 4,
    Digest = 5
}
