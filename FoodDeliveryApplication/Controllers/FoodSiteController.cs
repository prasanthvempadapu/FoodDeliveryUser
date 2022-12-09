﻿
using FoodDeliveryApplication.Models;
using FoodDeliveryApplication.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Data.SqlClient;


namespace FoodDeliveryApplication.Controllers
{
    
    public class FoodSiteController : Controller
    {

        public List<SignUp> userList = new List<SignUp>();
        public string NotExist = "User Not Exist";
        static string CurrUser;
        public List<FoodItems> FoodItemsSelected =  new List<FoodItems>();
        private readonly ILogger<FoodSiteController> _logger;


        public FoodSiteController(ILogger<FoodSiteController> logger)
        {
            _logger = logger;

            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433;Initial Catalog = FoodDeliveryApplication;Integrated Security=False;User ID=admin;Password=surya1997;");
            SqlCommand cmd = new SqlCommand("select * from Users", conn);
            conn.Open();
            SqlDataReader sr = cmd.ExecuteReader();
            while (sr.Read())
            {
                SignUp user = new SignUp(sr["UserName"].ToString(), sr["Email"].ToString(), sr["Password"].ToString());
                userList.Add(user);
            }
        }
        public IActionResult Index()
        {
    
            return View();
        }


        public IActionResult CreateAccount()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateAccount(SignUp signup)
        {
                var user = userList.Find(e => e.UserName == signup.UserName);
                if (user != null)
                {
                    ViewBag.UserName = "UserName already Exist";
                        _logger.LogInformation("User:{0} already Exist, unable to create new Account", signup.UserName);
                    return View();
                }

                SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
                SqlCommand cmd = new SqlCommand(String.Format("insert into Users values('{0}','{1}','{2}')", signup.UserName, signup.Email, signup.Password), conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Log.Information(String.Format("A new Account Created with UserName {0}", signup.UserName));



            


            _logger.LogInformation(String.Format("A new Account Created with UserName {0}", signup.UserName));
            

            return View("AccountCreated");

        }

        public IActionResult Login()
        {
                        _logger.LogInformation("Login Triggered");
            return View();
        }
        
        
        [HttpPost]
        public IActionResult Login(LoginDetails login)
        {
           
                var user = userList.Find(e => e.UserName == login.UserName);
                if (user == null)
                {
                    ViewBag.NotExist = NotExist;
                           _logger.LogInformation("User does not Exist");
                return View();
                }
                foreach (var i in userList)
                {
                    if (i.UserName == login.UserName && i.Password == login.Password)
                    {
                    //Log.Information(String.Format("{0} Logged in", login.UserName));
                    _logger.LogInformation(String.Format("{0} Logged in", login.UserName));
                    CurrUser = i.UserName;
                    HttpContext.Session.SetString("UserName", login.UserName);
                        return RedirectToAction("Restaurants");
                    }
                }

                ViewBag.IncorrectPassword = "Incorrect Password";
                 _logger.LogError("Invalid User Details Entered");

            return View();

        }

        public IActionResult Logout()
        {

            //Log.Information(String.Format("{0} Logged out", HttpContext.Session.GetString("UserName")));
            _logger.LogInformation("{0} Logged Out", HttpContext.Session.GetString("UserName"));
            return View("Login");
        }

     

        public IActionResult Menu()
        {
            
            return View();
        }

       
        public IActionResult Restaurants()
        {
            if (HttpContext.Session.GetString("UserName") == null)
            {
                _logger.LogInformation("{0} Logged Out", CurrUser);
                return RedirectToAction("Login");
            }
            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997; ");
            SqlCommand cmd = new SqlCommand("select * from Restaurants", conn);
            conn.Open();
            SqlDataReader sr = cmd.ExecuteReader();
            List<Restaurants> res = new List<Restaurants>();
            while(sr.Read())
            {
               Restaurants restaurant = new Restaurants((int)sr["Restaurant_Id"], sr["Restaurant_Name"].ToString(), sr["Restaurant_Image"].ToString());
                res.Add(restaurant);
            }
           
            return View("Restaurants",res);
        }




        public IActionResult RestaurantMenu(int Id)
        {
            if (HttpContext.Session.GetString("UserName") == null)
            {
                _logger.LogInformation("{0} Logged Out", CurrUser);
                return RedirectToAction("Login");
            }
            Console.WriteLine("ResId" + Id);

            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand cmd = new SqlCommand(String.Format("select *  from Food where Restaurant_Id={0}", Id), conn);
            conn.Open();
            SqlDataReader sr = cmd.ExecuteReader();


            //string str;
            HttpContext.Session.SetInt32("ResId", Id);
         
            var model = new List<Menu>();

            while (sr.Read())
            {
                Menu menu = new Menu((int)sr["Id"], sr["Food_Image"].ToString(), sr["Food_Item"].ToString(), (int)sr["Price"], (int)sr["Restaurant_Id"]);
                int id = (int)sr["Id"];
                Console.WriteLine(id);
                model.Add(menu);
            }
           
           

           

            return View("Menu", model);
        }



        public IActionResult Order()
        {
            return Content(HttpContext.Session.GetString("UserName"));

        }

        [HttpPost]
        public IActionResult AddToCart(IFormCollection col)
        {

            if (HttpContext.Session.GetString("UserName") == null)
            {
                _logger.LogInformation("{0} Logged Out", CurrUser);
                return RedirectToAction("Login");
            }
            Console.WriteLine(col["Food_Item"]);
            //Console.WriteLine("Food Id :" +col["Food_Id"]);
            var Food_Item = col["Food_Item"];
            var Quantity = col["Quantity"];
            var Restaurant_Id = col["RestaurantId"];
            var Food_Id = Convert.ToInt32(col["Food_Id"]);
            var Price = Convert.ToInt32(col["Price"]);

            _logger.LogInformation("Item:{0} added to cart by the user:{1} of Quantity:{2}", Food_Item, HttpContext.Session.GetString("UserName"), Quantity);


            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand cmd = new SqlCommand(String.Format("insert into AddItemToCart values('{0}','{1}','{2}','{3}','{4}')", HttpContext.Session.GetString("UserName"), Food_Item, Quantity,Restaurant_Id,Price), conn);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();

            //Tempdata["success"] = "Item Added to Cart";
            TempData["success"] = "Item Added to Cart";

            return RedirectToAction("RestaurantMenu",new { Id = Restaurant_Id });

          

        }

       
        public IActionResult DeleteItemFromCart(/*IFormCollection col,*/int Id)
        {/*
            string FoodItem = col["FoodItem"];

            int ItemNo = Convert.ToInt32(col["ItemNo"]);*/

          /*  Console.WriteLine("FoodItem = " + FoodItem);*/
           
            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand cmd = new SqlCommand(String.Format("delete from AddItemToCart where ItemNo = '{0}'", Id), conn);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();

            TempData["success"] = "Item Removed From Cart";
            return RedirectToAction("Cart");
        }


        public IActionResult Cart()
        {
            if(HttpContext.Session.GetString("UserName")==null)
            {
                _logger.LogInformation("{0} Logged Out", CurrUser);
                return RedirectToAction("Login");
            }
            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand cmd = new SqlCommand(String.Format("select A.FoodItem, A.Quantity,A.ItemNo, F.Food_Image,F.Price,F.Id,F.Restaurant_Id from AddItemToCart A inner join Food F on F.Food_Item = A.FoodItem where A.UserName = '{0}'", HttpContext.Session.GetString("UserName")), conn);
            conn.Open();
            SqlDataReader sr = cmd.ExecuteReader();

            List<Cart> cart = new List<Cart>();

            while(sr.Read())
            {
                Cart cartItem = new Cart(HttpContext.Session.GetString("UserName"), sr["FoodItem"].ToString(), (int)sr["Quantity"], sr["Food_Image"].ToString(), (int)sr["Price"], (int)sr["Id"], (int)sr["Restaurant_Id"], (int)sr["ItemNo"]);
                cart.Add(cartItem);
            }
            if(cart.Count == 0)
            {
                ViewBag.Cart = "Empty";
            }


            return View("Cart",cart);
        }

        [HttpPost]
        public IActionResult PlaceOrder(CustomerDetails details)
        {
            if (HttpContext.Session.GetString("UserName") == null)
            {
                _logger.LogInformation("{0} Logged Out", CurrUser);
                return RedirectToAction("Login");
            }
           

            

            Random rnd = new Random();
            int inVoiceNo = rnd.Next(10000, 10000000);

            DateTime OrderTime = DateTime.Now;
        

            string address = details.Address;
            string phoneNo = details.PhoneNo;

            Console.WriteLine(OrderTime);

            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand cmd = new SqlCommand(String.Format("insert into Orders values('{0}','{1}','{2}','{3}','{4}')",inVoiceNo, HttpContext.Session.GetString("UserName"),address,phoneNo,OrderTime.ToString("yyyy-MM-dd HH:mm:ss")), conn);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();

           

            SqlConnection conn1 = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand sqlcmd = new SqlCommand(String.Format("select * from AddItemToCart where UserName = '{0}'", HttpContext.Session.GetString("UserName")), conn);
            conn.Open();
            SqlDataReader sr = sqlcmd.ExecuteReader();

            var orderList = new List<OrderDetails>();
            while (sr.Read())
            {
                OrderDetails orderDetails = new OrderDetails(inVoiceNo, sr["UserName"].ToString(), (int)sr["RestaurantId"], sr["FoodItem"].ToString(), (int)sr["Quantity"], (int)sr["Price"],OrderTime);
                orderList.Add(orderDetails);
            }
            conn.Close();

            string items = "";
            string resId = "";



            foreach (var obj in orderList)
            {
                SqlCommand sqlCommand = new SqlCommand(String.Format("insert into PlacedOrderDetail values('{0}','{1}','{2}','{3}','{4}','{5}','{6}')",obj.InVoiceNo,obj.UserName,obj.RestaurantId,obj.FoodItem,obj.Quantity,obj.Price,obj.OrderTime.ToString("yyyy-MM-dd HH:mm:ss")), conn);
                items += obj.FoodItem + ",";
                resId = obj.RestaurantId.ToString();
                conn.Open();
                sqlCommand.ExecuteNonQuery();
                conn.Close();
            }


            _logger.LogDebug(String.Format("Order placed by user {0} of Items: {1} from restaurant Id : {2}", HttpContext.Session.GetString("UserName"), items, resId));

            SqlCommand cmd1 = new SqlCommand(String.Format("delete from AddItemToCart where UserName = '{0}'", HttpContext.Session.GetString("UserName")), conn);
            conn.Open();
            cmd1.ExecuteNonQuery();
            conn.Close();


            return View();
        
        }

        public IActionResult CustomerDetails()
        {
            return View();
        }

        public IActionResult CancelOrder()
        {
            if (HttpContext.Session.GetString("UserName") == null)
            {
                return RedirectToAction("Login");
            }
            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand cmd = new SqlCommand(String.Format("select * from ConfirmOrder where UserName = '{0}'", HttpContext.Session.GetString("UserName")), conn);
            conn.Open();
            SqlDataReader sr = cmd.ExecuteReader();
            List<Order> CancelOrderList = new List<Order>();
            while(sr.Read())
            {
                Order order = new Order(sr["UserName"].ToString(), sr["FoodItem"].ToString(),(int)sr["Price"], (int)sr["Quantity"], (int)sr["RestaurantId"]);
                CancelOrderList.Add(order);
            }
            conn.Close();

           foreach(var obj in CancelOrderList)
            {
                SqlCommand cmd1 = new SqlCommand(String.Format("insert into CancelOrder values('{0}','{1}','{2}','{3}','{4}')", obj.UserName,obj.FoodItem,obj.Price,obj.Quantity,obj.RestaurantId), conn);
                conn.Open();
                cmd1.ExecuteNonQuery();
                conn.Close();
            }

            SqlCommand cmd2 = new SqlCommand(String.Format("delete from ConfirmOrder where UserName = '{0}'", HttpContext.Session.GetString("UserName")), conn);
            conn.Open();
            cmd2.ExecuteNonQuery();
            conn.Close();

            return View();



        }

        public IActionResult OrderStatus()
        {
            return View();
        }

        public IActionResult Status(int Id)
        {
            Console.WriteLine("Id : " + Id);
            if (HttpContext.Session.GetString("UserName") == null)
            {
                _logger.LogInformation("{0} Logged Out", CurrUser);
                return RedirectToAction("Login");
            }

            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");

            ViewBag.ObjectPassed = "True";
            var OrderList = new List<Order>();
            var orderlist = new List<OrderDetails>();

            

            if (Id == 1)
            {
               

                ViewBag.ObjectPassed = "Pending";
                SqlCommand cmd = new SqlCommand(String.Format("Select * from PlacedOrderDetail where UserName = '{0}'", HttpContext.Session.GetString("UserName")), conn);
                conn.Open();
                SqlDataReader sr = cmd.ExecuteReader();

                while (sr.Read())
                {
                    string time = sr["OrderTime"].ToString();
                    DateTime orderTime = Convert.ToDateTime(time);
                    OrderDetails orderDetails = new OrderDetails((int)sr["InVoiceNo"], sr["UserName"].ToString(), (int)sr["RestaurantId"], sr["FoodItem"].ToString(), (int)sr["Quantity"], (int)sr["Price"],orderTime);
                    orderlist.Add(orderDetails);
                }
                conn.Close();
                return View("OrderStatus",orderlist);
            }

            /*if(Id==2)
            {
                SqlCommand cmd2 = new SqlCommand(String.Format("Select * from CancelOrder where UserName = '{0}'", HttpContext.Session.GetString("UserName")), conn);
                conn.Open();
                SqlDataReader sr2 = cmd2.ExecuteReader();


                while (sr2.Read())
                {
                    Order order = new Order(sr2["FoodItem"].ToString(), (int)sr2["Price"], (int)sr2["Quantity"], "Cancelled");
                    OrderList.Add(order);
                }
                conn.Close();
                return View("OrderStatus", OrderList);

            }*/

            if(Id==3)
            {
                ViewBag.ObjectPassed = "Completed";
                SqlCommand cmd1 = new SqlCommand(String.Format("Select * from CompletedOrder where UserName = '{0}'", HttpContext.Session.GetString("UserName")), conn);
                conn.Open();
                SqlDataReader sr1 = cmd1.ExecuteReader();
                while (sr1.Read())
                {
                    OrderDetails order = new OrderDetails((int)sr1["InVoiceNo"], sr1["UserName"].ToString(),sr1["FoodItem"].ToString(), (int)sr1["Quantity"], (int)sr1["Price"], (DateTime)sr1["OrderCompletionTime"]);
                    orderlist.Add(order);
                }
                conn.Close();
                return View("OrderStatus", orderlist);

            }

           


            return View("OrderStatus", OrderList);


        }


    }
}
