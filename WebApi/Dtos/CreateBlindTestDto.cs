using System.Collections.Generic;
namespace WebApi.Dtos
{
    public class CreateBlindTestDto
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public string AdminKey { get; set; }
        public List<TrackSelectionDto> Tracks { get; set; }
        
    }
}
