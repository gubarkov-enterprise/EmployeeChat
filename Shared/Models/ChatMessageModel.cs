namespace Shared.Models
{
    public class ChatMessageModel : Header
    {
        public ChatMessageModel()
        {
            Target = InvocationTarget.ChatMessage;
        }

        public string Content { get; set; }
    }
}