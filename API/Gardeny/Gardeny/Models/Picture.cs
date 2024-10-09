using System.ComponentModel.DataAnnotations.Schema;

namespace Gardeny.Models
{
    public class Picture
    {
        public int PictureId { get; set; }
        public string Url { get; set; }
        public int PostId { get; set; }
        public Post Post { get; set; }
        

        // Additional properties for file storage
        [NotMapped] // Not mapped to the database
        public IFormFile? File { get; set; } // Represents the uploaded file

        public string FileName { get; set; } // Actual file name saved on the server
    }
}
