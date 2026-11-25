using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vinfast.Models
{
    public class DSSanPham_Anh
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public int MaLoai { get; set; } // mới thêm 
        public decimal Gia { get; set; }
        public string MauSac { get; set; }
        public int TonKho { get; set; }

        public string AvatarDuongDan { get; set; }
    }
}