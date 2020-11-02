namespace Shared.Models
{
    public class LoginModel : Header
    {
        public LoginModel()
        {
            Target = InvocationTarget.Authorization;
        }

        public string Name { get; set; }
        public string Status { get; set; }
    }
}