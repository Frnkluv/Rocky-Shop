namespace Rocky.Models.ViewModels
{
    public class DetailsVM
    {
        public DetailsVM()
        {
            Product = new Product();    //когда эта VM понадобится, то уже будет инициализирован Продукт
        }

        public Product Product { get; set; }
        public bool ExistsInCart { get; set; }   //для осущ проверки при добавлении в корзину
    }
}
