using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace SKYM_Api.Controllers;

public class OrderLog
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public DateTime HappeningDateTime { get; set; }
    public string Username { get; set; }
    public string? Description { get; set; }
    public OrderLogOperationType Type { get; set; }
}
public enum OrderLogOperationType
{
    NewRecord = 1,
    DeleteRecord = 2,
    UpdateRecord = 3
}

public class DayEarningReportModel
{
    public DateTime OrderDate { get; set; }
    public int Earning { get; set; }
}
public class TopCustomersReportModel
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; }
    public int CountOfOrders { get; set; }
    public int CustomerTotalSpending { get; set; }
}
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string PhoneNumber { get; set; }
}
public class OrderMenu
{
    public int OrderId { get; set; }
    public int MenuId { get; set; }
    public int Count { get; set; }
}
public class Menu
{
    public int Id { get; set; }
    public MenuType Type { get; set; }
    public string Name { get; set; }
    public int Price { get; set; }
    public int Count { get; set; }
}
public enum OrderType
{
    InPersonCustomerOrder = 1,
    TakeawayCustomerOrder = 2,
    DeliveryCustomerOrder = 3
}
public enum TableStatus
{
    Free = 1,
    Busy = 2
}
public enum MenuType
{
    Appetizer = 1,
    Entress = 2,
    Desserts = 3,
    Drinks = 4,
    SideDish = 5
}
public enum StaffType
{
    Waiter = 1
}
public class TodayOrder
{
    public DateTime OrderDate { get; set; }
    public OrderType OrderType { get; set; }
    public bool IsOrderFinished { get; set; }
    public int OrderId { get; set; }
    public string CustomerName { get; set; }
}

public class Staff
{
    public int Id { get; set; }
    public string FName { get; set; }
    public string LName { get; set; }
    public int Salary { get; set; }
    public DateTime StartDate { get; set; }
    public StaffType Type { get; set; }
}
public class Table
{
    public int Id { get; set; }
    public TableStatus Status { get; set; }
    public int Capacity { get; set; }
}
public class StartOrderForNewCustomerRequest
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; }
    public string CustomerPhoneNumber { get; set; }
    public int OrderType { get; set; }
    public int WaiterId { get; set; }
    public int TableId { get; set; }
    public string address { get; set; } = string.Empty;
}
[ApiController]
[Route("[action]")]
public class WeatherForecastController : ControllerBase
{
    private bool IsTheSameDay(DateTime date1, DateTime date2)
    {
        var x = (date1.Year == date2.Year && date1.DayOfYear == date2.DayOfYear);
        return x;
    }
    private readonly ILogger<WeatherForecastController> _logger;
    private readonly string _connectionString;
    public WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("SKYM_DB");
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTables()
    {
        List<Table> data = [];
        using SqlConnection sqlConnection = new(_connectionString);
        using SqlCommand sqlCommand = new("SELECT * FROM [Table]", sqlConnection);
        await sqlConnection.OpenAsync();
        using SqlDataReader reader = sqlCommand.ExecuteReader();
        while (reader.Read())
        {
            data.Add(new Table
            {
                Capacity = (int)reader["capacity"],
                Id = (int)reader["id"],
                Status = (TableStatus)(int)reader["status"]
            });
        }
        return Ok(data);
    }



    [HttpGet]
    public async Task<IActionResult> DayEarningReport()
    {
        List<DayEarningReportModel> data = [];
        using SqlConnection sqlConnection = new(_connectionString);
        using SqlCommand sqlCommand = new("SELECT * FROM [day_earning_report]", sqlConnection);
        sqlConnection.Open();
        using SqlDataReader reader = sqlCommand.ExecuteReader();
        while (reader.Read())
        {
            data.Add(new DayEarningReportModel
            {
                Earning = (int)reader["earning"],
                OrderDate = (DateTime)reader["orderDate"]
            });
        }
        return Ok(data);
    }
    [HttpGet]
    public async Task<IActionResult> TodayOrders()
    {
        List<TodayOrder> data = [];
        using SqlConnection sqlConnection = new(_connectionString);
        string query = """
            SELECT 
            [Order].[id] as orderId,
            [Order].[Finished] as isOrderFinished,
            [Order].orderDate as orderDate,
            [Customer].[name] as customerName,
            1 as orderType
            FROM [Order] inner join InPersonOrderCustomer on [Order].[id]=[InPersonOrderCustomer].order_id inner join Customer on [InPersonOrderCustomer].customer_id=[Customer].id
            union
            SELECT 
            [Order].[id] as orderId,
            [Order].[Finished] as isOrderFinished,
            [Order].orderDate as orderDate,                
            [Customer].[name] as customerName,
            2 as orderType                
            FROM [Order] inner join DeliveryCustomer on [Order].[id]=[DeliveryCustomer].order_id inner join Customer on [DeliveryCustomer].customer_id=[Customer].id                
            union
            SELECT 
            [Order].[id] as orderId,
            [Order].[Finished] as isOrderFinished,
            [Order].orderDate as orderDate,                
            [Customer].[name] as customerName,
            3 as orderType
            FROM [Order] inner join TakeAwayCustomer on [Order].[id]=[TakeAwayCustomer].order_id inner join Customer on [TakeAwayCustomer].customer_id=[Customer].id                                
            """;
        using SqlCommand sqlCommand = new(query, sqlConnection);
        sqlConnection.Open();
        using SqlDataReader reader = sqlCommand.ExecuteReader();
        while (reader.Read())
        {
            data.Add(new TodayOrder
            {
                OrderId = (int)reader["orderId"],
                CustomerName = reader["customerName"].ToString(),
                OrderType = (OrderType)(int)reader["orderType"],
                IsOrderFinished = (bool)reader["isOrderFinished"],
                OrderDate = (DateTime)reader["orderDate"]
            });
        }
        var ans = data
            //.Where(a => this.IsTheSameDay(DateTime.Now, a.OrderDate))
            .ToList();
        return Ok(ans);
    }
    [HttpGet]
    public async Task<IActionResult> TopCustomersReport()
    {
        List<TopCustomersReportModel> data = [];
        using SqlConnection sqlConnection = new(_connectionString);
        using SqlCommand sqlCommand = new("SELECT * FROM [top_customers_report]", sqlConnection);
        sqlConnection.Open();
        using SqlDataReader reader = sqlCommand.ExecuteReader();
        while (reader.Read())
        {
            data.Add(new TopCustomersReportModel
            {
                CustomerId = (int)reader["customer_id"],
                CustomerName = reader["customer_name"].ToString(),
                CountOfOrders = (int)reader["count_of_orders"],
                CustomerTotalSpending = (int)reader["customer_total_spending"],

            });
        }
        return Ok(data);
    }


    [HttpGet]
    public async Task<IActionResult> GetMenus()
    {
        List<Menu> menus = [];
        using SqlConnection sqlConnection = new(_connectionString);
        using SqlCommand sqlCommand = new("SELECT * FROM Menu", sqlConnection);
        sqlConnection.Open();
        using SqlDataReader reader = sqlCommand.ExecuteReader();
        while (reader.Read())
        {
            menus.Add(new Menu
            {
                Id = (int)reader["id"],
                Name = reader["name"].ToString(),
                Count = (int)reader["count"],
                Price = (int)reader["price"],
                Type = (MenuType)(int)reader["type"]
            });
        }
        return Ok(menus);
    }



    [HttpGet]
    public async Task<IActionResult> GetStaffs()
    {
        List<Staff> staffs = [];
        using SqlConnection sqlConnection = new(_connectionString);
        using SqlCommand sqlCommand = new("SELECT * FROM Staff", sqlConnection);
        sqlConnection.Open();

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
                Type = (StaffType)(int)reader["type"],
            });
        }
        return Ok(staffs);
    }
    [HttpGet]

    public async Task<IActionResult> GetCustomers()
    {
        List<Customer> customers = [];
        using SqlConnection sqlConnection = new(_connectionString);
        using SqlCommand sqlCommand = new("SELECT * FROM Customer", sqlConnection);
        sqlConnection.Open();

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
    //[HttpPost]

    //public async Task<IActionResult> UpdateTableStatus(int orderId)
    //{
    //    using SqlConnection sqlConnection = new(_connectionString);
    //    using SqlCommand sqlCommand = new SqlCommand("update_table_status", sqlConnection);
    //    sqlConnection.Open();

    //    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
    //    sqlCommand.Parameters.AddWithValue("@orderId", orderId);
    //    sqlConnection.Open();
    //    await sqlCommand.ExecuteNonQueryAsync();
    //    return Ok();
    //}



    [HttpPost]
    public async Task<IActionResult> EndOrder([FromQuery] int orderId)
    {
        using SqlConnection sqlConnection = new(_connectionString);
        using SqlCommand sqlCommand = new("end_order", sqlConnection);
        sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
        sqlCommand.Parameters.AddWithValue("@order_id", orderId);
        await sqlConnection.OpenAsync();
        await sqlCommand.ExecuteNonQueryAsync();
        return Ok();
    }





    [HttpPost]
    public async Task<IActionResult> CreateStaff(
        string fName, string lName, int salary, DateTime startDate, StaffType type)
    {
        using SqlConnection sqlConnection = new(_connectionString);
        using SqlCommand sqlCommand =
            new("INSERT INTO Staff([fName],[lName],[salary],[startDate],[type]) Values (@FName,LName,@Salari,@StartDate,@Type)");
        sqlCommand.Parameters.AddWithValue("@FName", fName);
        sqlCommand.Parameters.AddWithValue("@LName", lName);
        sqlCommand.Parameters.AddWithValue("@Salary", salary);
        sqlCommand.Parameters.AddWithValue("@StartDate", startDate);
        sqlCommand.Parameters.AddWithValue("@Type", type);
        await sqlConnection.OpenAsync();
        await sqlCommand.ExecuteNonQueryAsync();
        return Ok();
    }



    [HttpPost]
    public async Task<IActionResult> CreateMenu(
        MenuType type, string name, string ingrediants, int price, int count
        )
    {
        using SqlConnection sqlConnection = new(_connectionString);
        using SqlCommand sqlCommand = new("INSERT INTO Menu([type],[name],[ingrediants],[price],[count]) VALUES (@Type,@Name,@Ingrediants,@Price,@Count)");
        sqlCommand.Parameters.AddWithValue("@Type", (int)type);
        sqlCommand.Parameters.AddWithValue("@Name", name);
        sqlCommand.Parameters.AddWithValue("@Ingrediants", ingrediants);
        sqlCommand.Parameters.AddWithValue("@Price", price);
        sqlCommand.Parameters.AddWithValue("@Count", count);
        await sqlConnection.OpenAsync();
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
    [HttpPost]
    public async Task<IActionResult> StartOrderForExistingCustomer(
        int customerId, int tableId, int waiter, OrderType orderType, string address
        )
    {
        using SqlConnection sqlConnection = new(_connectionString);
        using SqlCommand sqlCommand = new("start_order_for_existing_customer", sqlConnection);
        sqlConnection.Open();

        sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
        sqlCommand.Parameters.AddWithValue("@customerId", customerId);
        sqlCommand.Parameters.AddWithValue("@tableId", tableId);
        sqlCommand.Parameters.AddWithValue("@waiter", waiter);
        sqlCommand.Parameters.AddWithValue("@orderType", (int)orderType);
        sqlCommand.Parameters.AddWithValue("@address", address);
        sqlCommand.Parameters.AddWithValue("@orderDate", DateTime.UtcNow);
        sqlConnection.Open();
        await sqlCommand.ExecuteNonQueryAsync();
        return Ok();
    }







    /*
     	@order_id int,
	@menu_id int,
	@count int
     */
    [HttpGet]
    public async Task<IActionResult> GetOrderMenuItems(int orderId)
    {
        List<OrderMenu> data = [];
        using SqlConnection sqlConnection = new(_connectionString);
        using SqlCommand sqlCommand = new("SELECT * FROM OrderMenu where order_id=@OrderId", sqlConnection);
        sqlCommand.Parameters.AddWithValue("@OrderId", orderId);
        await sqlConnection.OpenAsync();

        using SqlDataReader reader = sqlCommand.ExecuteReader();
        while (reader.Read())
        {
            data.Add(new OrderMenu
            {
                Count = (int)reader["count"],
                MenuId = (int)reader["menu_id"],
                OrderId = (int)reader["order_id"]
            });
        }
        return Ok(data);

    }
    [HttpPost]
    public async Task<IActionResult> AddItemToOrder(int orderId, int menuId, int count)
    {
        using SqlConnection sqlConnection = new(_connectionString);
        using SqlCommand sqlCommand = new("add_item_to_order", sqlConnection);
        sqlConnection.Open();

        sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
        sqlCommand.Parameters.AddWithValue("@order_id", orderId);
        sqlCommand.Parameters.AddWithValue("@menu_id", menuId);
        sqlCommand.Parameters.AddWithValue("@count", count);
        await sqlCommand.ExecuteNonQueryAsync();
        return Ok();
    }


    [HttpPost]
    public async Task<IActionResult> StartOrderForNewCustomer([FromBody] StartOrderForNewCustomerRequest request)
    {
        using SqlConnection sqlConnection = new(_connectionString);
        using SqlCommand sqlCommand = new("start_order_for_new_customer", sqlConnection);
        sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
        sqlCommand.Parameters.AddWithValue("@customerName", request.CustomerName);
        sqlCommand.Parameters.AddWithValue("@customerPhoneNumber", request.CustomerPhoneNumber);
        sqlCommand.Parameters.AddWithValue("@orderType", request.OrderType);
        sqlCommand.Parameters.AddWithValue("@table_id", request.TableId);
        sqlCommand.Parameters.AddWithValue("@waiter", request.WaiterId);
        sqlCommand.Parameters.AddWithValue("@address", request.address);
        sqlCommand.Parameters.AddWithValue("@orderDate", DateTime.UtcNow);
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



    [HttpGet]
    public async Task<IActionResult> GetOrderLogData()
    {
        List<OrderLog> data = [];
        using SqlConnection sqlConnection = new(_connectionString);
        using SqlCommand sqlCommand = new("SELECT * FROM OrderLog;", sqlConnection);
        await sqlConnection.OpenAsync();

        using SqlDataReader reader = sqlCommand.ExecuteReader();
        while (reader.Read())
        {
            data.Add(new OrderLog
            {
                Description = reader["description"].ToString(), 
                HappeningDateTime = (DateTime)reader["happeningDateTime"],
                Id = (int)reader["log_id"],
                OrderId = (int)reader["order_id"],
                Type = (OrderLogOperationType)(int)reader["type"],
                Username = reader["username"].ToString()
            });
        }
        return Ok(data);

    }
}
