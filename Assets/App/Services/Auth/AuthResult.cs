namespace Miyo.Services.Auth
{
    public struct AuthResult
    {
        public bool Success;
        public string ErrorMessage;

        public static AuthResult Successful() => new AuthResult { Success = true };

        public static AuthResult Failed(string error) => new AuthResult
        {
            Success = false,
            ErrorMessage = error
        };
    }
}
