using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace ProjectHubAPI.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string receiverId, object messageData)
        {
            await Clients.Group(receiverId).SendAsync("ReceiveMessage", Context.UserIdentifier, messageData);
        }

        public async Task JoinChat(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }
    }
}
 
