using OrderFlow.Api.Models;
namespace OrderFlow.Api.Services
{
    public class ProductService
    {
        //Fields store the service's data
        //Methods provide operations on that data
        private readonly Product[] storedProducts = 
            [
            new Product( 1, "Notebook", 4.99m, 25),
            new Product( 2, "Pen", 1.50m, 100)
            ];

        public IEnumerable<Product> GetProducts()
        {
            return storedProducts;
        }

        // Service: returns data or null
        public Product? GetProduct(int id)
        {
            Product? searchProduct = storedProducts.FirstOrDefault(p => p.Id == id);
            return searchProduct;
        }
    }

}
