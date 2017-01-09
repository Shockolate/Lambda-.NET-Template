using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Implementation.Model;

namespace Implementation.Repository
{
    public class ProductRepository
    {
        private readonly string _connectionString;

        public ProductRepository()
        {
            _connectionString = @"Server=localhost;Database=dbname;Trusted_Connection=true"; //Get connection string
        }

        internal IDbConnection Connection
        {
            get
            {
                return new SqlConnection(_connectionString);
            }
        }

        public void Add(Product product)
        {
            using (var connection = Connection)
            {
                connection.Open();
                connection.Execute(Resource.InsertProduct, product);
            }
        }

        public IEnumerable<Product> GetAll()
        {
            using (var connection = Connection)
            {
                connection.Open();
                return connection.Query<Product>(Resource.SelectAllProducts);
            }
        }

        public Product GetByCrn(string crn)
        {
            using (var connection = Connection)
            {
                connection.Open();
                return connection.Query<Product>(Resource.SelectProductByCrn, new {Crn = crn}).FirstOrDefault();
            }
        }

        public void Delete(string crn)
        {
            using (var connection = Connection)
            {
                connection.Open();
                connection.Execute(Resource.DeleteProduct, new {Crn = crn});
            }
        }

        public void Update(Product product)
        {
            using (var connection = Connection)
            {
                connection.Open();
                connection.Query(Resource.UpdateProduct, product);
            }
        }
    }
}
