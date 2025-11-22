
namespace Domain.Environment
{
    public class InventarioApiSetting
    {
        public String URL { get; set; } = "";
        public ApiCredential Credential { get; set; } = new();

        public class ApiCredential
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }
    }
}
