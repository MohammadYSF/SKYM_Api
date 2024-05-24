using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Net;

namespace SKYM_Api.Controllers
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
    }
    public class Menu
    {
        public int Id { get; set; }
        public int Type { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public int Count { get; set; }
    }
    public class Staff
    {
        public int Id { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }
        public int Salary { get; set; }
        public DateTime StartDate { get; set; }
        public int Type { get; set; }
    }
    public class Table
    {
        public int Id { get; set; }
        public int Status { get; set; }
        public int Capacity { get; set; }
    }
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly string _connectionString;
        public WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SKYM_DB");
            _logger = logger;
        }
        [HttpGet(Name = "GetMenus")]
        public async Task<IActionResult> GetMenus()
        {
            List<Menu> menus = [];
            using SqlConnection sqlConnection = new(_connectionString);
            using SqlCommand sqlCommand = new("SELECT * FROM Menu", sqlConnection);
            using SqlDataReader reader = sqlCommand.ExecuteReader();
            while (reader.Read())
            {
                menus.Add(new Menu
                {
                    Id = (int)reader["id"],
                    Name = reader["name"].ToString(),
                    Count = (int)reader["count"],
                    Price = (int)reader["price"],
                });
            }
            return Ok(menus);
        }



        [HttpGet(Name = "GetStaffs")]
        public async Task<IActionResult> GetStaffs()
        {
            List<Staff> staffs = [];
            using SqlConnection sqlConnection = new(_connectionString);
            using SqlCommand sqlCommand = new("SELECT * FROM Staff", sqlConnection);
            using SqlDataReader reader = sqlCommand.ExecuteReader();
            while (reader.Read())
            {
                staffs.Add(new Staff
                {
                    Id = (int)reader["id"],
                    FName = reader["fName"].ToString(),
                    LName = reader["lName"].ToString(),
                    Salary = (int)reader["salary"],
                    StartDate = (DateTime)reader["startDate"],
                    Type = (int)reader["type"],
                });
            }
            return Ok(staffs);
        }
        [HttpGet(Name = "GetCustomers")]

        public async Task<IActionResult> GetCustomers()
        {
            List<Customer> customers = [];
            using SqlConnection sqlConnection = new(_connectionString);
            using SqlCommand sqlCommand = new("SELECT * FROM Customer", sqlConnection);
            using SqlDataReader reader = sqlCommand.ExecuteReader();
            while (reader.Read())
            {
                customers.Add(new Customer
                {
                    Id = (int)reader["id"],
                    Name = reader["name"].ToString(),
                    PhoneNumber = reader["phoneNumber"].ToString()
                });
            }
            return Ok(customers);
        }





        /*
         
         CREATE procedure [dbo].[update_table_status](
 @order_id int 
)
         */
        [HttpGet(Name = "UpdateTableStatus")]

        public async Task<IActionResult> UpdateTableStatus(int orderId)
        {
            using SqlConnection sqlConnection = new(_connectionString);
            using SqlCommand sqlCommand = new SqlCommand("update_table_status", sqlConnection);
            sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
            sqlCommand.Parameters.AddWithValue("@orderId", orderId);
            sqlConnection.Open();
            await sqlCommand.ExecuteNonQueryAsync();
            return Ok();
        }









        /*
         
CREATE procedure [dbo].[start_order_for_existing_customer](
	@customer_id int,
	@table_id int,
	@waiter int,
	@order_type int,
	@address nvarchar(255)
)
         */
        public async Task<IActionResult> StartOrderForExistingCustomer(
            int customerId, int tableId, int waiter, int orderType, string address
            )
        {
            using SqlConnection sqlConnection = new(_connectionString);
            using SqlCommand sqlCommand = new SqlCommand("start_order_for_existing_customer", sqlConnection);
            sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
            sqlCommand.Parameters.AddWithValue("@customerId", customerId);
            sqlCommand.Parameters.AddWithValue("@tableId", tableId);
            sqlCommand.Parameters.AddWithValue("@waiter", waiter);
            sqlCommand.Parameters.AddWithValue("@orderType", orderType);
            sqlCommand.Parameters.AddWithValue("@address", address);
            sqlConnection.Open();
            await sqlCommand.ExecuteNonQueryAsync();
            return Ok();
        }







        /*
         	@order_id int,
	@menu_id int,
	@count int
         */
        [HttpGet(Name = "AddItemToOrder")]
        public async Task<IActionResult> AddItemToOrder(int orderId, int menuId, int count)
        {
            using SqlConnection sqlConnection = new(_connectionString);
            using SqlCommand sqlCommand = new SqlCommand("add_item_to_order", sqlConnection);
            sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
            sqlCommand.Parameters.AddWithValue("@orderId", orderId);
            sqlCommand.Parameters.AddWithValue("@menuId", menuId);
            sqlCommand.Parameters.AddWithValue("@count", count);
            sqlConnection.Open();
            await sqlCommand.ExecuteNonQueryAsync();
            return Ok();
        }


        [HttpGet(Name = "StartOrderForNewCustomer")]
        public async Task<IActionResult> StartOrderForNewCustomer(string customerName, string customerPhoneNumber,
            int orderType, int tableId, int waiter, string? address)
        {
            using SqlConnection sqlConnection = new(_connectionString);
            using SqlCommand sqlCommand = new SqlCommand("start_order_for_new_customer", sqlConnection);
            sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
            sqlCommand.Parameters.AddWithValue("@customerName", customerName);
            sqlCommand.Parameters.AddWithValue("@customerPhoneNumber", customerPhoneNumber);
            sqlCommand.Parameters.AddWithValue("@orderType", orderType);
            sqlCommand.Parameters.AddWithValue("@tableId", tableId);
            sqlCommand.Parameters.AddWithValue("@waiter", waiter);
            sqlCommand.Parameters.AddWithValue("@address", address);
            sqlConnection.Open();
            await sqlCommand.ExecuteNonQueryAsync();
            return Ok();

            /*
            exec start_order_for_new_customer 'ricky' , '02290100987',1,3,@JsonTaylor_StaffId,null; 

             	@customerName nvarchar(255),
	@customerPhoneNumber nchar(11),
	@orderType int,
	    --order types :
	--	1.in-person order 
	--  2.take-away order
	--	3.delivery order
	@table_id int,
	@waiter int,
	@address nvarchar(255) --@address will be used in the case when @orderType is delivery
             */
        }
    }
}
