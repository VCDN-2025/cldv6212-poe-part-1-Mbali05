using System.ComponentModel.DataAnnotations;

namespace AzureApp.Models
{
    public class Images
    {
        [Key]
        public int ImageID { get; set; }
        public string FileName { get; set; }
        public string BlobURL { get; set; }

    }
}
