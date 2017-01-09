using System;
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
        private readonly Func<string, IDbConnection> _connectionFactory;
        private readonly string _connectionString;

        public ProductRepository() : this(CreateConnection) {}

        public ProductRepository(Func<string, IDbConnection> connectionFactory)
        {
            _connectionFactory = connectionFactory;
            _connectionString = @"Server=localhost;Database=dbname;Trusted_Connection=true"; //Get connection string here. Opportunity for DI

        }

        internal static IDbConnection CreateConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        public void Add(Product product)
        {
            using (var connection = _connectionFactory(_connectionString))
            {
                connection.Open();
                connection.Execute(Resource.InsertProduct, product);
            }
        }

        public IEnumerable<Product> GetAll()
        {
            using (var connection = _connectionFactory(_connectionString))
            {
                connection.Open();
                return connection.Query<Product>(Resource.SelectAllProducts);
            }
        }

        public Product GetByCrn(string crn)
        {
            using (var connection = _connectionFactory(_connectionString))
            {
                connection.Open();
                return connection.Query<Product>(Resource.SelectProductByCrn, new {Crn = crn}).FirstOrDefault();
            }
        }

        public void Delete(string crn)
        {
            using (var connection = _connectionFactory(_connectionString))
            {
                connection.Open();
                connection.Execute(Resource.DeleteProduct, new {Crn = crn});
            }
        }

        public void Update(Product product)
        {
            using (var connection = _connectionFactory(_connectionString))
            {
                connection.Open();
                connection.Query(Resource.UpdateProduct, product);
            }
        }
    }
}
