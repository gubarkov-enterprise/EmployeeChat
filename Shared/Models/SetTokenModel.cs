namespace Shared.Models
{
    public class SetTokenModel : Header
    {
        public SetTokenModel()
        {
            Target = InvocationTarget.SetToken;
        }
    }
}