using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Vinfast.Models;

namespace Vinfast.Controllers
{
    public class SanPhamController : Controller
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




        // hàm sp
        public List<LoaiSanPham> LayLoaiSPBangFunction()
        {


            List<LoaiSanPham> dsLoai = new List<LoaiSanPham>();
            SqlConnection con = GetUserConnection();

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

        public List<SelectListItem> GetLoaiSPSelectList()
        {
            List<LoaiSanPham> dsLoaiSP = LayLoaiSPBangFunction();
            List<SelectListItem> selectList = new List<SelectListItem>();

            foreach (var item in dsLoaiSP)
            {
                selectList.Add(new SelectListItem
                {
                    Value = item.MaLoaiSP.ToString(),
                    Text = item.TenLoai
                });
            }
            return selectList;
        }
       

        public static string TaoID()
        {
            string timePart = DateTime.Now.ToString("yyyyMMddHHmmssffff");
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random rand = new Random();

            string randomPart = new string(Enumerable.Repeat(chars, 4)
                .Select(s => s[rand.Next(s.Length)]).ToArray());

            return timePart + randomPart;
        }




        // VIewwww

        public ActionResult DanhSachSanPham()
        {

            return View();
        }

        public ActionResult CreateSanPham()
        {

            var loaiSPList = LayLoaiSPBangFunction().OrderBy(n => n.TenLoai);

            ViewBag.DSLoaiSP = new SelectList(loaiSPList,"MaLoaiSP", "TenLoai"   );
            return View();
        }
 

        public ActionResult ThemSanPhamHoanChinh(FormCollection f,HttpPostedFileBase file_avata,HttpPostedFileBase file_ngoaithat,HttpPostedFileBase file_noithat,HttpPostedFileBase file_congnghe)
        {
            if (Session["user"] == null)
            {
                TempData["Error"] = "Lỗi xác thực: Vui lòng đăng nhập lại.";
                return RedirectToAction("DangNhap", "Account");
            }
            NguoiDung nd = (NguoiDung)Session["user"];
            int maND = nd.MaND;
            int newMaSP = 0;


            string tenSP = f["TenSP"];
            int.TryParse(f["MaLoai"], out int maLoai);
            decimal.TryParse(f["Gia"], out decimal gia);
            string mauSac = f["MauSac"];
            int nam;
            if (!int.TryParse(f["NamSX"], out nam))
            {
                TempData["Error"] = "Năm sản xuất không hợp lệ!";
                return View("CreateSanPham");
            }

            DateTime namSX = new DateTime(nam, 1, 1);
            int.TryParse(f["PhienBan"], out int phienBan);
            int.TryParse(f["TonKho"], out int tonKho);

            string dongco = f["DongCo"].ToString();
            int.TryParse(f["CongSuat"], out int congSuat);
            int.TryParse(f["MoMenXoan"], out int moMenXoan);
            int.TryParse(f["QuangDuongToiDa"], out int quangDuongToiDa);
            int.TryParse(f["ThoiGianSac"], out int thoigiansac);
            int.TryParse(f["SoChoNgoi"], out int sochongoi);
            int.TryParse(f["TrongLuong"], out int trongluong);
            int.TryParse(f["TaiTrongToiDa"], out int taitrongtoida);
            int.TryParse(f["ChieuDai"], out int chieudai);
            int.TryParse(f["ChieuRong"], out int chieurong);
            int.TryParse(f["ChieuCao"], out int chieucao);
            int.TryParse(f["TocDoToiDa"], out int tocdotoida);
            string chedochay = f["CheDoChay"].ToString();
            string phanhtaisinh = f["PhanhTaiSinh"].ToString();
            int.TryParse(f["KhoangSangGam"], out int khoangsanggam);
            string hopso = f["HopSo"].ToString();
            int.TryParse(f["DungLuongPin"], out int dungluongpin);

            string ngoaithat = f["NgoaiThat"].ToString();
            string noithat = f["NoiThat"].ToString();
            string antoan = f["AnToan"].ToString();
            string congnghe = f["CongNghe"].ToString();
            string tiennghi = f["TienNghi"].ToString();



            //string loaianh_avata = "Avater";
            //string Đuongan_avata = "";
            string MoTaAnh_avata = f["mota_avata"].ToString();


            //string loaianh_ngoaithat = "Ngoại thất";
            //string Đuongan_ngoaithat = "";
            string MoTaAnh_ngoaithat = f["mota_ngoaithat"].ToString();


            //string loaianh_noithat = "Nội thất";
            //string Đuongan_noithat = "";
            string MoTaAnh_noithat = f["mota_noithat"].ToString();


            //string loaianh_congnghe = "Công nghệ";
            //string Đuongan_congnghe = "";
            string MoTaAnh_congnghe = f["mota_congnghe"].ToString();


            if (string.IsNullOrEmpty(tenSP) || maLoai == 0 || gia <= 0)
            {
                TempData["Error"] = "Vui lòng nhập Tên sản phẩm, Loại và Giá hợp lệ.";
                var loaiSPList = LayLoaiSPBangFunction().OrderBy(n => n.TenLoai);
                ViewBag.DSLoaiSP = new SelectList(loaiSPList, "MaLoaiSP", "TenLoai");
                return View("CreateSanPham", "SanPham"); 
            }

            if(string.IsNullOrEmpty(mauSac))
            {
                TempData["Error"] = "Vui lòng nhập màu.";
                var loaiSPList = LayLoaiSPBangFunction().OrderBy(n => n.TenLoai);
                ViewBag.DSLoaiSP = new SelectList(loaiSPList, "MaLoaiSP", "TenLoai");
                return View("CreateSanPham", "SanPham");
            }

            if (namSX.Year < 1900 || namSX.Year > DateTime.Now.Year)
            {
                TempData["Error"] = "Năm sản xuất phải từ 1900 đến năm hiện tại.";
                var loaiSPList = LayLoaiSPBangFunction().OrderBy(n => n.TenLoai);
                ViewBag.DSLoaiSP = new SelectList(loaiSPList, "MaLoaiSP", "TenLoai");
                return View("CreateSanPham");
            }
            if (phienBan <= 0)
            {
                TempData["Error"] = "Phiên bản phải lớn hơn 0.";
                var loaiSPList = LayLoaiSPBangFunction().OrderBy(n => n.TenLoai);
                ViewBag.DSLoaiSP = new SelectList(loaiSPList, "MaLoaiSP", "TenLoai");
                return View("CreateSanPham");
            }
            if (tonKho < 0)
            {
                TempData["Error"] = "Tồn kho không được nhỏ hơn 0.";
                var loaiSPList = LayLoaiSPBangFunction().OrderBy(n => n.TenLoai);
                ViewBag.DSLoaiSP = new SelectList(loaiSPList, "MaLoaiSP", "TenLoai");
                return View("CreateSanPham");
            }

            if (congSuat <= 0)
            {
                TempData["Error"] = "Công suất phải lớn hơn 0.";
                var loaiSPList = LayLoaiSPBangFunction().OrderBy(n => n.TenLoai);
                ViewBag.DSLoaiSP = new SelectList(loaiSPList, "MaLoaiSP", "TenLoai");
                return View("CreateSanPham");
            }

            if (moMenXoan <= 0)
            {
                TempData["Error"] = "Mô-men xoắn phải lớn hơn 0.";
                var loaiSPList = LayLoaiSPBangFunction().OrderBy(n => n.TenLoai);
                ViewBag.DSLoaiSP = new SelectList(loaiSPList, "MaLoaiSP", "TenLoai");
                return View("CreateSanPham");
            }
            if (quangDuongToiDa <= 0)
            {
                TempData["Error"] = "Quãng đường tối đa phải lớn hơn 0.";
                var loaiSPList = LayLoaiSPBangFunction().OrderBy(n => n.TenLoai);
                ViewBag.DSLoaiSP = new SelectList(loaiSPList, "MaLoaiSP", "TenLoai");
                return View("CreateSanPham");
            }

            if (thoigiansac <= 0)
            {
                TempData["Error"] = "Thời gian sạc phải lớn hơn 0.";
                var loaiSPList = LayLoaiSPBangFunction().OrderBy(n => n.TenLoai);
                ViewBag.DSLoaiSP = new SelectList(loaiSPList, "MaLoaiSP", "TenLoai");
                return View("CreateSanPham");
            }
            if (sochongoi <= 0)
            {
                TempData["Error"] = "Số chỗ ngồi phải lớn hơn 0.";
                var loaiSPList = LayLoaiSPBangFunction().OrderBy(n => n.TenLoai);
                ViewBag.DSLoaiSP = new SelectList(loaiSPList, "MaLoaiSP", "TenLoai");
                return View("CreateSanPham");
            }
            if (trongluong <= 0)
            {
                TempData["Error"] = "Trọng lượng phải lớn hơn 0.";
                var loaiSPList = LayLoaiSPBangFunction().OrderBy(n => n.TenLoai);
                ViewBag.DSLoaiSP = new SelectList(loaiSPList, "MaLoaiSP", "TenLoai");
                return View("CreateSanPham");
            }
            if (taitrongtoida <= 0)
            {
                TempData["Error"] = "Tải trọng tối đa phải lớn hơn 0.";
                var loaiSPList = LayLoaiSPBangFunction().OrderBy(n => n.TenLoai);
                ViewBag.DSLoaiSP = new SelectList(loaiSPList, "MaLoaiSP", "TenLoai");
                return View("CreateSanPham");
            }

            if (chieudai <= 0)
            {
                TempData["Error"] = "Chiều dài phải lớn hơn 0.";
                var loaiSPList = LayLoaiSPBangFunction().OrderBy(n => n.TenLoai);
                ViewBag.DSLoaiSP = new SelectList(loaiSPList, "MaLoaiSP", "TenLoai");
                return View("CreateSanPham");
            }
            if (chieurong <= 0)
            {
                TempData["Error"] = "Chiều rộng phải lớn hơn 0.";
                var loaiSPList = LayLoaiSPBangFunction().OrderBy(n => n.TenLoai);
                ViewBag.DSLoaiSP = new SelectList(loaiSPList, "MaLoaiSP", "TenLoai");
                return View("CreateSanPham");
            }

            if (chieucao <= 0)
            {
                TempData["Error"] = "Chiều cao phải lớn hơn 0.";
                var loaiSPList = LayLoaiSPBangFunction().OrderBy(n => n.TenLoai);
                ViewBag.DSLoaiSP = new SelectList(loaiSPList, "MaLoaiSP", "TenLoai");
                return View("CreateSanPham");
            }
            if (tocdotoida <= 0)
            {
                TempData["Error"] = "Tốc độ tối đa phải lớn hơn 0.";
                var loaiSPList = LayLoaiSPBangFunction().OrderBy(n => n.TenLoai);
                ViewBag.DSLoaiSP = new SelectList(loaiSPList, "MaLoaiSP", "TenLoai");
                return View("CreateSanPham");
            }
            if (khoangsanggam <= 0)
            {
                TempData["Error"] = "Khoảng sáng gầm phải lớn hơn 0.";
                var loaiSPList = LayLoaiSPBangFunction().OrderBy(n => n.TenLoai);
                ViewBag.DSLoaiSP = new SelectList(loaiSPList, "MaLoaiSP", "TenLoai");
                return View("CreateSanPham");
            }
            if (dungluongpin <= 0)
            {
                TempData["Error"] = "Dung lượng pin phải lớn hơn 0.";
                var loaiSPList = LayLoaiSPBangFunction().OrderBy(n => n.TenLoai);
                ViewBag.DSLoaiSP = new SelectList(loaiSPList, "MaLoaiSP", "TenLoai");
                return View("CreateSanPham");
            }

            if (string.IsNullOrEmpty(ngoaithat))
            {
                TempData["Error"] = "Vui lòng nhập mô tả ngoại thất.";
                var loaiSPList = LayLoaiSPBangFunction().OrderBy(n => n.TenLoai);
                ViewBag.DSLoaiSP = new SelectList(loaiSPList, "MaLoaiSP", "TenLoai");
                return View("CreateSanPham");
            }


            if (string.IsNullOrEmpty(noithat))
            {
                TempData["Error"] = "Vui lòng nhập mô tả nội thất.";
                var loaiSPList = LayLoaiSPBangFunction().OrderBy(n => n.TenLoai);
                ViewBag.DSLoaiSP = new SelectList(loaiSPList, "MaLoaiSP", "TenLoai");
                return View("CreateSanPham");
            }
            if (string.IsNullOrEmpty(antoan))
            {
                TempData["Error"] = "Vui lòng nhập thông tin an toàn.";
                var loaiSPList = LayLoaiSPBangFunction().OrderBy(n => n.TenLoai);
                ViewBag.DSLoaiSP = new SelectList(loaiSPList, "MaLoaiSP", "TenLoai");
                return View("CreateSanPham");
            }
            if (string.IsNullOrEmpty(congnghe))
            {
                TempData["Error"] = "Vui lòng nhập thông tin công nghệ.";
                var loaiSPList = LayLoaiSPBangFunction().OrderBy(n => n.TenLoai);
                ViewBag.DSLoaiSP = new SelectList(loaiSPList, "MaLoaiSP", "TenLoai");
                return View("CreateSanPham");
            }
            if (string.IsNullOrEmpty(tiennghi))
            {
                TempData["Error"] = "Vui lòng nhập thông tin tiện nghi.";
                var loaiSPList = LayLoaiSPBangFunction().OrderBy(n => n.TenLoai);
                ViewBag.DSLoaiSP = new SelectList(loaiSPList, "MaLoaiSP", "TenLoai");
                return View("CreateSanPham");
            }

            if (string.IsNullOrEmpty(MoTaAnh_avata))
            {
                //TempData["Error"] = "Vui lòng nhập mô tả ảnh Avatar.";
                MoTaAnh_avata = "không có mô tả";
                //var loaiSPList = LayLoaiSPBangFunction().OrderBy(n => n.TenLoai);
                //ViewBag.DSLoaiSP = new SelectList(loaiSPList, "MaLoaiSP", "TenLoai");
                //return View("CreateSanPham");
            }

            if (string.IsNullOrEmpty(MoTaAnh_ngoaithat))
            {
                //TempData["Error"] = "Vui lòng nhập mô tả ảnh Ngoại thất.";
                //var loaiSPList = LayLoaiSPBangFunction().OrderBy(n => n.TenLoai);
                //ViewBag.DSLoaiSP = new SelectList(loaiSPList, "MaLoaiSP", "TenLoai");
                //return View("CreateSanPham");
                MoTaAnh_ngoaithat =  "không có mô tả";

            }

            if (string.IsNullOrEmpty(MoTaAnh_noithat))
            {
                //TempData["Error"] = "Vui lòng nhập mô tả ảnh Nội thất.";
                //var loaiSPList = LayLoaiSPBangFunction().OrderBy(n => n.TenLoai);
                //ViewBag.DSLoaiSP = new SelectList(loaiSPList, "MaLoaiSP", "TenLoai");
                //return View("CreateSanPham");

                MoTaAnh_noithat = "không có mô tả";
            }

            if (string.IsNullOrEmpty(MoTaAnh_congnghe))
            {
                //TempData["Error"] = "Vui lòng nhập mô tả ảnh Công nghệ.";
                //var loaiSPList = LayLoaiSPBangFunction().OrderBy(n => n.TenLoai);
                //ViewBag.DSLoaiSP = new SelectList(loaiSPList, "MaLoaiSP", "TenLoai");
                //return View("CreateSanPham");
                MoTaAnh_congnghe = "không có mô tả";
            }




            SqlConnection con = GetUserConnection();

            con.Open();
            SqlTransaction transaction = con.BeginTransaction();

            try
            {

                using (SqlCommand cmdSP = new SqlCommand("ThemSanPham", con, transaction))
                {
                    cmdSP.CommandType = CommandType.StoredProcedure;

                    cmdSP.Parameters.AddWithValue("@TenSP", tenSP);
                    cmdSP.Parameters.AddWithValue("@MaLoai", maLoai);
                    cmdSP.Parameters.AddWithValue("@Gia", gia);
                    cmdSP.Parameters.AddWithValue("@MauSac", mauSac ?? (object)DBNull.Value);
                    cmdSP.Parameters.AddWithValue("@NamSX", namSX); 
                    cmdSP.Parameters.AddWithValue("@PhienBan", phienBan > 0 ? (object)phienBan : DBNull.Value);
                    cmdSP.Parameters.AddWithValue("@TonKho", tonKho);
                    cmdSP.Parameters.AddWithValue("@MaND", maND);

                    object result = cmdSP.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        newMaSP = Convert.ToInt32(result);
                    }
                    else
                    {
                        throw new Exception("Thao tác ThemSanPham thất bại.");
                    }
                }


                string maTS = TaoID();
                using (SqlCommand cmdTS = new SqlCommand("ThemThongSo_SP", con, transaction))
                {
                    cmdTS.CommandType = CommandType.StoredProcedure;

                    cmdTS.Parameters.AddWithValue("@MaTS", maTS);
                    cmdTS.Parameters.AddWithValue("@MaSP", newMaSP);

                    cmdTS.Parameters.AddWithValue("@DongCo", f["DongCo"] ?? (object)DBNull.Value);
                    cmdTS.Parameters.AddWithValue("@CongSuat", congSuat > 0 ? (object)congSuat : DBNull.Value);
                    cmdTS.Parameters.AddWithValue("@MoMenXoan", moMenXoan > 0 ? (object)moMenXoan : DBNull.Value);

                    cmdTS.Parameters.AddWithValue("@QuangDuongToiDa", quangDuongToiDa);
                    cmdTS.Parameters.AddWithValue("@ThoiGianSac", thoigiansac);
                    cmdTS.Parameters.AddWithValue("@SoChoNgoi", sochongoi);
                    cmdTS.Parameters.AddWithValue("@TrongLuong", trongluong);
                    cmdTS.Parameters.AddWithValue("@TaiTrongToiDa", taitrongtoida);
                    cmdTS.Parameters.AddWithValue("@ChieuDai", chieudai);
                    cmdTS.Parameters.AddWithValue("@ChieuRong", chieurong);
                    cmdTS.Parameters.AddWithValue("@ChieuCao", chieucao);
                    cmdTS.Parameters.AddWithValue("@TocDoToiDa", tocdotoida);
                    cmdTS.Parameters.AddWithValue("@CheDoChay", chedochay);
                    cmdTS.Parameters.AddWithValue("@PhanhTaiSinh", phanhtaisinh);
                    cmdTS.Parameters.AddWithValue("@KhoangSangGam", khoangsanggam);
                    cmdTS.Parameters.AddWithValue("@HopSo", hopso);
                    cmdTS.Parameters.AddWithValue("@DungLuongPin", dungluongpin);


                    cmdTS.Parameters.AddWithValue("@MaND", maND);


                    cmdTS.ExecuteNonQuery();
                }




                string maMT = TaoID();
                using (SqlCommand cmdTS = new SqlCommand("ThemMoTa_SP", con, transaction))
                {
                    cmdTS.CommandType = CommandType.StoredProcedure;

                    cmdTS.Parameters.AddWithValue("@MaMoTa", maMT);
                    cmdTS.Parameters.AddWithValue("@MaSP", newMaSP);
                    cmdTS.Parameters.AddWithValue("@NgoaiThat", ngoaithat);
                    cmdTS.Parameters.AddWithValue("@NoiThat", noithat);
                    cmdTS.Parameters.AddWithValue("@AnToan", antoan);
                    cmdTS.Parameters.AddWithValue("@CongNghe", congnghe);
                    cmdTS.Parameters.AddWithValue("@TienNghi", tiennghi);


                    cmdTS.Parameters.AddWithValue("@MaND", maND);


                    cmdTS.ExecuteNonQuery();
                }





                //string maMT = TaoID();

                string folderPath = Server.MapPath("~/Content/Images/");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                var fileData = new List<(HttpPostedFileBase File, string MoTa, string Loai)>()
                    {
                        (file_avata, f["mota_avata"], "Avatar"),
                        (file_ngoaithat, f["mota_ngoaithat"], "Ngoại thất"),
                        (file_noithat, f["mota_noithat"], "Nội thất"),
                         (file_congnghe, f["mota_congnghe"], "Công nghệ")
                     };

                int imageIndex = 1;
                foreach (var item in fileData)
                {
                    if (item.File != null && item.File.ContentLength > 0)
                    {
                        string maAnh = TaoID();

                        string dir = "~/Content/Images/";
                        string filename = "";
                        string pysicaldir = Server.MapPath(dir);
                        if (!Directory.Exists(pysicaldir))
                        {
                            Directory.CreateDirectory(pysicaldir);
                        }
                        filename = Path.GetFileName(item.File.FileName);
                        string fulldir = Path.Combine(pysicaldir, filename);
                        item.File.SaveAs(fulldir);
                        string duongdan = filename;


                        using (SqlCommand cmdHA = new SqlCommand("ThemHinhAnh_SP", con, transaction))
                        {
                            cmdHA.CommandType = CommandType.StoredProcedure;
                            cmdHA.Parameters.AddWithValue("@MaAnh", maAnh);
                            cmdHA.Parameters.AddWithValue("@MaSP", newMaSP);
                            cmdHA.Parameters.AddWithValue("@LoaiAnh", item.Loai);
                            cmdHA.Parameters.AddWithValue("@DuongDan", duongdan);
                            cmdHA.Parameters.AddWithValue("@MoTaAnh", item.MoTa);
                            cmdHA.Parameters.AddWithValue("@MaND", maND);
                            cmdHA.ExecuteNonQuery();
                        }

                        imageIndex++;
                    }
                }

                transaction.Commit();

                TempData["Success"] = $"Thêm sản phẩm '{tenSP}' (Mã SP: {newMaSP}) hoàn chỉnh thành công!";
                var loaiSPList = LayLoaiSPBangFunction().OrderBy(n => n.TenLoai);
                ViewBag.DSLoaiSP = new SelectList(loaiSPList, "MaLoaiSP", "TenLoai");
                con.Close();

                return RedirectToAction("CreateSanPham");

            }
            catch (Exception ex)
            {


                con.Close();

                TempData["Error"] = $"Lỗi hệ thống khi lưu sản phẩm. Đã thực hiện Rollback. Chi tiết: {ex.Message}";

                var loaiSPList = LayLoaiSPBangFunction().OrderBy(n => n.TenLoai);
                ViewBag.DSLoaiSP = new SelectList(loaiSPList, "MaLoaiSP", "TenLoai");
                return View("CreateSanPham");
            }
            
        }

        public List<DSSanPham_Anh> LayDSSanPhamVaoList()
        {
            List<DSSanPham_Anh> danhSachSP = new List<DSSanPham_Anh>();
            SqlConnection con = GetUserConnection();

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
                            MaLoai = int.Parse(reader["MaLoai"].ToString()),//mới thêm nha
                            Gia = reader.GetDecimal(reader.GetOrdinal("Gia")),
                            MauSac = reader["MauSac"]?.ToString(),
                            TonKho = reader.GetInt32(reader.GetOrdinal("TonKho")),

                            AvatarDuongDan = reader["AvatarDuongDan"] != DBNull.Value? reader["AvatarDuongDan"]?.ToString(): null
                        });
                    }
                } 
            }

            return danhSachSP;
        }


        public ActionResult DSSanPham()
        {

            if (Session["user"] == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để tiếp tục thao tác";
                return RedirectToAction("DangNhap", "Account");
            }

            return View(LayDSSanPhamVaoList());
        }




        //load thong tin sản phẩm lêm

        
        public ThongTinAll_SanPham LayThongTinSP(int masp)
        {
            ThongTinAll_SanPham sp = new ThongTinAll_SanPham();
            SqlConnection con = GetUserConnection();


            SqlCommand cmd = new SqlCommand("LayThongTinSanPham", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@MaSP", masp);

            con.Open();
            SqlDataReader rd = cmd.ExecuteReader();

            if (rd.Read())
            {
                sp.MaSP = (int)rd["MaSP"];
                sp.TenSP = rd["TenSP"].ToString();
                sp.MaLoai = (int)rd["MaLoai"];
                sp.Gia = Convert.ToDecimal(rd["Gia"]);
                sp.MauSac = rd["MauSac"].ToString();
                sp.NamSX = Convert.ToDateTime(rd["NamSX"]);
                sp.Phienban = Convert.ToInt32(rd["PhienBan"]);
                sp.Tonkho = Convert.ToInt32(rd["TonKho"]);
            }

            rd.NextResult();

            if (rd.Read())
            {
                sp.MaTS = rd["MaTS"].ToString();
                sp.DongCo = rd["DongCo"].ToString();
                sp.CongSuat = Convert.ToInt32(rd["CongSuat"]);
                sp.MoMenXoan = Convert.ToInt32(rd["MoMenXoan"]);
                sp.QuangDuongToiDa = Convert.ToInt32(rd["QuangDuongToiDa"]);
                sp.ThoiGianSac = Convert.ToInt32(rd["ThoiGianSac"]);
                sp.SoChoNgoi = Convert.ToInt32(rd["SoChoNgoi"]);
                sp.TrongLuong = Convert.ToInt32(rd["TrongLuong"]);
                sp.TaiTrongToiDa = Convert.ToInt32(rd["TaiTrongToiDa"]);
                sp.ChieuDai = Convert.ToInt32(rd["ChieuDai"]);
                sp.ChieuRong = Convert.ToInt32(rd["ChieuRong"]);
                sp.ChieuCao = Convert.ToInt32(rd["ChieuCao"]);
                sp.TocDoToiDa = Convert.ToInt32(rd["TocDoToiDa"]);
                sp.CheDoChay = rd["CheDoChay"].ToString();
                sp.PhanhTaiSinh = rd["PhanhTaiSinh"].ToString();
                sp.KhoangSangGam = Convert.ToInt32(rd["KhoangSangGam"]);
                sp.HopSo = rd["HopSo"].ToString();
                sp.DungLuongPin = Convert.ToInt32(rd["DungLuongPin"]);
            }

            rd.NextResult();

            if (rd.Read())
            {
                sp.MaMoTa = rd["MaMoTa"].ToString();
                sp.NgoaiThat = rd["NgoaiThat"].ToString();
                sp.NoiThat = rd["NoiThat"].ToString();
                sp.AnToan = rd["AnToan"].ToString();
                sp.CongNghe = rd["CongNghe"].ToString();
                sp.TienNghi = rd["TienNghi"].ToString();
            }

            rd.Close();

            return sp;
        }


        public ActionResult SuaThongTinSanPham(int masp)
        {

            ModelState.Clear();   

            var loaiSPList = LayLoaiSPBangFunction().OrderBy(n => n.TenLoai);
            ViewBag.DSLoaiSP = new SelectList(loaiSPList, "MaLoaiSP", "TenLoai");

            var model = LayThongTinSP(masp);
            model.PhanhTaiSinh = model.PhanhTaiSinh.ToString().Trim(); 

            return View(model);
        }


        public ActionResult SuaThongTinSanPham_Action(ThongTinAll_SanPham model)
        {
            SqlConnection con = GetUserConnection();

            if (!ModelState.IsValid)
            {

                var loaiSPList = LayLoaiSPBangFunction().OrderBy(n => n.TenLoai);
                ViewBag.DSLoaiSP = new SelectList(loaiSPList, "MaLoaiSP", "TenLoai", model.MaLoai);
                model.PhanhTaiSinh = model.PhanhTaiSinh.ToString().Trim();
                TempData["Error"] = "Vui lòng điền đầy dữ thông tin";

                return View("SuaThongTinSanPham", model);
            }

            try
            {
                SqlCommand cmd = new SqlCommand("SuaSanPham", con);
                cmd.CommandType = CommandType.StoredProcedure;

                string phanhTaiSinh = model.PhanhTaiSinh?.Trim();

                cmd.Parameters.AddWithValue("@MaSP", model.MaSP);
                cmd.Parameters.AddWithValue("@TenSP", model.TenSP.Trim());
                cmd.Parameters.AddWithValue("@MaLoai", model.MaLoai);
                cmd.Parameters.AddWithValue("@Gia", model.Gia);
                cmd.Parameters.AddWithValue("@MauSac", model.MauSac.Trim());
                cmd.Parameters.AddWithValue("@NamSX", model.NamSX);
                cmd.Parameters.AddWithValue("@PhienBan", model.Phienban);
                cmd.Parameters.AddWithValue("@TonKho", model.Tonkho);

                cmd.Parameters.AddWithValue("@DongCo", model.DongCo.Trim());
                cmd.Parameters.AddWithValue("@CongSuat", model.CongSuat);
                cmd.Parameters.AddWithValue("@MoMenXoan", model.MoMenXoan);
                cmd.Parameters.AddWithValue("@QuangDuongToiDa", model.QuangDuongToiDa);
                cmd.Parameters.AddWithValue("@ThoiGianSac", model.ThoiGianSac);
                cmd.Parameters.AddWithValue("@SoChoNgoi", model.SoChoNgoi);
                cmd.Parameters.AddWithValue("@TrongLuong", model.TrongLuong);
                cmd.Parameters.AddWithValue("@TaiTrongToiDa", model.TaiTrongToiDa);
                cmd.Parameters.AddWithValue("@ChieuDai", model.ChieuDai);
                cmd.Parameters.AddWithValue("@ChieuRong", model.ChieuRong);
                cmd.Parameters.AddWithValue("@ChieuCao", model.ChieuCao);
                cmd.Parameters.AddWithValue("@TocDoToiDa", model.TocDoToiDa);
                cmd.Parameters.AddWithValue("@CheDoChay", model.CheDoChay.Trim());
                cmd.Parameters.AddWithValue("@PhanhTaiSinh", phanhTaiSinh);
                cmd.Parameters.AddWithValue("@KhoangSangGam", model.KhoangSangGam);
                cmd.Parameters.AddWithValue("@HopSo", model.HopSo.Trim());
                cmd.Parameters.AddWithValue("@DungLuongPin", model.DungLuongPin);

                cmd.Parameters.AddWithValue("@NgoaiThat", model.NgoaiThat.Trim());
                cmd.Parameters.AddWithValue("@NoiThat", model.NoiThat.Trim());
                cmd.Parameters.AddWithValue("@AnToan", model.AnToan.Trim());
                cmd.Parameters.AddWithValue("@CongNghe", model.CongNghe.Trim());
                cmd.Parameters.AddWithValue("@TienNghi", model.TienNghi.Trim());

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();

                TempData["Success"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction("DSSanPham", "SanPham");
            }
            catch (Exception ex)
            {
                TempData["Error"] =ex.Message;

                var loaiSPList = LayLoaiSPBangFunction().OrderBy(n => n.TenLoai);
                ViewBag.DSLoaiSP = new SelectList(loaiSPList, "MaLoaiSP", "TenLoai", model.MaLoai);

                return View("SuaThongTinSanPham", model);
            }
        }

        ///lấy danh sách ảnh
        public List<HinhAnhSP> LayDanhSachAnhSP(int maSP)
        {
            List<HinhAnhSP> dsAnh = new List<HinhAnhSP>();
            SqlConnection con = GetUserConnection();


            using (SqlCommand cmd = new SqlCommand("SP_LayAnhSanPham_Cursor", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@MaSP", maSP);

                con.Open();
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        HinhAnhSP anh = new HinhAnhSP()
                        {
                            MaAnh = rd["MaAnh"].ToString(),
                            LoaiAnh = rd["LoaiAnh"].ToString(),
                            Duongdan = rd["DuongDan"].ToString(),
                            MotaAnh = rd["MoTaAnh"].ToString()
                        };
                        dsAnh.Add(anh);
                    }
                }
            }

            return dsAnh;
        }


        public ActionResult SuaAnhSP(int masp)
        {

            return View(LayDanhSachAnhSP(masp));
        }

        public HinhAnhSP Lay1Anh(string maanh)
        {
            HinhAnhSP anh = new HinhAnhSP();
            SqlConnection con = GetUserConnection();

            SqlCommand cmd = new SqlCommand("SELECT * FROM dbo.LayAnhTheoMaAnh(@MaAnh)", con);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@MaAnh", maanh.ToString());

            con.Open();
            using (SqlDataReader rd = cmd.ExecuteReader())
            {
                if (rd.Read()) 
                {
                    anh = new HinhAnhSP()
                    {
                        MaAnh = rd["MaAnh"].ToString(),
                        MaSP = Convert.ToInt32(rd["MaSP"]),
                        LoaiAnh = rd["LoaiAnh"].ToString(),
                        Duongdan = rd["DuongDan"].ToString(),
                        MotaAnh = rd["MoTaAnh"].ToString()
                    };
                }
            }

            return anh;
        }

        public ActionResult SuaMotAnh(string maanh)
        {
            return View(Lay1Anh(maanh));
        }


        public void LuuAnhSP(string MaAnh, string moTaAnh, HttpPostedFileBase fileAnhMoi)
        {
            SqlConnection con = GetUserConnection();

            using (SqlCommand cmd = new SqlCommand("SuaAnhSP", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@MaAnh", MaAnh.ToString());
                cmd.Parameters.AddWithValue("@MotaAnh", moTaAnh);

                if (fileAnhMoi != null && fileAnhMoi.ContentLength > 0)
                {

                    string dir = "~/Content/Images/";
                    string filename = "";
                    string pysicaldir = Server.MapPath(dir);
                    if (!Directory.Exists(pysicaldir))
                    {
                        Directory.CreateDirectory(pysicaldir);
                    }
                    filename = Path.GetFileName(fileAnhMoi.FileName);
                    string fulldir = Path.Combine(pysicaldir, filename);
                    fileAnhMoi.SaveAs(fulldir);
                    string duongdan = filename;

                    cmd.Parameters.AddWithValue("@Duongdan", duongdan);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@Duongdan", DBNull.Value);
                }

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }


        public ActionResult SuaMotAnh_Action(string MaAnh, string MotaAnh, HttpPostedFileBase fileAnhMoi)
        {
            try
            {
                LuuAnhSP(MaAnh, MotaAnh, fileAnhMoi);

                TempData["Success"] = "Update ảnh thành công";
                return RedirectToAction("DSSanPham", "SanPham");
            }
            catch
            {
                TempData["Error"] = "Không thể lưu thay đổi";
                return RedirectToAction("DSSanPham", "SanPham");

            }
        }


        public ActionResult XoaSP_Action(int masp)
        {
            SqlConnection con = GetUserConnection();

            if (Session["user"] == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập";
                return RedirectToAction("DangNhap", "Account");
            }

            NguoiDung nd = (NguoiDung)Session["user"];

            int mand = nd.MaND;

            try
            { 

                SqlCommand cmd = new SqlCommand("XoaSanPham", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@MaSP", masp);
                cmd.Parameters.AddWithValue("@MaND", mand);
                con.Open();
                cmd.ExecuteNonQuery();

                TempData["Success"] = "Xóa thành công";
                return RedirectToAction("DSSanPham", "SanPham");

            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("DSSanPham", "SanPham");
            }
        }




        ////chi tiết sp
        public List<HinhAnhSP> LayDanhSachAnh(int masp)
        {
            List<HinhAnhSP> list = new List<HinhAnhSP>();
            SqlConnection con = GetUserConnection();

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
        public ActionResult LayChiTietSP_Action(int masp)
        {
            ThongTinSanPhamCoAnh sp = new ThongTinSanPhamCoAnh();

            SqlConnection con = GetUserConnection();

            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_LayThongTinSanPhamCoAnh", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MaSP", masp);


                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {

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

                        }
                    }

                    con.Close();
                }

                return View(sp);

            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("DongXe", "Home");
            }



        }

    }
}