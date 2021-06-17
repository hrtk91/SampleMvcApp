namespace SampleMvcApp.ViewModels.Product
{
    public class CreateViewModel
    {
        public int ShopId { get; set; }
        public Models.Product Product { get; set; }

        public CreateViewModel(int shopId, Models.Product product = null)
        {
            ShopId = shopId;
            Product = product;
        }
    }
}
