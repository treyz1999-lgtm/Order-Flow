
using OrderFlow.Api.Models;
using OrderFlow.Api.DTOs;
using System.Security.AccessControl;
namespace OrderFlow.Api.Services
{
    public class ProductService
    {
        //Fields store the service's data
        //Methods provide operations on that data

        private readonly object prductLock = new object();

        private readonly List<Product> storedProducts = 
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

        public Product CreateProduct(CreateProductRequest creationRequest)
        {
            lock (prductLock) 
            { 
                int nextProductId = storedProducts.Count == 0 ? 1 : storedProducts.Max(candidateProduct => candidateProduct.Id) + 1; 


                Product createdProduct = new Product(
                    nextProductId,
                    creationRequest.Name,
                    creationRequest.Price,
                    creationRequest.StockQuantity
                    );

                storedProducts.Add(createdProduct);
                return createdProduct;
            }
        }

        
    }

}
