using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vinfast.Models
{
    public class ChiTietGioHang
    {
        public int MaGH { get; set; }
        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public decimal Gia { get; set; }
        public int SoLuong { get; set; }
        public decimal ThanhTien { get; set; }

        //public DateTime NgayThem { get; set; }
    }
}