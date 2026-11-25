using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vinfast.Models
{
    public class HoaDon
    {
        public int MaHD { get; set; }
        public int MaND { get; set; }
        public DateTime NgayBan { get; set; }
        public decimal TongTien { get; set; }
        public int GiamGia { get; set; }
        public decimal ThanhToan { get; set; }
        public string MaVoucher { get; set; }
        public string TrangThai { get; set; }
    }
}