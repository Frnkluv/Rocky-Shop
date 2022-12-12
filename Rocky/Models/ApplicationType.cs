using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Rocky.Models
{
    public class ApplicationType
    {
        [Key]
        public int Id { get; set; }     // если в названии есть "Id", то автомат первичный ключ, но его также можно указать вручную
        [Required]
        public string Name { get; set; }
    }
}
