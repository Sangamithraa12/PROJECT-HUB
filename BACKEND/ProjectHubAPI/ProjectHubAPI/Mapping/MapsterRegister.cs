using Mapster;
using ProjectHubAPI.DTOs;
using ProjectHubAPI.Models;

namespace ProjectHubAPI.Mapping
{
    public class MapsterRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<TaskItem, TaskDto>()
                .Map(dest => dest.ProjectName, src => src.Project != null ? src.Project.Name : null)
                .Map(dest => dest.AssignedToName, src => src.AssignedUser != null ? src.AssignedUser.Name : "Unassigned");

            config.NewConfig<Comment, CommentDto>()
                .Map(dest => dest.UserName, src => src.User != null ? src.User.Name : "Anonymous");
        }
    }
}
 
