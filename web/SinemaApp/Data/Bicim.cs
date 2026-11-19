using System;
using System.Globalization;

namespace SinemaApp.Data
{
    // Ekranda tarih, saat, para gösterimi için
    public static class Bicim
    {
        private static readonly CultureInfo tr = new CultureInfo("tr-TR");

        public static string Para(object deger)
        {
            if (deger == null || deger == DBNull.Value) return "0,00 ₺";
            return Convert.ToDecimal(deger).ToString("N2", tr) + " ₺";
        }

        public static string Tarih(object deger)
        {
            if (deger == null || deger == DBNull.Value) return "";
            return Convert.ToDateTime(deger).ToString("dd.MM.yyyy", tr);
        }

        public static string TarihGun(object deger)
        {
            if (deger == null || deger == DBNull.Value) return "";
            return Convert.ToDateTime(deger).ToString("dd MMMM dddd", tr);
        }

        public static string TarihSaat(object deger)
        {
            if (deger == null || deger == DBNull.Value) return "";
            return Convert.ToDateTime(deger).ToString("dd.MM.yyyy HH:mm", tr);
        }

        public static string Saat(object deger)
        {
            if (deger == null || deger == DBNull.Value) return "";
            if (deger is TimeSpan ts) return ts.ToString(@"hh\:mm");
            return deger.ToString();
        }

        // 1 -> A1, 11 -> B1 ... (her sırada 10 koltuk)
        public const int SiraBasinaKoltuk = 10;

        public static string Koltuk(object deger)
        {
            int no = Convert.ToInt32(deger);
            char sira = (char)('A' + (no - 1) / SiraBasinaKoltuk);
            int numara = (no - 1) % SiraBasinaKoltuk + 1;
            return sira.ToString() + numara;
        }

        public static string DurumRenk(object durum)
        {
            switch (durum?.ToString())
            {
                case "Odendi": return "success";
                case "Beklemede": return "warning";
                case "Iptal": return "secondary";
                default: return "light";
            }
        }

        public static string DurumYazi(object durum)
        {
            switch (durum?.ToString())
            {
                case "Odendi": return "Ödendi";
                case "Iptal": return "İptal";
                default: return durum?.ToString();
            }
        }
    }
}
