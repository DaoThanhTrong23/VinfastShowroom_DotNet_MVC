using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vinfast.Models
{
    public class PhieuLaiThu_View
    {
        public int MaLT { get; set; }
        public int MaND { get; set; }
        public string TenNguoiDung { get; set; }

        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public string MauSac { get; set; }
        public decimal Gia { get; set; }

        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public string DiaDiem { get; set; }
        public string GhiChu { get; set; }
        public string TrangThai { get; set; }

    }
}