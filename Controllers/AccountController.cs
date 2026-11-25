using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Microsoft.Ajax.Utilities;
using Microsoft.SqlServer.Server;
using Vinfast.Models;

namespace Vinfast.Controllers
{
    public class AccountController : Controller
    {
        static string strCon = ConfigurationManager.ConnectionStrings["DB_VINFAST_SHOWROOM"].ConnectionString;

        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));   
                }
                return builder.ToString();
            }
        }

        private SqlConnection GetUserConnection()
        {
            string dbUser = Session["DBUsername"]?.ToString();
            string dbPass = Session["DBPassword"]?.ToString();

            if (string.IsNullOrEmpty(dbUser) || string.IsNullOrEmpty(dbPass))
            {
                return new SqlConnection(strCon);
            }

            string server = "LAPTOP-6A1M9L9L\\SQLEXPRESS";
            string database = "QL_VINFAST_SHOWROOM";

            string connStr = $"Server={server};Database={database};User Id={dbUser};Password={dbPass};";
            return new SqlConnection(connStr);
        }

        public ActionResult DangNhap()
        {
            return View();
        }

        public ActionResult DangNhapAction(FormCollection form)
        {
            string username = form["username"];
            string password = form["password"];

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ thông tin!";
                return RedirectToAction("DangNhap");
            }

            try
            {

                SqlConnection con = new SqlConnection(strCon);

                SqlCommand cmd = new SqlCommand("DangNhapLoginSQL", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TenDangNhap", username);
                cmd.Parameters.AddWithValue("@MatKhauDaBam", HashPassword(password));

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    NguoiDung user = new NguoiDung();
                    user.MaND = int.Parse(dt.Rows[0]["MaND"].ToString());
                    user.HoTen = dt.Rows[0]["HoTen"].ToString();
                    user.GioiTinh = dt.Rows[0]["GioiTinh"].ToString();
                    user.NgaySinh = DateTime.Parse(dt.Rows[0]["NgaySinh"].ToString());
                    user.SoDienThoai = dt.Rows[0]["SoDienThoai"].ToString();
                    user.Email = dt.Rows[0]["Email"].ToString();
                    user.TinhTP = dt.Rows[0]["TinhTP"].ToString();
                    user.QuyenTruyCap = dt.Rows[0]["QuyenTruyCap"].ToString();

                    Session["user"] = user;


                    Session["DBUsername"] = username;
                    Session["DBPassword"] = HashPassword(password).ToString();

                    return RedirectToAction("Dashboard", "Home");
                }
                else
                {
                    TempData["Error"] = "Tên đăng nhập hoặc mật khẩu không đúng!";
                    return RedirectToAction("DangNhap");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("DangNhap");
            }
        }
        public ActionResult DangKy()
        {
            return View();
        }
        public ActionResult DangKyAC(int? mand)
        {

            int id = (int) Session["ID"];
           
            ViewBag.MaND = mand.Value;
            return View();
        }


        public ActionResult DangKy_Action(FormCollection form)
        {
            string hoten = form["hoten"];
            string gioitinh = form["GioiTinh"];
            string ngaysinhStr = form["ngaysinh"];
            string sdt = form["sodienthoai"];
            string email = form["email"];
            string diachi = form["diachi"];


            if (string.IsNullOrWhiteSpace(hoten) ||
                string.IsNullOrWhiteSpace(gioitinh) ||
                string.IsNullOrWhiteSpace(ngaysinhStr) ||
                string.IsNullOrWhiteSpace(sdt) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(diachi)
            )
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ thông tin!";
                return RedirectToAction("DangKy", "Account");
            }

            DateTime ngaysinh;
            if (!DateTime.TryParse(ngaysinhStr, out ngaysinh))
            {
                TempData["Error"] = "Ngày sinh không hợp lệ!";
                return RedirectToAction("DangKy", "Account");
            }

            if (ngaysinh > DateTime.Now)
            {
                TempData["Error"] = "Ngày sinh không được lớn hơn ngày hiện tại!";
                return RedirectToAction("DangKy", "Account");
            }

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                TempData["Error"] = "Email không hợp lệ!";
                return RedirectToAction("DangKy");
            }

            if (!Regex.IsMatch(sdt, @"^[0-9]{10}$"))
            {
                TempData["Error"] = "Số điện thoại phải có đúng 10 chữ số!";
                return RedirectToAction("DangKy", "Account");
            }

            string qtc = "Khách hàng";

            try
            {
                SqlConnection con = new SqlConnection(strCon);

                SqlCommand cmd = new SqlCommand("ThemNguoiDung", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@HoTen", hoten);
                cmd.Parameters.AddWithValue("@GioiTinh", gioitinh);
                cmd.Parameters.AddWithValue("@NgaySinh", ngaysinh);
                cmd.Parameters.AddWithValue("@SoDienThoai", sdt);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@TinhTP", diachi);
                cmd.Parameters.AddWithValue("@QuyenTruyCap", qtc);


                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                //con.Open();
                if (dt.Rows.Count > 0)
                {
                    int newid = Convert.ToInt32(dt.Rows[0]["MaND"]);
                    //tempdata["success"] = $"đăng ký thành công! mã người dùng: {newid}";

                    Session["ID"] = newid;  
                    return RedirectToAction("DangKyAC", "Account", new { mand = newid });
                }
                else
                {
                    TempData["error"] = "đăng ký thất bại!";
                }

            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("DangKy", "Account");
            }
            return RedirectToAction("DangKy", "Account");
        }


        [HttpPost]
        public ActionResult DangKyAC_Action(FormCollection form)
        {
            string username = form["username"];
            string password = form["password"];
            string password_re = form["password_re"];

            string info = form["mandi"];

            int mandi = (int)Session["ID"];
            //if (!int.TryParse(info, out int mandi))
            //{
            //    TempData["Error"] = "Mã người dùng không hợp lệ!";
            //    return View("DangKyAC", new { mand = info });
            //}


            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(password_re))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ thông tin!";
                return View("DangKyAC", new { mand = mandi });
            }

            if (password != password_re)
            {
                TempData["Error"] = "Mật khẩu nhập lại không khớp!";
                return View("DangKyAC", new { mand = mandi });
            }

            var passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$");
            if (!passwordRegex.IsMatch(password))
            {
                TempData["Error"] = "Mật khẩu phải có ít nhất 8 ký tự, gồm chữ hoa, chữ thường, số và ký tự đặc biệt!";
                return View("DangKyAC", new { mand = mandi });
            }

            try
            {
                SqlConnection con = new SqlConnection(strCon);

                SqlCommand cmd = new SqlCommand("ThemTaiKhoan", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TenDangNhap", username);

                cmd.Parameters.AddWithValue("@MatKhau", HashPassword(password));
                cmd.Parameters.AddWithValue("@MaND", mandi);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    TempData["Success"] = "Đăng ký thành công!";
                    Session["user"] = null;
                    return RedirectToAction("DangNhap");
                }
                else
                {
                    TempData["Error"] = "Đăng ký thất bại!";
                    return View("DangKyAC", new { mand = mandi });
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View("DangKyAC", new { mand = mandi });
            }
        }

        //P@ssword01
        public ActionResult DangXuatAction()
        {
            Session["user"] = null;
            return RedirectToAction("Dashboard", "Home");
        }

    }
}