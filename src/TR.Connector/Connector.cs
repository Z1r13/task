using TR.Connector.Configurations;
using TR.Connector.Constants;
using TR.Connector.DTOs;
using TR.Connector.DTOs.Requests;
using TR.Connector.DTOs.Responses;
using TR.Connector.Http;
using TR.Connectors.Api.Entities;
using TR.Connectors.Api.Interfaces;

namespace TR.Connector
{
    public class Connector : IConnector
    {
        public ILogger Logger { get; set; }

        private HttpClient _httpClient;
        private ApiClient _apiClient;

        //Пустой конструктор
        public Connector() { }

        public void StartUp(string connectionString)
        {
            try
            {
                Logger?.Debug("Initializing connector...");
                var config = ConnectionConfig.Parse(connectionString);
                // config.
                _httpClient = new HttpClient();

                _apiClient = new ApiClient(_httpClient, Logger);

                var bearerToken = Authenticate(config);

                _apiClient.Configure(config.Url, bearerToken);

                Logger?.Debug("Connector initialized successfully");
            }
            catch (Exception e)
            {
                Logger?.Error($"Failed to initialize connector: {e.Message}");
                throw;
            }
        }

        private string Authenticate(ConnectionConfig config)
        {
            var authClient = new ApiClient(_httpClient, Logger);
            authClient.Configure(config.Url, null);

            var request = new LoginRequest { Login = config.Login, Password = config.Password };

            var response = authClient
                .PostAsync<TokenResponse>(ApiEndpoints.Login, request)
                .GetAwaiter()
                .GetResult();

            return response.Data.AccessToken;
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            try
            {
                Logger?.Debug("Getting all permisions...");

                var roles = _apiClient
                    .GetAsync<RoleListResponse>(ApiEndpoints.RolesAll)
                    .GetAwaiter()
                    .GetResult();

                var rights = _apiClient
                    .GetAsync<RightListResponse>(ApiEndpoints.RightsAll)
                    .GetAwaiter()
                    .GetResult();

                var rolePermissions = roles.Data.Select(x => new Permission(
                    $"{PermissionTypes.ItRole},{x.Id}",
                    x.Name,
                    x.CorporatePhoneNumber
                ));

                var rightPermissions = rights.Data.Select(x => new Permission(
                    $"{PermissionTypes.RequestRight},{x.Id}",
                    x.Name,
                    string.Empty
                ));

                return rolePermissions.Concat(rightPermissions);
            }
            catch (Exception e)
            {
                Logger?.Error($"Cannot get all permisiions: {e.Message}");
                throw;
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            try
            {
                Logger?.Debug($"Getting user {userLogin} permissions...");

                var roles = _apiClient
                    .GetAsync<RoleListResponse>(ApiEndpoints.GetUserRoles(userLogin))
                    .GetAwaiter()
                    .GetResult();

                var rights = _apiClient
                    .GetAsync<RightListResponse>(ApiEndpoints.GetUserRights(userLogin))
                    .GetAwaiter()
                    .GetResult();

                var userRoles = roles.Data.Select(x => $"{PermissionTypes.ItRole},{x.Id}");
                var userRights = rights.Data.Select(x => $"{PermissionTypes.RequestRight},{x.Id}");

                return userRoles.Concat(userRights).ToList();
            }
            catch (Exception e)
            {
                Logger?.Error($"Cannot get user {userLogin} permissions: {e.Message}");
                throw;
            }
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {
                Logger?.Debug($"Adding permissions to user {userLogin}...");

                var userResponse = _apiClient
                    .GetAsync<UserResponse>(ApiEndpoints.GetUser(userLogin))
                    .GetAwaiter()
                    .GetResult();

                if (userResponse.Data.Status == "Lock")
                {
                    Logger?.Error($"User {userLogin} locked");
                    return;
                }

                foreach (var permissionId in rightIds)
                {
                    var part = permissionId.Split(",");
                    if (part.Length != 2)
                        continue;

                    var type = part[0];
                    var id = part[1];

                    if (type == PermissionTypes.ItRole)
                    {
                        _apiClient
                            .PutAsync(ApiEndpoints.GetAddRole(userLogin, id))
                            .GetAwaiter()
                            .GetResult();
                    }
                    else if (type == PermissionTypes.RequestRight)
                    {
                        _apiClient
                            .PutAsync(ApiEndpoints.GetAddRight(userLogin, id))
                            .GetAwaiter()
                            .GetResult();
                    }
                }

                Logger?.Debug($"Users {userLogin} permissions successfully added");
            }
            catch (Exception e)
            {
                Logger?.Error($"Cannot add permissions to user {userLogin}: {e.Message}");
                throw;
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {
                Logger?.Debug($"Removing users {userLogin} permissions...");

                var userResponse = _apiClient
                    .GetAsync<UserResponse>(ApiEndpoints.GetUser(userLogin))
                    .GetAwaiter()
                    .GetResult();

                if (userResponse.Data.Status == "Lock")
                {
                    Logger?.Error($"User {userLogin} locked");
                    return;
                }

                foreach (var permissionId in rightIds)
                {
                    var part = permissionId.Split(",");
                    if (part.Length != 2)
                        continue;

                    var type = part[0];
                    var id = part[1];

                    if (type == PermissionTypes.ItRole)
                    {
                        _apiClient
                            .DeleteAsync(ApiEndpoints.GetDropRole(userLogin, id))
                            .GetAwaiter()
                            .GetResult();
                    }
                    else if (type == PermissionTypes.RequestRight)
                    {
                        _apiClient
                            .DeleteAsync(ApiEndpoints.GetDropRight(userLogin, id))
                            .GetAwaiter()
                            .GetResult();
                    }
                }

                Logger?.Debug($"Users {userLogin} permissions successfully removed");
            }
            catch (Exception e)
            {
                Logger?.Error($"Cannot remove permissions to user {userLogin}: {e.Message}");
                throw;
            }
        }

        // TODO ?
        public IEnumerable<Property> GetAllProperties()
        {
            Logger?.Debug("Getting all properties metadata...");

            return new List<Property>
            {
                new Property("lastName", "Last Name"),
                new Property("firstName", "First Name"),
                new Property("middleName", "Middle Name"),
                new Property("telephoneNumber", "Telephone Number"),
                new Property("isLead", "Is Lead"),
            };
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            try
            {
                Logger?.Debug($"Getting user {userLogin} properties...");

                var userResponse = _apiClient
                    .GetAsync<UserPropertyResponse>(ApiEndpoints.GetUser(userLogin))
                    .GetAwaiter()
                    .GetResult();

                if (userResponse.Data.Status == "Lock")
                {
                    Logger?.Error($"User {userLogin} lock");
                    throw new Exception($"User {userLogin} locked");
                }

                var userPropertyResponse = _apiClient
                    .GetAsync<UserPropertyResponse>(ApiEndpoints.GetUser(userLogin))
                    .GetAwaiter()
                    .GetResult();

                var user = userPropertyResponse.Data;

                return new List<UserProperty>
                {
                    new UserProperty("lastName", user.LastName),
                    new UserProperty("firstName", user.FirstName),
                    new UserProperty("middleName", user.MiddleName),
                    new UserProperty("telephoneNumber", user.TelephoneNumber),
                    new UserProperty("isLead", user.IsLead.ToString()),
                };
            }
            catch (Exception e)
            {
                Logger?.Error($"Cannot get user {userLogin} properties: {e.Message}");
                throw;
            }
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            try
            {
                Logger?.Debug($"Updating user {userLogin} properties...");

                var userResponse = _apiClient
                    .GetAsync<UserPropertyResponse>(ApiEndpoints.GetUser(userLogin))
                    .GetAwaiter()
                    .GetResult();

                var user = userResponse.Data;
                if (user.Status == "Lock")
                {
                    Logger?.Error($"User {userLogin} locked");
                    return;
                }

                var updateRequest = new CreateUserRequest
                {
                    Login = user.Login,
                    Status = user.Status,
                    LastName = user.LastName,
                    FirstName = user.FirstName,
                    MiddleName = user.MiddleName,
                    TelephoneNumber = user.TelephoneNumber,
                    IsLead = user.IsLead,
                };

                foreach (var prop in properties)
                {
                    switch (prop.Name)
                    {
                        case "lastName":
                            updateRequest.LastName = prop.Value;
                            break;
                        case "firstName":
                            updateRequest.FirstName = prop.Value;
                            break;
                        case "middleName":
                            updateRequest.MiddleName = prop.Value;
                            break;
                        case "telephoneNumber":
                            updateRequest.TelephoneNumber = prop.Value;
                            break;
                        case "isLead":
                            if (bool.TryParse(prop.Value, out var IsLead))
                                updateRequest.IsLead = IsLead;
                            break;
                    }
                }

                _apiClient.PutAsync(ApiEndpoints.UserEdit, updateRequest).GetAwaiter().GetResult();

                Logger?.Debug($"users {userLogin} properties updates successfully");
            }
            catch (Exception e)
            {
                Logger?.Error($"Cannot update user {userLogin} properties: {e.Message}");
                throw;
            }
        }

        public bool IsUserExists(string userLogin)
        {
            try
            {
                Logger?.Debug($"Checking if user {userLogin} exist...");

                var userResponce = _apiClient
                    .GetAsync<UserResponse>(ApiEndpoints.GetUser(userLogin))
                    .GetAwaiter()
                    .GetResult();

                return userResponce.Data != null;
            }
            catch
            {
                return false;
            }
        }

        public void CreateUser(UserToCreate user)
        {
            try
            {
                Logger?.Debug($"Creating user {user.Login}...");

                var request = new CreateUserRequest
                {
                    Login = user.Login,
                    Password = user.HashPassword,
                    LastName = GetPropValue(user.Properties, "lastName"),
                    FirstName = GetPropValue(user.Properties, "firstName"),
                    MiddleName = GetPropValue(user.Properties, "middleName"),
                    TelephoneNumber = GetPropValue(user.Properties, "telephoneNumber"),
                    IsLead =
                        bool.TryParse(GetPropValue(user.Properties, "isLead"), out var isLead)
                        && isLead,
                    Status = "Unlock",
                };

                _apiClient
                    .PostAsync<ApiResponse>(ApiEndpoints.UserCreate, request)
                    .GetAwaiter()
                    .GetResult();

                Logger?.Debug($"User {user.Login} created successfully");
            }
            catch (Exception e)
            {
                Logger?.Error($"Cannot create user {user.Login}: {e.Message}");
                throw;
            }
        }

        private string GetPropValue(IEnumerable<UserProperty> properties, string name)
        {
            return properties
                    ?.FirstOrDefault(x =>
                        string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)
                    )
                    ?.Value
                ?? string.Empty;
        }
    }
}
