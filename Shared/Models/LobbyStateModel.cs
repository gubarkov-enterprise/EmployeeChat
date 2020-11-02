using System.Collections.Generic;

namespace Shared.Models
{
    public class LobbyStateModel : Header
    {
        public LobbyStateModel()
        {
            Target = InvocationTarget.StateHasChanged;
        }

        public List<ChatMember> Members = new List<ChatMember>();
    }

    public class ChatMember
    {
        public string MemberName { get; set; }
        public string MemberStatus { get; set; }
    }
}