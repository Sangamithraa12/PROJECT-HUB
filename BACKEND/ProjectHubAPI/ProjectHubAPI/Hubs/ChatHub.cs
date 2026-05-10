using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace ProjectHubAPI.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string receiverId, object messageData)
        {
            // Send message data (including potential file info) to a specific user group
            await Clients.Group(receiverId).SendAsync("ReceiveMessage", Context.UserIdentifier, messageData);
        }

        public async Task JoinChat(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }
    }
}
