using MVCProject1.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using static System.Collections.Specialized.BitVector32;


namespace MVCProject1.Controllers
{
    public class HomeController : Controller
    {
        SqlConnection _connection = new SqlConnection("Data Source=DESKTOP-QOQA5FS;Initial Catalog=MVC1;Integrated Security=True");

        public ActionResult Index()
        {
            if (Session["user_id"] != null)
            {
                TempData["msg"] = "Please logout to access this page!";
                return RedirectToAction("UserProfile");
            }
            return View();
        }

        public ActionResult Login()
        {
            if (Session["user_id"] != null)
            {
                TempData["msg"] = "Please logout to access this page!";
                return RedirectToAction("UserProfile");
            }
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginStudent login)
        {
            if (Session["user_id"] != null)
            {
                TempData["msg"] = "Please logout to access this page!";
                return RedirectToAction("UserProfile");
            }
            if (ModelState.IsValid)
            {
                string checkEmail = $"SELECT id,email,password,count from Student where email = '{login.email}'";
                _connection.Open();
                SqlCommand cmd = new SqlCommand(checkEmail, _connection);
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    if (login.password == dr["password"].ToString())
                    {
                        string user_id = dr["id"].ToString();
                        Session["user_id"] = user_id;
                        if (dr["count"].ToString() == "0")
                        {
                            string updateCount = $"update Student set count = 1 where id={user_id}";
                            _connection.Close();
                            _connection.Open();
                            new SqlCommand(updateCount, _connection).ExecuteNonQuery();
                            return RedirectToAction("UpdateProfile");
                        }
                        else
                        {
                            return RedirectToAction("UserProfile");
                        }
                    }
                    else
                    {
                        ViewBag.msg = "Password is wrong";
                    }
                }
                else
                {
                    ViewBag.msg = "Email is wrong";
                }
            }
            return View();
        }
        public ActionResult Register()
        {
            if (Session["user_id"] != null)
            {
                TempData["msg"] = "Please logout to access this page!";
                return RedirectToAction("UserProfile");
            }
            return View();
        }
        [HttpPost]
        public ActionResult Register(RegStudent register)
        {
            if (ModelState.IsValid)
            {
                string checkEmail = $"SELECT email from Student where email = '{register.email}'";
                _connection.Open();
                SqlCommand cmd = new SqlCommand(checkEmail, _connection);

                if (!cmd.ExecuteReader().Read())
                {  
                    _connection.Close();


                    string query = $"INSERT INTO Student(name,email,password,count) VALUES('{register.name}','{register.email}','{register.password}','0')";
                    _connection.Open();
                    SqlCommand cmd2 = new SqlCommand(query, _connection);
                    cmd2.ExecuteNonQuery();
                    TempData["msg"] = "registeration successfully";
                    
                    _connection.Close();

                    /* Session["email"] = register.email;
                     string email = Session["email"].ToString();
                     string selectquery = $"select id from student where email = '{email}'";
                     _connection.Open();
                     SqlCommand cmd3 = new SqlCommand(selectquery, _connection);
                     string id = cmd3.ExecuteScalar().ToString();
                     if (id != null)
                     {
                         Session["user_id"] = id;
                         return RedirectToAction("UserProfile");
                     }
                     _connection.Close();*/
                    return RedirectToAction("Login");
                }
                else
                {
                    ModelState.AddModelError("email", "Email Already exist");
                }
            }
            return View();
        }

        public ActionResult UserProfile(Student st)
        {
            if (Session["user_id"] == null)
            {
                TempData["msg"] = "Please Login to access this page!";
                return RedirectToAction("Login");
            }
            string id = Session["user_id"].ToString();
            _connection.Open();
            SqlCommand cmd = new SqlCommand($"select * from Student where id={id}", _connection);
            SqlDataReader dr = cmd.ExecuteReader();

            dr.Read();


             st = new Student() {
                id = dr["id"].ToString(),
                name = dr["name"].ToString(),
                email = dr["email"].ToString(),
                password = dr["password"].ToString(),
                phone = dr["phone"].ToString(),
                dob = dr["dob"].ToString(),
                image = dr["image"].ToString(),
                address = dr["address"].ToString(),
                };
            
            return View(st);
        }

        [HttpPost]
        public ActionResult UserProfile()
        {
            Session.Abandon();
            return RedirectToAction("Index");
        }

        public ActionResult UpdateProfile()
        {
            if (Session["user_id"] == null)
            {
                TempData["msg"] = "Please Login to access this page!";
                return RedirectToAction("Login");
            }
            string user_id = Session["user_id"].ToString();
            _connection.Open();
            SqlCommand cmd = new SqlCommand($"select * from Student where id={user_id}", _connection);
            SqlDataReader dr = cmd.ExecuteReader();
            dr.Read();
            Student student = new Student()
            {
                id = dr["id"].ToString(),
                name = dr["name"].ToString(),
                email = dr["email"].ToString(),
                password = dr["password"].ToString(),
                phone = dr["phone"].ToString(),
                dob = dr["dob"].ToString(),
                image = dr["image"].ToString(),
                address = dr["address"].ToString(),
            };
            return View(student);


        }

        [HttpPost]
        public ActionResult UpdateProfile(UpdateStudent updateStudent)
        {
            if (Session["user_id"] == null)
            {
                TempData["msg"] = "Please Login to access this page!";
                return RedirectToAction("Login");
            }
            string query = $"UPDATE Student SET ";
            List<string> upd = new List<string>();

            string user_id = Session["user_id"].ToString();
            if (updateStudent.name != null)
            {
                string name = updateStudent.name;
                upd.Add($"Name='{name}'");
            }
            if (updateStudent.email != null)
            {
                string email = updateStudent.email;
                upd.Add($"Email='{email}'");
            }
            if (updateStudent.password != null)
            {
                string password = updateStudent.password;
                upd.Add($"Password='{password}'");
            }
            if (!string.IsNullOrEmpty(updateStudent.phone))
            {
                string phone = updateStudent.phone;
                upd.Add($"Phone='{phone}'");
            }
            if (updateStudent.dob != null)
            {
                string dob = updateStudent.dob;
                upd.Add($"Dob='{dob}'");
            }
            if (updateStudent.address != null)
            {
                string address = updateStudent.address;
                upd.Add($"Address='{address}'");
            }
            if (updateStudent.fileimage != null)
            {
                string folder = "~/images";
                string filename = updateStudent.fileimage.FileName;
                string savepath = Path.Combine(Server.MapPath(folder), filename);
                updateStudent.fileimage.SaveAs(savepath);
                string storepath = folder + "/" + filename;
                upd.Add($"Image='{storepath}'");
            }
            if (upd.Count == 0)
            {
                return RedirectToAction("UserProfile");
            }
            query += string.Join(",", upd);
            query += $" WHERE Id={user_id}";
            _connection.Open();
            new SqlCommand(query, _connection).ExecuteNonQuery();
            return RedirectToAction("UserProfile");
        }


        public ActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgetPassword(string email)
        {
            string checkQuery = $"SELECT * FROM Student where email='{email}'";
            _connection.Open();
            SqlCommand cmd = new SqlCommand(checkQuery, _connection);
            SqlDataReader rd = cmd.ExecuteReader();
            if (rd.Read())
            {
                string user_id = rd["id"].ToString();
                Random random = new Random();
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                string key = "";
                Random random1 = new Random();
                const string chars1 = "0123456789";
                string otp = "";

                string checkKey = $"SELECT * FROM Forget where user_id='{user_id}'";
                _connection.Close();
                _connection.Open();
                SqlCommand cmd2 = new SqlCommand(checkKey, _connection);


                SqlDataReader dr2 = cmd2.ExecuteReader();
                if (dr2.Read())
                {
                    key = dr2["fkey"].ToString();
                    otp = dr2["Otp"].ToString();
                }
                else
                {
                    key = new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
                    otp = new string(Enumerable.Repeat(chars1, 6).Select(s => s[random.Next(s.Length)]).ToArray());
                    string query = $"INSERT INTO Forget(fkey,Otp,user_id) VALUES ('{key}','{otp}','{user_id}')";
                    _connection.Close();
                    _connection.Open();
                    new SqlCommand(query, _connection).ExecuteNonQuery();
                }
                string b = "";
                using (StreamReader reader = new StreamReader(Server.MapPath("~/ExtraFiles/Mail.html")))
                {
                    b = reader.ReadToEnd();
                }
                _connection.Close();
                _connection.Open();
                SqlCommand cmd1 = new SqlCommand($"select name,email from Student where id={user_id}", _connection);
                SqlDataReader dr1 = cmd1.ExecuteReader();
                if (dr1.Read())

                {

                    string name = dr1["name"].ToString();
                    string uemail = dr1["email"].ToString();
                    b = b.Replace("{uname}", name);
                    b = b.Replace("{uemail}", uemail);
                    string link = $"https://localhost:44303/Home/Reset?key={key}";
                    b = b.Replace("{link}", link);
                    b = b.Replace("{OTP}", otp);

                }


                SmtpClient client = new SmtpClient("smtp.gmail.com", 587); //Gmail smtp               
                client.Credentials = new NetworkCredential("rjha3352@gmail.com", "xguxrgjpvflyurpk");
                client.EnableSsl = true;

                MailMessage message = new MailMessage("rjha3352@gmail.com", email);
                message.Subject = "Reset Your Password";
                message.Body = b;
                message.BodyEncoding = Encoding.UTF8;
                message.IsBodyHtml = true;


                client.Send(message);

                ViewBag.msg = "mail sent please check it to reset";

            }
            else
            {
                ViewBag.msg = "email not exist";
            }
            return View();
        }


        public ActionResult Reset(string key)
        {
            _connection.Open();
            string checkKey = $"SELECT * FROM Forget where fkey='{key}'";
            SqlCommand cmd2 = new SqlCommand(checkKey, _connection);
            SqlDataReader dr2 = cmd2.ExecuteReader();
            if (!dr2.Read())
            {
                TempData["msg"] = "Link not valid";
                return RedirectToAction("ForgetPassword");
            }

            Session["user_id"] = dr2["user_id"].ToString();

            return View();
        }
        [HttpPost]
        public ActionResult Reset(string key, string password, string cpassword, string otp)
        {
            _connection.Open();
            string otp1 = "";
            string query = $"SELECT * FROM Forget where fkey='{key}'";
            SqlCommand cmd = new SqlCommand(query, _connection);
            SqlDataReader dr2 = cmd.ExecuteReader();
            if (dr2.Read())
            {
                otp1 = dr2["Otp"].ToString();
                return RedirectToAction("ForgetPassword");
            }
            if (otp != otp1)
            {
                ViewBag.msg = "OTP is invalid!";
                return View();
            }

            if (password != cpassword)
            {
                ViewBag.msg = "Password and Confirm Password didnt matched";
                return View();
            }
            _connection.Open();
            string checkKey = $"UPDATE Student SET password ='{password}' where id='{Session["user_id"]}';DELETE FROM Forget where user_id='{Session["user_id"]}';";
            SqlCommand cmd2 = new SqlCommand(checkKey, _connection);
            cmd2.ExecuteNonQuery();
            Session["user_id"] = null;
            TempData["msg"] = "Password Has been updated";
            return RedirectToAction("login");
        }
        public ActionResult Delete()
        { 
         return View();
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            string image = "";
            _connection.Open();
            SqlCommand cmd1 = new SqlCommand($"select image from Student where id={id}", _connection);
            SqlDataReader dr1 = cmd1.ExecuteReader();
            if (dr1.Read())
            {
                image = dr1["image"].ToString();
            }   _connection.Close();
                _connection.Open();
            System.IO.File.Delete(Server.MapPath(image));
            string checkKey = $"delete from Student  where id='{id}';";
            SqlCommand cmd2 = new SqlCommand(checkKey, _connection);
            cmd2.ExecuteNonQuery();
            ViewBag.msg = "Your Account has been deleted";
            return View();
        }
    }
}

