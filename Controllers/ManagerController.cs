using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Vinfast.Models;

namespace Vinfast.Controllers
{
    public class ManagerController : Controller
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




        public List<LoaiSanPham> LayLoaiSPBangFunction()
        {


            List<LoaiSanPham> dsLoai = new List<LoaiSanPham>();
            SqlConnection con = GetUserConnection();

            try
            {
                string query = "SELECT MaLoai, TenLoai FROM dbo.fn_LayDanhSachLoaiSP()";
                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    dsLoai.Add(new LoaiSanPham
                    {
                        MaLoaiSP = Convert.ToInt32(reader["MaLoai"]),
                        TenLoai = reader["TenLoai"].ToString()
                    });
                }
                con.Close();

                return dsLoai;
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                con.Close();
                return dsLoai;
            }
        }


        //---------------------------------------------------------------------------trang chủ manger

        public ActionResult Manager()
        {

            if (Session["user"] == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để tiếp tục thao tác";
                return RedirectToAction("DangNhap", "Account");
            }    
            List<HoaDon> dsHoaDonHomNay = new List<HoaDon>();

            using (SqlConnection conn = GetUserConnection())
            using (SqlCommand cmd = new SqlCommand("SP_HoaDonTrongNgay_Cursor", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@NgayXem", DateTime.Today);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    dsHoaDonHomNay.Add(new HoaDon
                    {
                        MaHD = Convert.ToInt32(reader["MaHD"]),
                        NgayBan = Convert.ToDateTime(reader["NgayBan"]),
                        TongTien = Convert.ToDecimal(reader["TongTien"]),

                        MaND = 0,
                        GiamGia = 0,
                        ThanhToan = Convert.ToDecimal(reader["TongTien"]),
                        MaVoucher = null,
                        TrangThai = "Đã thanh toán"
                    });
                }
                reader.Close();
            }

            var chartData = dsHoaDonHomNay.GroupBy(h => h.NgayBan.Hour).Select(g => new{Gio = g.Key,DoanhThu = g.Sum(x => x.TongTien)}).OrderBy(x => x.Gio).ToList();

            ViewBag.ChartLabels = chartData.Select(x => x.Gio + "h").ToArray();
            ViewBag.ChartValues = chartData.Select(x => x.DoanhThu).ToArray();

            ViewBag.TongDoanhThuHomNay = dsHoaDonHomNay.Sum(hd => hd.TongTien);
            ViewBag.SoHoaDonHomNay = dsHoaDonHomNay.Count;

            return View(dsHoaDonHomNay);
        }




        public List<TaiKhoan_NguoiDung> LayDanhSachTaiKhoan()
        {
            List<TaiKhoan_NguoiDung> danhSachTK = new List<TaiKhoan_NguoiDung>();
            SqlConnection con = GetUserConnection();

            try
            {
                using (SqlCommand cmd = new SqlCommand("SP_LayDSTKVaND_Cursor", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            danhSachTK.Add(new TaiKhoan_NguoiDung
                            {
                                MaND = reader.GetInt32(reader.GetOrdinal("MaND")),
                                HoTen = reader["HoTen"]?.ToString(),
                                GioiTinh = reader["GioiTinh"]?.ToString(),

                                NgaySinh = reader["NgaySinh"] != DBNull.Value ? reader.GetDateTime(reader.GetOrdinal("NgaySinh")) : DateTime.MinValue,

                                SoDienThoai = reader["SoDienThoai"]?.ToString(),
                                Email = reader["Email"]?.ToString(),
                                TinhTP = reader["TinhTP"]?.ToString(),
                                QuyenTruyCap = reader["QuyenTruyCap"]?.ToString(),

                                TenDangNhap = reader["TenDangNhap"]?.ToString(),
                                MatKhau = reader["MatKhau"]?.ToString()
                            });
                        }
                    }
                }
                con.Close();
                return danhSachTK;
            }
            catch (Exception ex) 
            {

                TempData["Error"] = ex.Message;
                con.Close();
                return danhSachTK;
            }
        }
        public ActionResult Taikhoan()
        {
            if (Session["user"] == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để tiếp tục thao tác";
                return RedirectToAction("DangNhap", "Account");
            }
       
            return View(LayDanhSachTaiKhoan());
        }

        public ActionResult LocTaiKhoanNhanVien()
        {
            List<TaiKhoan_NguoiDung> list = LayDanhSachTaiKhoan().FindAll(n => n.QuyenTruyCap != "Khách hàng");
            return View("Taikhoan", list);
        }
        public ActionResult LocTaiKhoanKhachHang()
        {
            List<TaiKhoan_NguoiDung> list = LayDanhSachTaiKhoan().FindAll(n => n.QuyenTruyCap == "Khách hàng");
            return View("Taikhoan", list);
        }




        ///dổi từ proc sang func 
        public ActionResult SuaNguoiDung(int mand)
        {
            NguoiDung nd = new NguoiDung();
            SqlConnection con = GetUserConnection();

            try
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM dbo.fn_LayNguoiDungTheoMa(@MaND)", con);
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@MaND", mand);

                con.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                if (rd.Read())
                {
                    nd.MaND = (int)rd["MaND"];
                    nd.HoTen = rd["HoTen"].ToString();
                    nd.GioiTinh = rd["GioiTinh"].ToString();
                    nd.NgaySinh = Convert.ToDateTime(rd["NgaySinh"]);
                    nd.SoDienThoai = rd["SoDienThoai"].ToString();
                    nd.Email = rd["Email"].ToString();
                    nd.TinhTP = rd["TinhTP"].ToString();
                    nd.QuyenTruyCap = rd["QuyenTruyCap"].ToString();
                }

                con.Close();
                return View(nd);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                con.Close();
                return View(nd);
            }
        }

        public ActionResult SuaNguoiDung_Action(NguoiDung nd, DateTime NgaySinh)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Vui lòng kiểm tra lại dữ liệu ";
                return View("SuaNguoiDung", nd);
            }
            SqlConnection con = GetUserConnection();

            try
            {
                SqlCommand cmd = new SqlCommand("SuaNguoiDung", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@MaND", nd.MaND);
                cmd.Parameters.AddWithValue("@HoTen", nd.HoTen);
                cmd.Parameters.AddWithValue("@GioiTinh", nd.GioiTinh);
                cmd.Parameters.AddWithValue("@NgaySinh", NgaySinh);
                cmd.Parameters.AddWithValue("@SoDienThoai", nd.SoDienThoai);
                cmd.Parameters.AddWithValue("@Email", nd.Email);
                cmd.Parameters.AddWithValue("@TinhTP", nd.TinhTP);
                cmd.Parameters.AddWithValue("@QuyenTruyCap", nd.QuyenTruyCap);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
                TempData["Success"] = "Update thành công";
                return RedirectToAction("Taikhoan", "Manager");

            }
            catch (Exception)
            {
                //TempData["Error"] = ex.Message;
                return RedirectToAction("Taikhoan", "Manager");

            }


        }

        public ActionResult XoaNguoiDung(int mand)
        {
            if (Session["user"] == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để tiếp tục thao tác";
                return RedirectToAction("DangNhap", "Account");
            }

            SqlConnection con = GetUserConnection();

            try
            {
                SqlCommand cmd = new SqlCommand("XoaNguoiDung", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@MaND", mand);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
                TempData["Success"] = "Xóa thành công";
                return RedirectToAction("Taikhoan", "Manager");

            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Taikhoan", "Manager");
            }
        }



        //-------------------------------------------------Loại sp
        public ActionResult QuanLyLoaiSanPham()
        {
            if (Session["user"] == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để tiếp tục thao tác";
                return RedirectToAction("DangNhap", "Account");
            }

            var dsLoaiSP = LayLoaiSPBangFunction();
            return View(dsLoaiSP);

        }

        public ActionResult _ThemLoaiSP()
        {
            if (Session["user"] == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để tiếp tục thao tác";
                return RedirectToAction("DangNhap", "Account");
            }

            return PartialView();
        }

        public ActionResult ThemLoaiSP_Action(FormCollection f)
        {
            string tenloai = f["tenloai"].ToString();

            NguoiDung nd = (NguoiDung)Session["user"];
            SqlConnection con = GetUserConnection();

            int mand = nd.MaND;

            if (string.IsNullOrEmpty(tenloai))
            {
                TempData["Error"] = "Tên loại không được để trống.";
                return RedirectToAction("QuanLyLoaiSanPham");
            }

            try
            {
                using (SqlCommand cmd = new SqlCommand("ThemLoaiSanPham", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TenLoai", tenloai);
                    //cmd.Parameters.AddWithValue("@MaND", mand);

                    con.Open();

                    cmd.ExecuteNonQuery();

                    con.Close();

                    TempData["Success"] = "Thêm loại sản phẩm thành công.";

                    return RedirectToAction("QuanLyLoaiSanPham", "Manager");
                }
            }
            catch (SqlException ex)
            {
                con.Close();

                string errorMessage = ex.Message;
                TempData["Error"] = errorMessage;
            }
            catch (Exception ex)
            {
                con.Close();

                TempData["Error"] = "Đã xảy ra lỗi hệ thống: " + ex.Message;
            }

            return RedirectToAction("QuanLyLoaiSanPham", "Manager");


        }

        public ActionResult Huy_ActionSP()
        {

            return RedirectToAction("QuanLyLoaiSanPham", "Manager");
        }

        public ActionResult SuaLoaiSanPham_Action(FormCollection f)
        {
            int maloai = Convert.ToInt32(f["maloai"]);
            string tenloai = f["tenloaimoi"].ToString();
            NguoiDung nd = (NguoiDung)Session["user"];
            int mand = nd.MaND;
            SqlConnection con = GetUserConnection();

            if (maloai.ToString().Length <= 0)
            {
                TempData["Error"] = "Mã loại sản phẩm không được để trống.";
                return RedirectToAction("QuanLyLoaiSanPham", "Manager");
            }

            if (string.IsNullOrEmpty(tenloai))
            {
                TempData["Error"] = "Tên loại sản phẩm không được để trống.";
                return RedirectToAction("QuanLyLoaiSanPham", "Manager");
            }


            try
            {
                using (SqlCommand cmd = new SqlCommand("SuaLoaiSanPham", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MaLoai", maloai);
                    cmd.Parameters.AddWithValue("@TenLoaiMoi", tenloai);
                    cmd.Parameters.AddWithValue("@MaND", mand);

                    con.Open();

                    cmd.ExecuteNonQuery();
                    con.Close();

                    TempData["Success"] = "Update loại sản phẩm thành công.";

                    return RedirectToAction("QuanLyLoaiSanPham", "Manager");
                }
            }
            catch (Exception ex)
            {
                con.Close();

                TempData["Error"] = "Đã xảy ra lỗi hệ thống: " + ex.Message;
            }

            return RedirectToAction("QuanLyLoaiSanPham", "Manager");

        }

        public ActionResult Delete_LoaiSP_Action(int maloai)
        {
            SqlConnection con = GetUserConnection();

            NguoiDung nd = (NguoiDung)Session["user"];
            int mand = nd.MaND;

            int maloaidelete = Convert.ToInt32(maloai);

            try
            {
                using (SqlCommand cmd = new SqlCommand("XoaLoaiSanPham", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MaLoai", maloaidelete);
                    //cmd.Parameters.AddWithValue("@MaND", mand);

                    con.Open();

                    cmd.ExecuteNonQuery();
                    con.Close();

                    TempData["Success"] = "Xóa loại sản phẩm thành công.";

                    return RedirectToAction("QuanLyLoaiSanPham", "Manager");
                }
            }

            catch (Exception ex)
            {
                con.Close();

                TempData["Error"] = "Đã xảy ra lỗi hệ thống: " + ex.Message;
            }

            return RedirectToAction("QuanLyLoaiSanPham", "Manager");

        }

        
        //------------------------Lấy ds lái thử

        public ActionResult LaiThu()
        {
            if (Session["user"] == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để tiếp tục thao tác";
                return RedirectToAction("DangNhap", "Account");
            }

            List<LaiThu_View> dsLichSu = new List<LaiThu_View>();

            using (SqlConnection con = GetUserConnection())
            {
                using (SqlCommand cmd = new SqlCommand("SP_LayDanhSachLaiThu_Cursor", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dsLichSu.Add(new LaiThu_View
                            {
                                MaLT = Convert.ToInt32(reader["MaLT"]),
                                MaND = Convert.ToInt32(reader["MaND"]),
                                HoTen = reader["HoTen"].ToString(),

                                MaSP = Convert.ToInt32(reader["MaSP"]),
                                TenSP = reader["TenSP"].ToString(),
                                MauSac = reader["MauSac"].ToString(),
                                Gia = Convert.ToDecimal(reader["Gia"]),

                                NgayBatDau = Convert.ToDateTime(reader["NgayBatDau"]),
                                NgayKetThuc = Convert.ToDateTime(reader["NgayKetThuc"]),
                                DiaDiem = reader["DiaDiem"].ToString(),
                                GhiChu = reader["GhiChu"].ToString(),
                                TrangThai = reader["TrangThai"].ToString()
                            });
                        }
                    }
                }
            }
            DateTime ngay = DateTime.Now;
            ViewBag.NgayLaiThu = ngay.ToString("yyyy-MM-dd");

            return View(dsLichSu);
        }

        public ActionResult HoanThanhLaiThu(int malt)
        {

            try
            {
                using (SqlConnection con = GetUserConnection()) 
                {
                    using (SqlCommand cmd = new SqlCommand("SP_UpdateTrangThaiLaiThu", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MaLT", malt);

                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();

                        TempData["Success"] = "Update thành công";

                        return RedirectToAction("LaiThu", "Manager");
                        
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("LaiThu", "Manager");
            }
        }

        public ActionResult LoclaiThu(DateTime? ngaylaithu )
        {

            DateTime ngay = ngaylaithu ?? DateTime.Today;


            List<LaiThu_View> dsLichSu = new List<LaiThu_View>();

            using (SqlConnection con = GetUserConnection())
            {
                using (SqlCommand cmd = new SqlCommand("SP_LayDanhSachLaiThu_Cursor", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dsLichSu.Add(new LaiThu_View
                            {
                                MaLT = Convert.ToInt32(reader["MaLT"]),
                                MaND = Convert.ToInt32(reader["MaND"]),
                                HoTen = reader["HoTen"].ToString(),

                                MaSP = Convert.ToInt32(reader["MaSP"]),
                                TenSP = reader["TenSP"].ToString(),
                                MauSac = reader["MauSac"].ToString(),
                                Gia = Convert.ToDecimal(reader["Gia"]),

                                NgayBatDau = Convert.ToDateTime(reader["NgayBatDau"]),
                                NgayKetThuc = Convert.ToDateTime(reader["NgayKetThuc"]),
                                DiaDiem = reader["DiaDiem"].ToString(),
                                GhiChu = reader["GhiChu"].ToString(),
                                TrangThai = reader["TrangThai"].ToString()
                            });
                        }
                    }
                }
            }

            ViewBag.NgayLaiThu = ngay.ToString("yyyy-MM-dd");

            dsLichSu = dsLichSu.FindAll(x => x.NgayBatDau.Date == ngay.Date);

            return View("LaiThu", dsLichSu);
        }


        //-----------------------------Hóa đơn


        

        public ActionResult HoaDon()
        {
            if (Session["user"] == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để tiếp tục thao tác";
                return RedirectToAction("DangNhap", "Account");
            }

            List<HoaDon> dsHoaDon = new List<HoaDon>();

            using (SqlConnection con = GetUserConnection())
            using (SqlCommand cmd = new SqlCommand("SP_LayDanhSachHoaDon_Cursor", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

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
            DateTime ngay = DateTime.Now;
            ViewBag.NgayHoaDon = ngay.ToString("yyyy-MM-dd");
            return View(dsHoaDon);
        }


        public ActionResult LocHDTheoNgay(DateTime? ngay)
        {
            DateTime ngayd = ngay ?? DateTime.Today;

            List<HoaDon> dsHoaDon = new List<HoaDon>();

            using (SqlConnection con = GetUserConnection())
            using (SqlCommand cmd = new SqlCommand("SP_LayDanhSachHoaDon_Cursor", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

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

            ViewBag.NgayHoaDon = ngayd.ToString("yyyy-MM-dd");

            dsHoaDon = dsHoaDon.FindAll(x => x.NgayBan.Date == ngayd.Date);

            return View("HoaDon", dsHoaDon);
        }

        //-------------------------Doanh thu

        public ActionResult DoanhThu(DateTime? tuNgay, DateTime? denNgay)
        {
            if (Session["user"] == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để tiếp tục thao tác";
                return RedirectToAction("DangNhap", "Account");
            }

            if (!tuNgay.HasValue)
                tuNgay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            if (!denNgay.HasValue)
                denNgay = DateTime.Now.Date; 

            var dsHoaDon = new List<HoaDon>();

            using (SqlConnection conn = GetUserConnection())
            {
                SqlCommand cmd = new SqlCommand("SP_LayDanhSachHoaDon_Cursor", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
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
                reader.Close();
            }

            dsHoaDon = dsHoaDon.Where(h => h.NgayBan.Date >= tuNgay.Value.Date && h.NgayBan.Date <= denNgay.Value.Date).ToList();

            var chartData = dsHoaDon.GroupBy(h => h.NgayBan.Date).Select(g => new { Ngay = g.Key.ToString("dd/MM/yyyy"), DoanhThu = g.Sum(x => x.ThanhToan) }).OrderBy(x => DateTime.ParseExact(x.Ngay, "dd/MM/yyyy", null)).ToList();

            ViewBag.ChartLabels = chartData.Select(x => x.Ngay).ToArray();
            ViewBag.ChartValues = chartData.Select(x => x.DoanhThu).ToArray();

            ViewBag.TongDoanhThu = dsHoaDon.Sum(h => h.ThanhToan);
            ViewBag.SoHoaDon = dsHoaDon.Count;

            ViewBag.TuNgay = tuNgay.Value.ToString("yyyy-MM-dd");
            ViewBag.DenNgay = denNgay.Value.ToString("yyyy-MM-dd");

            return View(dsHoaDon);
        }

    }



}