namespace TR.Connector.Constants;

/// <summary>
/// Константы endpoint'ов API
/// </summary>
internal static class ApiEndpoints
{
    public const string IsServiceWorking = "api/v1/isServiceWorking";

    public const string Login = "api/v1/login";

    public const string UsersAll = "api/v1/users/all";
    public const string UserByLogin = "api/v1/users/{0}";
    public const string UserCreate = "api/v1/users/create";
    public const string UserEdit = "api/v1/users/edit";

    public const string RolesAll = "api/v1/roles/all";
    public const string UserRoles = "api/v1/users/{0}/roles";
    public const string AddRole = "api/v1/users/{0}/add/role/{1}";
    public const string DropRole = "api/v1/users/{0}/drop/role/{1}";

    public const string RightsAll = "api/v1/rights/all";
    public const string UserRights = "api/v1/users/{0}/rights";
    public const string AddRight = "api/v1/users/{0}/add/right/{1}";
    public const string DropRight = "api/v1/users/{0}/drop/right/{1}";

    public const string LockUser = "api/v1/users/{0}/lock";
    public const string UnlockUser = "api/v1/users/{0}/unlock";

    public static string GetUser(string login) => string.Format(UserByLogin, login);

    public static string GetUserRoles(string login) => string.Format(UserRoles, login);

    public static string GetUserRights(string login) => string.Format(UserRights, login);

    public static string GetLockUser(string login) => string.Format(LockUser, login);

    public static string GetUnlockUser(string login) => string.Format(UnlockUser, login);

    public static string GetAddRole(string login, string roleId) =>
        string.Format(AddRole, login, roleId);

    public static string GetDropRole(string login, string roleId) =>
        string.Format(DropRole, login, roleId);

    public static string GetAddRight(string login, string rightId) =>
        string.Format(AddRight, login, rightId);

    public static string GetDropRight(string login, string rightId) =>
        string.Format(DropRight, login, rightId);
}
