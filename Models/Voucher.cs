using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vinfast.Models
{
    public class Voucher
    {
        public string MaVoucher { get; set; }
        public string GhiChu { get; set; }
        public int GiamGia { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime NgayHetHan { get; set; }
        public int SoLuong { get; set; }

    }
}