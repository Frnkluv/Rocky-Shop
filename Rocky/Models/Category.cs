using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Rocky.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }     // если в названии есть "Id", то автомат первичный ключ, но его также можно указать вручную
        [Required]
        public string Name { get; set; }
        [DisplayName("Display Order")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Display Order for category must be greather than 0")]   //Выводит кастомное смс об ошибке (вместо дефолта)
        public int DisplayOrder { get; set; }

        /* Для валиадации (обяз. заполнении полей) на стороне клиента 
         * добавил в представлении Create секцию скрипта*/
    }
}
