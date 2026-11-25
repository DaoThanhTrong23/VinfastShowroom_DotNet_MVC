using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vinfast.Models
{
    public class ThongTinSanPhamCoAnh
    {
        //thongtinsp
        public int MaSP { get; set; }

        public string TenSP { get; set; }

        public int MaLoai { get; set; }

        public decimal Gia { get; set; }

        public string MauSac { get; set; }
        public DateTime NamSX { get; set; }

        public int Phienban { get; set; }

        public int Tonkho { get; set; }

        //mô tả sp 
        public string MaMoTa { get; set; }
        public string NgoaiThat { get; set; }
        public string NoiThat { get; set; }
        public string AnToan { get; set; }
        public string CongNghe { get; set; }
        public string TienNghi { get; set; }

        //thông số sp
        public string MaTS { set; get; }
        public string DongCo { set; get; }
        public int CongSuat { set; get; }
        public int MoMenXoan { set; get; }
        public int QuangDuongToiDa { set; get; }
        public int ThoiGianSac { set; get; }
        public int SoChoNgoi { set; get; }
        public int TrongLuong { set; get; }
        public int TaiTrongToiDa { set; get; }
        public int ChieuDai { set; get; }
        public int ChieuRong { set; get; }
        public int ChieuCao { set; get; }
        public int TocDoToiDa { set; get; }
        public string CheDoChay { set; get; }
        public string PhanhTaiSinh { set; get; }
        public int KhoangSangGam { set; get; }
        public string HopSo { set; get; }
        public int DungLuongPin { set; get; }

        public List<HinhAnhSP> DSAnh { get; set; }

    }
}