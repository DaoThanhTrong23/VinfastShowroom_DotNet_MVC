using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vinfast.Models
{
    public class SanPham
    {
        public int MaSP { get; set; }

        public string TenSP { get; set; }

        public int MaLoai { get; set; }

        public decimal Gia { get; set; }

        public string MauSac { get; set; }
        public DateTime NamSX { get; set; }

        public int Phienban { get; set; }

        public int Tonkho { get; set; }


    }
}