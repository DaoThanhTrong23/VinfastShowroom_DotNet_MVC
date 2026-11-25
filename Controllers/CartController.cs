using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls.WebParts;
using Vinfast.Models;

namespace Vinfast.Controllers
{
    public class CartController : Controller
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


        public JsonResult CartItemCount()
        {
            if (Session["user"] == null)
            {
                return Json(0, JsonRequestBehavior.AllowGet);

            }

            NguoiDung nd = (NguoiDung)Session["user"];
            int maND = nd.MaND;
            int count = 0;

            SqlConnection con = GetUserConnection();

            try
            {
                SqlCommand cmd = new SqlCommand("sp_GetCartItemCount", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@MaND", maND);

                con.Open();
                object result = cmd.ExecuteScalar();

                if (result != DBNull.Value && result != null)
                    count = Convert.ToInt32(result);

                con.Close();
                return Json(count, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex) 
            {
                con.Close();
                TempData["Error"] = ex.Message;
                return Json(count, JsonRequestBehavior.AllowGet);
            }

        }


        public ActionResult ThemVaoChiTietGioHang(int masp)
        {
            if (Session["user"] == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để thao tác";
                return RedirectToAction("DangNhap", "Account");

            }

            NguoiDung nd = (NguoiDung)Session["user"];
            int mand = nd.MaND;
            SqlConnection con = GetUserConnection();
            try
            {
                SqlCommand cmd = new SqlCommand("ThemChiTietGioHang", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@MaSP", masp);
                cmd.Parameters.AddWithValue("@MaND", mand);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
                TempData["Success"] = "Thêm vào thành công!";
                return RedirectToAction("DongXe", "Home");

            }
            catch(Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("DongXe", "Home");
            }
        }



        public ActionResult GioHang()
        {

            decimal tongthanhtoan = 0;
            if (Session["user"] == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để thao tác";
                return RedirectToAction("DangNhap", "Account");
            }

            NguoiDung nd = (NguoiDung)Session["user"];
            int mand = nd.MaND;
            SqlConnection con = GetUserConnection();

            List<ChiTietGioHang> list = new List<ChiTietGioHang>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_LayChiTietGioHang", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@MaND", mand);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while(dr.Read())
                {

                    decimal thanhTien = dr["ThanhTien"] != DBNull.Value? Convert.ToDecimal(dr["ThanhTien"]): 0;

                    tongthanhtoan += thanhTien;

                    list.Add(new ChiTietGioHang
                    {
                        MaGH = int.Parse(dr["MaGH"].ToString()),
                        MaSP = int.Parse(dr["MaSP"].ToString()),
                        TenSP = dr["TenSP"].ToString(),
                        SoLuong = int.Parse(dr["SoLuong"].ToString()),
                        Gia = decimal.Parse(dr["Gia"].ToString()),
                        ThanhTien = decimal.Parse(dr["ThanhTien"].ToString()),

                    });

                }    
                con.Close();

                ViewBag.TongThanhToan = tongthanhtoan;

                return View(list);

            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Dashboard", "Home");
            }
        }



        public ActionResult GiamSoLuongSP(int masp)
        {
          
            if (Session["user"] == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để thao tác";
                return RedirectToAction("DangNhap", "Account");
            }

            NguoiDung nd = (NguoiDung)Session["user"];
            int mand = nd.MaND;
            SqlConnection con = GetUserConnection();
            try
            {
                SqlCommand cmd = new SqlCommand("GiamSoLuongChiTietGioHang", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@MaND", mand);
                cmd.Parameters.AddWithValue("@MaSP", masp);

                con.Open();
                cmd.ExecuteNonQuery();
                return RedirectToAction("GioHang", "Cart");

            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("GioHang", "Cart");
            }

        }

        public ActionResult TangSoLuongSP(int masp)
        {
            if (Session["user"] == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để thao tác";
                return RedirectToAction("DangNhap", "Account");

            }

            NguoiDung nd = (NguoiDung)Session["user"];
            int mand = nd.MaND;
            SqlConnection con = GetUserConnection();
            try
            {
                SqlCommand cmd = new SqlCommand("ThemChiTietGioHang", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@MaSP", masp);
                cmd.Parameters.AddWithValue("@MaND", mand);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
                return RedirectToAction("GioHang", "Cart");

            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("GioHang", "Cart");
            }
        }



        public ActionResult ApDungVoucher(string MaVoucher)
        {
            try
            {
                using (SqlConnection con = GetUserConnection())
                {
                    SqlCommand cmd = new SqlCommand("LayVoucher", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MaVoucher", MaVoucher);

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        int giamGia = Convert.ToInt32(dr["GiamGia"]);
                        int soLuong = Convert.ToInt32(dr["SoLuong"]);

                        if (soLuong <= 0)
                        {
                            TempData["Error"] = "Voucher đã hết số lượng!";
                            TempData["GiamGia"] = 0;
                        }
                        else
                        {
                            TempData["Success"] = "Áp dụng voucher thành công!";
                            TempData["GiamGia"] = giamGia;
                            TempData["mavc"] = MaVoucher;
                        }
                    }
                    else
                    {
                        TempData["Error"] = "Voucher không tồn tại!";
                        TempData["GiamGia"] = 0;
                    }

                    return RedirectToAction("GioHang");

                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("GioHang");

            }

        }





        public ActionResult ThanhToan(string MaVoucher)
        {
            if (Session["user"] == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để thao tác";
                return RedirectToAction("DangNhap", "Account");
            }

            NguoiDung nd = (NguoiDung)Session["user"];
            int maND = nd.MaND;

            try
            {
                using (SqlConnection con = GetUserConnection())
                {
                    SqlCommand cmd = new SqlCommand("ThanhToanGioHang", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MaND", maND);
                    if (!string.IsNullOrEmpty(MaVoucher))
                        cmd.Parameters.AddWithValue("@MaVoucher", MaVoucher);
                    else
                        cmd.Parameters.AddWithValue("@MaVoucher", DBNull.Value);

                    con.Open();
                    cmd.ExecuteNonQuery();


                    con.Close();
                }

                TempData["Success"] = "Thanh toán thành công!";
                return RedirectToAction("GioHang", "Cart");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi thanh toán: " + ex.Message;
                return RedirectToAction("GioHang", "Cart");
            }
        }




        public List<HoaDon> LayHoaDonTheoNguoiDung(int maND)
        {
            List<HoaDon> dsHoaDon = new List<HoaDon>();

            using (SqlConnection con = GetUserConnection())
                try
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM dbo.fn_HoaDon_CuaKhachHang(@MaND)", con))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@MaND", maND);

                        con.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                dsHoaDon.Add(new HoaDon
                                {
                                    MaHD = Convert.ToInt32(reader["MaHD"]),
                                    MaND = Convert.ToInt32(reader["MaND"]),
                                    NgayBan = Convert.ToDateTime(reader["NgayBan"]),
                                    TongTien = Convert.ToDecimal(reader["TongTien"]),
                                    GiamGia = reader["GiamGia"] != DBNull.Value ? Convert.ToInt32(reader["GiamGia"]) : 0,
                                    ThanhToan = reader["ThanhToan"] != DBNull.Value ? Convert.ToDecimal(reader["ThanhToan"]) : 0,
                                    MaVoucher = reader["MaVoucher"] != DBNull.Value ? reader["MaVoucher"].ToString() : "",
                                    TrangThai = reader["TrangThai"].ToString()
                                });
                            }
                        }
                    }
                    return dsHoaDon;

                }
                catch (Exception ex)
                {
                    TempData["Error"] = ex.Message;
                    return dsHoaDon;

                }

        }



        public ActionResult _DanhSachHoaDon()
        {

            if (Session["user"] == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để thao tác";
                return RedirectToAction("DangNhap", "Account");
            }

            NguoiDung nd = (NguoiDung)Session["user"];
            int maND = nd.MaND;

            List<HoaDon> dsHoaDon  = new List<HoaDon>();

            dsHoaDon = LayHoaDonTheoNguoiDung(maND);

            return PartialView(dsHoaDon);
        }

    }
}