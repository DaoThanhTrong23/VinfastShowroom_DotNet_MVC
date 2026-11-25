using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Vinfast.Models;

namespace Vinfast.Controllers
{
    public class HomeController : Controller
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

        public string TaoID()
        {
            string time = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            string rand = Guid.NewGuid().ToString("N").Substring(0, 3);
            return time + rand;
        }

        public List<SelectListItem> LayDanhSachSanPham()
        {
            List<SelectListItem> dsSP = new List<SelectListItem>();
            SqlConnection con = GetUserConnection();

            try
            {
                string query = "SELECT * FROM dbo.fn_LayDanhSachSanPham()";
                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string tensp = reader["TenSP"].ToString();
                    string mau = reader["MauSac"].ToString();


                    dsSP.Add(new SelectListItem
                    {
                        Value = reader["MaSP"].ToString(),
                        Text = tensp + " - " + mau
                    });
                }
                con.Close();
                return dsSP;

            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return dsSP;

            }


        }



        /// các trang vieww và action

        public ActionResult Dashboard()
        {

            var model = LayDanhSach_MoiLoaiMotSP();
            return View(model);
        }

        public ActionResult _SPNoiBatTheoLoai()
        {

            var model = LayDanhSach_MoiLoaiMotSP();
            return PartialView(model);
        }



        public ActionResult GioiThieu()
        {
            return View();
        }



        //Lấy danh sách mối loại 1 ảnh cho home

        public List<HinhAnhSP> LayDanhSachAnh(int masp)
        {
            List<HinhAnhSP> list = new List<HinhAnhSP>();
            SqlConnection con = GetUserConnection();

            try
            {
                using (SqlCommand cmd = new SqlCommand("LayAnhSP", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MaSP", masp);

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            HinhAnhSP img = new HinhAnhSP();
                            img.MaAnh = reader["MaAnh"].ToString();
                            img.MaSP = Convert.ToInt32(reader["MaSP"]);
                            img.LoaiAnh = reader["LoaiAnh"].ToString();
                            img.Duongdan = reader["DuongDan"].ToString();
                            img.MotaAnh = reader["MoTaAnh"].ToString();

                            list.Add(img);
                        }
                    }
                }

                return list;

            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return list;

            }

        }

        public List<ThongTinSanPhamCoAnh> LayDanhSach_MoiLoaiMotSP()
        {
            List<ThongTinSanPhamCoAnh> ds = new List<ThongTinSanPhamCoAnh>();

            SqlConnection con = GetUserConnection();
            try
            {
                using (SqlCommand cmd = new SqlCommand("Lay_MoiLoaiMotSanPham", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ThongTinSanPhamCoAnh sp = new ThongTinSanPhamCoAnh();

                            sp.MaSP = Convert.ToInt32(reader["MaSP"]);
                            sp.TenSP = reader["TenSP"].ToString();
                            sp.MaLoai = Convert.ToInt32(reader["MaLoai"]);
                            sp.Gia = Convert.ToDecimal(reader["Gia"]);
                            sp.MauSac = reader["MauSac"].ToString();
                            sp.NamSX = Convert.ToDateTime(reader["NamSX"]);
                            sp.Phienban = Convert.ToInt32(reader["Phienban"]);
                            sp.Tonkho = Convert.ToInt32(reader["Tonkho"]);

                            // Mô tả
                            sp.MaMoTa = reader["MaMoTa"].ToString();
                            sp.NgoaiThat = reader["NgoaiThat"].ToString();
                            sp.NoiThat = reader["NoiThat"].ToString();
                            sp.AnToan = reader["AnToan"].ToString();
                            sp.CongNghe = reader["CongNghe"].ToString();
                            sp.TienNghi = reader["TienNghi"].ToString();

                            // Thông số
                            sp.MaTS = reader["MaTS"].ToString();
                            sp.DongCo = reader["DongCo"].ToString();
                            sp.CongSuat = Convert.ToInt32(reader["CongSuat"]);
                            sp.MoMenXoan = Convert.ToInt32(reader["MoMenXoan"]);
                            sp.QuangDuongToiDa = Convert.ToInt32(reader["QuangDuongToiDa"]);
                            sp.ThoiGianSac = Convert.ToInt32(reader["ThoiGianSac"]);
                            sp.SoChoNgoi = Convert.ToInt32(reader["SoChoNgoi"]);
                            sp.TrongLuong = Convert.ToInt32(reader["TrongLuong"]);
                            sp.TaiTrongToiDa = Convert.ToInt32(reader["TaiTrongToiDa"]);
                            sp.ChieuDai = Convert.ToInt32(reader["ChieuDai"]);
                            sp.ChieuRong = Convert.ToInt32(reader["ChieuRong"]);
                            sp.ChieuCao = Convert.ToInt32(reader["ChieuCao"]);
                            sp.TocDoToiDa = Convert.ToInt32(reader["TocDoToiDa"]);
                            sp.CheDoChay = reader["CheDoChay"].ToString();
                            sp.PhanhTaiSinh = reader["PhanhTaiSinh"].ToString();
                            sp.KhoangSangGam = Convert.ToInt32(reader["KhoangSangGam"]);
                            sp.HopSo = reader["HopSo"].ToString();
                            sp.DungLuongPin = Convert.ToInt32(reader["DungLuongPin"]);

                            sp.DSAnh = LayDanhSachAnh(sp.MaSP);

                            ds.Add(sp);
                        }
                    }
                }
                return ds;

            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return ds;

            }

        }

        // ------------------------------------------------------------------------------------------Dòng xe


        //load loại vào nav
        public List<LoaiSanPham> LayLoaiSPBangFunction()
        {
            List<LoaiSanPham> dsLoai = new List<LoaiSanPham>();
            SqlConnection con = new SqlConnection(strCon);

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
                return dsLoai;

            }
        }

        public ActionResult _LoaiSP()
        {
            return PartialView(LayLoaiSPBangFunction());
        }

        public List<DSSanPham_Anh> LayDSSanPhamVaoList()
        {
            List<DSSanPham_Anh> danhSachSP = new List<DSSanPham_Anh>();
            SqlConnection con = GetUserConnection();

            try
            {
                using (SqlCommand cmd = new SqlCommand("SP_LayDSSanPhamVaAvatar_Cursor", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            danhSachSP.Add(new DSSanPham_Anh
                            {
                                MaSP = reader.GetInt32(reader.GetOrdinal("MaSP")),
                                TenSP = reader["TenSP"]?.ToString(),
                                MaLoai = int.Parse(reader["MaLoai"].ToString()),//mới thêm nha tr
                                Gia = reader.GetDecimal(reader.GetOrdinal("Gia")),
                                MauSac = reader["MauSac"]?.ToString(),
                                TonKho = reader.GetInt32(reader.GetOrdinal("TonKho")),

                                AvatarDuongDan = reader["AvatarDuongDan"] != DBNull.Value ? reader["AvatarDuongDan"]?.ToString() : null
                            });
                        }
                    }
                }

                return danhSachSP;
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return danhSachSP;

            }
        }


        public ActionResult DongXe()
        {
            return View(LayDSSanPhamVaoList());
        }


        public ActionResult LocTheoLoai_Action(int maloai)
        {
            List<DSSanPham_Anh> list = LayDSSanPhamVaoList().FindAll(x => x.MaLoai == maloai).ToList();

            return View("DongXe", list);
        }

        public ActionResult TimTheoKeyword_Action(string keyword)
        {
            List<DSSanPham_Anh> list = LayDSSanPhamVaoList().ToList();
            if (!string.IsNullOrWhiteSpace(keyword) && keyword.Length > 0)
            {
                list = list.FindAll(x => x.TenSP.ToString().ToLower().Contains(keyword.ToString().ToLower()));
            }


            return View("DongXe", list);
        }

        //--------------------------------------------------------------------Danh sách các voucher
        public List<Voucher> LayTatCaVoucher()
        {
            List<Voucher> ds = new List<Voucher>();
            SqlConnection con = GetUserConnection();

            try
            {
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
            catch (Exception ex)
            {
                con.Close();
                TempData["Error"] = ex.Message;
                return ds;
            }
        }


        public ActionResult KhuyenMai()
        {
            List<Voucher> list = LayTatCaVoucher().FindAll(x => x.NgayHetHan >= DateTime.Now).ToList();
            return View(list);
        }


        //-------------------------------------------Liên hệ

        public ActionResult LienHe()
        {
            return View();
        }

        public List<PhieuLaiThu_View> XemPhieuLaiThuTheoMaND(int maND)
        {
            List<PhieuLaiThu_View> ds = new List<PhieuLaiThu_View>();

            SqlConnection con = GetUserConnection();

            try
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM fn_LichSuLaiThu_CuaKhachHang(@MaND)", con);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@MaND", maND);

                con.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                while (rd.Read())
                {
                    ds.Add(new PhieuLaiThu_View
                    {
                        MaLT = Convert.ToInt32(rd["MaLT"]),
                        MaND = Convert.ToInt32(rd["MaND"]),
                        TenNguoiDung = rd["HoTen"].ToString(),

                        MaSP = Convert.ToInt32(rd["MaSP"]),
                        TenSP = rd["TenSP"].ToString(),
                        MauSac = rd["MauSac"].ToString(),
                        Gia = Convert.ToDecimal(rd["Gia"]),

                        NgayBatDau = Convert.ToDateTime(rd["NgayBatDau"]),
                        NgayKetThuc = Convert.ToDateTime(rd["NgayKetThuc"]),
                        DiaDiem = rd["DiaDiem"].ToString(),
                        GhiChu = rd["GhiChu"].ToString(),
                        TrangThai = rd["TrangThai"].ToString()
                    });
                }

                con.Close();

                return ds;
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;

                con.Close();

                return ds;
            }
        }


        public ActionResult DangKyLaiThu()
        {
            if (Session["user"] == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để tiếp tục thực hiện thao tác";
                return RedirectToAction("DangNhap", "Account");
            }
            else
            {
                NguoiDung nd = (NguoiDung)Session["user"];
                int mand = nd.MaND;

                var dsSP = LayDanhSachSanPham();
                ViewBag.DanhSachSP = dsSP;
                return View(XemPhieuLaiThuTheoMaND(mand));
            }
        }




        /// Action

        public ActionResult DangKyLaiThuAction(FormCollection f)
        {

            if (Session["user"] == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập trước khi đăng ký lái thử.";
                return RedirectToAction("DangNhap", "Account");
            }

            NguoiDung nguoiDung = (NguoiDung)Session["user"];
            int maND = nguoiDung.MaND;

            int maSP;
            DateTime ngayBD = DateTime.Parse(f["ngayBD"]);
            DateTime ngayKT = DateTime.Parse(f["ngayKT"]);
            string diaDiem = f["diadiem"];
            string ghiChu = f["ghichu"];
            string trangthai = "Đăng ký";

            if (!int.TryParse(f["MaSP"], out maSP))
            {
                TempData["Error"] = "Sản phẩm không hợp lệ.";
                return RedirectToAction("DangKyLaiThu");
            }

            if (string.IsNullOrWhiteSpace(diaDiem))
            {
                TempData["Error"] = "Vui lòng nhập địa điểm lái thử.";
                return RedirectToAction("DangKyLaiThu");
            }



            SqlConnection con = GetUserConnection();

            try
            {

                using (SqlCommand cmd = new SqlCommand("ThemLaiThu", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MaND", maND);
                    cmd.Parameters.AddWithValue("@MaSP", maSP);
                    cmd.Parameters.AddWithValue("@NgayBatDau", ngayBD);
                    cmd.Parameters.AddWithValue("@NgayKetThuc", ngayKT);
                    cmd.Parameters.AddWithValue("@DiaDiem", diaDiem);
                    cmd.Parameters.AddWithValue("@GhiChu", ghiChu);
                    cmd.Parameters.AddWithValue("@TrangThai", trangthai);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    TempData["Success"] = "Đăng ký thành công";
                    con.Close();

                    return RedirectToAction("DangKyLaiThu", "Home");
                }
            }
            catch (Exception ex)
            {
                con.Close();

                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("DangKyLaiThu", "Home");
        }


        public ActionResult XoaLaiThu(int malt)
        {
            SqlConnection con = GetUserConnection();

            try
            {

                SqlCommand cmd = new SqlCommand("XoaLaiThu", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@MaLT", malt);

                con.Open();
                SqlDataReader rd = cmd.ExecuteReader();
                TempData["Success"] = "Xóa thành công";
                return RedirectToAction("DangKyLaiThu");
            }
            catch (Exception ex)
            {
                con.Close();
                TempData["Error"] = ex.Message;
                return RedirectToAction("DangKyLaiThu");

            }
        }

        public ActionResult HoanThanhLaiThu_User(int malt)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(strCon))
                {
                    using (SqlCommand cmd = new SqlCommand("SP_UpdateTrangThaiLaiThu", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MaLT", malt);

                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();

                        TempData["Success"] = "Update thành công";

                        return RedirectToAction("DangKyLaiThu", "Home");

                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("DangKyLaiThu", "Home");
            }
        }
    }
}