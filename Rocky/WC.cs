namespace Rocky
{
    // Web Constant
    public static class WC
    {
        public const string ImagePath = @"\images\product\";
        //добавляю ключ для доступа к сеансу:
        public const string SessionCart = "ShoppingCartSession";   //иду в Layout делаю проверку вкл ли сейчас сессия


        // для пеализации ролей пользователей, после иду ...(урок 5 секция 6)
        public const string AdminRole = "Admin";
        public const string CustomerRole = "Customer";

        public const string EmailAdmin = "egormedd52@gmail.com";
    }
}
