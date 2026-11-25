using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.SqlServer.Server;
using Vinfast.Models;

namespace Vinfast.Controllers
{
    public class VoucherController : Controller
    {
        static string strCon = ConfigurationManager.ConnectionStrings["DB_VINFAST_SHOWROOM"].ConnectionString;


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


        public List<Voucher> LayTatCaVoucher()
        {
            List<Voucher> ds = new List<Voucher>();
            SqlConnection con =GetUserConnection();

            using (SqlCommand cmd = new SqlCommand("LayTatCaVoucher", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                con.Open();
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        Voucher vc = new Voucher()
                        {
                            MaVoucher = rd["MaVoucher"].ToString(),
                            GhiChu = rd["GhiChu"].ToString(),
                            GiamGia = Convert.ToInt32(rd["GiamGia"]),
                            NgayTao = Convert.ToDateTime(rd["NgayTao"]),
                            NgayHetHan = Convert.ToDateTime(rd["NgayHetHan"]),
                            SoLuong = Convert.ToInt32(rd["SoLuong"])
                        };

                        ds.Add(vc);
                    }
                }
            }

            return ds;
        }


        public ActionResult DSVoucher()
        {
            if (Session["user"] == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để tiếp tục thao tác";
                return RedirectToAction("DangNhap", "Account");
            }
            return View(LayTatCaVoucher());
        }

        public ActionResult CreateVoucher()
        {
            if (Session["user"] == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để tiếp tục thao tác";
                return RedirectToAction("DangNhap", "Account");
            }
            return View();
        }

        public ActionResult CreateVoucher_Action(Voucher vc)   
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Vui lòng điền đầy đủ thông tin";
                return View("CreateVoucher", vc);
            }    
            if (Session["user"] == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập";
                return RedirectToAction("DangNhap", "Account");
            }

            if (vc.NgayTao < DateTime.Today)
            {
                TempData["Error"] = "Ngày tạo phải lớn hơn hoặc bằng ngày hôm nay.";
                return View("CreateVoucher", vc);
            }

            if (vc.NgayHetHan < vc.NgayTao)
            {
                TempData["Error"] = "Ngày hết hạn phải lớn hơn hoặc bằng ngày tạo.";
                return View("CreateVoucher", vc);
            }

            NguoiDung nd = (NguoiDung)Session["user"];
            int mand = nd.MaND;

            SqlConnection con =GetUserConnection();

            try
            {
                SqlCommand cmd = new SqlCommand("ThemVoucher", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@MaVoucher", vc.MaVoucher);
                cmd.Parameters.AddWithValue("@GhiChu", vc.GhiChu);
                cmd.Parameters.AddWithValue("@GiamGia", vc.GiamGia);
                cmd.Parameters.AddWithValue("@NgayTao", vc.NgayTao);
                cmd.Parameters.AddWithValue("@NgayHetHan", vc.NgayHetHan);
                cmd.Parameters.AddWithValue("@SoLuong", vc.SoLuong);

                cmd.Parameters.AddWithValue("@MaND", mand);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
                TempData["Success"] = "Thêm voucher thành công!";
                return RedirectToAction("DSVoucher", "Voucher");

            }
            catch(Exception ex)
            {
                con.Close();
                TempData["Error"] = ex.Message;
                return RedirectToAction("CreateVoucher", vc);
            }

        }


        public Voucher LayVoucher(string mavc)
        {
            SqlConnection con =GetUserConnection();

            Voucher voucher = new Voucher();
            SqlCommand cmd = new SqlCommand("LayVoucher", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@MaVoucher", mavc);

            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                voucher.MaVoucher = reader["MaVoucher"].ToString();
                voucher.GhiChu = reader["GhiChu"].ToString();
                voucher.GiamGia = int.Parse(reader["GiamGia"].ToString());
                voucher.NgayTao = DateTime.Parse(reader["NgayTao"].ToString());
                voucher.NgayHetHan = DateTime.Parse(reader["NgayHetHan"].ToString());
                voucher.SoLuong = int.Parse(reader["SoLuong"].ToString());

            }
            //con.Close();    
            return voucher;
        }


        public ActionResult SuaVoucher(string mavc)
        {
            if (Session["user"] == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để tiếp tục thao tác";
                return RedirectToAction("DangNhap", "Account");
            }
            return View(LayVoucher(mavc));
        }
        public ActionResult SuaVoucher_Action(Voucher vc)
        {
            if (Session["user"] == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập";
                return RedirectToAction("DangNhap", "Account");
            }

            NguoiDung nd = (NguoiDung)Session["user"];
            int mand = nd.MaND;
            SqlConnection con =GetUserConnection();

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Vui lòng điền đầy đủ thông tin";
                return RedirectToAction("SuaVoucher", new { mavc = vc.MaVoucher });

            }

            try
            {
                SqlCommand cmd = new SqlCommand("SuaVoucher", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@MaVoucher", vc.MaVoucher);
                cmd.Parameters.AddWithValue("@GhiChu", vc.GhiChu);
                cmd.Parameters.AddWithValue("@GiamGia", vc.GiamGia);
                cmd.Parameters.AddWithValue("@NgayTao", vc.NgayTao);
                cmd.Parameters.AddWithValue("@NgayHetHan", vc.NgayHetHan);
                cmd.Parameters.AddWithValue("@SoLuong", vc.SoLuong);
                cmd.Parameters.AddWithValue("@MaND", mand);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();

                TempData["Success"] = "Update voucher thành công";
                return RedirectToAction("DSVoucher", "Voucher");
            }
            catch(Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("SuaVoucher", new { mavc = vc.MaVoucher });

            }
        }


        public ActionResult XoaVoucher_Action(string mavc)
        {
            if (Session["user"] == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập";
                return RedirectToAction("DangNhap", "Account");
            }

            NguoiDung nd = (NguoiDung)Session["user"];
            int mand = nd.MaND;
            SqlConnection con =GetUserConnection();

            try
            {
                SqlCommand cmd = new SqlCommand("XoaVoucher", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@MaVoucher", mavc);
                cmd.Parameters.AddWithValue("@MaND", mand);

                con.Open();
                cmd.ExecuteNonQuery();
                TempData["Success"] = "Xóa voucher thành công";
                return RedirectToAction("DSVoucher", "Voucher");
            }
            catch (Exception ex) 
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("DSVoucher", "Voucher");
            }
        }
    }
}