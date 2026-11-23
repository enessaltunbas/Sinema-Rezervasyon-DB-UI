using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace SinemaApp.Data
{
    // Veritabanı işlemleri için yardımcı sınıf (MS Access - OleDb)
    public class Db
    {
        private readonly string baglantiCumlesi;

        public Db(IConfiguration config, IWebHostEnvironment env)
        {
            var dosya = config["AccessDosyasi"] ?? "../../veritabani/SinemaRezervasyon.accdb";
            var tamYol = Path.GetFullPath(Path.Combine(env.ContentRootPath, dosya));
            baglantiCumlesi = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={tamYol};Persist Security Info=False;";
        }

        public OleDbConnection BaglantiAc()
        {
            var con = new OleDbConnection(baglantiCumlesi);
            con.Open();
            return con;
        }

        // Gösterim listesi sorgusu (bitis_saati ve bos_koltuk sorguda hesaplanıyor)
        public const string GosterimDetaySql = @"
            SELECT g.gosterim_no, g.tarih, g.baslangic_saati,
                   DateAdd('n', f.sure_dk, g.baslangic_saati) AS bitis_saati,
                   f.film_no, f.ad AS film_adi, f.tur, f.sure_dk, f.vizyon_tarihi,
                   si.sinema_no, si.ad AS sinema_adi,
                   sa.salon_no, sa.ad AS salon_adi, sa.kapasite,
                   g.bilet_fiyati,
                   COUNT(r.rezervasyon_no) AS satilan_koltuk,
                   sa.kapasite - COUNT(r.rezervasyon_no) AS bos_koltuk
            FROM (((GOSTERIM AS g
                  INNER JOIN FILM AS f ON g.film_no = f.film_no)
                  INNER JOIN SALON AS sa ON (g.sinema_no = sa.sinema_no AND g.salon_no = sa.salon_no))
                  INNER JOIN SINEMA AS si ON sa.sinema_no = si.sinema_no)
                  LEFT JOIN (SELECT rezervasyon_no, gosterim_no FROM REZERVASYON WHERE odeme_durumu <> 'Iptal') AS r
                         ON g.gosterim_no = r.gosterim_no
            GROUP BY g.gosterim_no, g.tarih, g.baslangic_saati,
                     f.film_no, f.ad, f.tur, f.sure_dk, f.vizyon_tarihi,
                     si.sinema_no, si.ad,
                     sa.salon_no, sa.ad, sa.kapasite, g.bilet_fiyati";

        // Rezervasyon listesi sorgusu
        public const string RezervasyonDetaySql = @"
            SELECT r.rezervasyon_no, r.rezervasyon_tarihi, r.odeme_durumu, r.koltuk_no,
                   g.bilet_fiyati AS ucret,
                   m.musteri_no, (m.ad & ' ' & m.soyad) AS musteri, m.eposta,
                   g.gosterim_no, g.tarih, g.baslangic_saati,
                   f.ad AS film_adi, si.ad AS sinema_adi, sa.ad AS salon_adi
            FROM ((((REZERVASYON AS r
                  INNER JOIN MUSTERI AS m ON r.musteri_no = m.musteri_no)
                  INNER JOIN GOSTERIM AS g ON r.gosterim_no = g.gosterim_no)
                  INNER JOIN FILM AS f ON g.film_no = f.film_no)
                  INNER JOIN SALON AS sa ON (g.sinema_no = sa.sinema_no AND g.salon_no = sa.salon_no))
                  INNER JOIN SINEMA AS si ON sa.sinema_no = si.sinema_no";

        public static DateTime SaatTarihi(TimeSpan s) =>
            new DateTime(1899, 12, 30, s.Hours, s.Minutes, s.Seconds);

        public DataTable Tablo(string sql, params object[] parametreler)
        {
            using var con = BaglantiAc();
            using var cmd = KomutOlustur(con, sql, parametreler);
            using var da = new OleDbDataAdapter(cmd);
            var dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public DataRow Satir(string sql, params object[] parametreler)
        {
            var dt = Tablo(sql, parametreler);
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        public object Deger(string sql, params object[] parametreler)
        {
            using var con = BaglantiAc();
            using var cmd = KomutOlustur(con, sql, parametreler);
            var sonuc = cmd.ExecuteScalar();
            return sonuc == DBNull.Value ? null : sonuc;
        }

        public int Calistir(string sql, params object[] parametreler)
        {
            using var con = BaglantiAc();
            using var cmd = KomutOlustur(con, sql, parametreler);
            return cmd.ExecuteNonQuery();
        }

        public long Ekle(string sql, params object[] parametreler)
        {
            using var con = BaglantiAc();
            using (var cmd = KomutOlustur(con, sql, parametreler))
            {
                cmd.ExecuteNonQuery();
            }
            using var idCmd = new OleDbCommand("SELECT @@IDENTITY", con);
            return Convert.ToInt64(idCmd.ExecuteScalar());
        }

        private static OleDbCommand KomutOlustur(OleDbConnection con, string sql, object[] parametreler)
        {
            var cmd = new OleDbCommand(sql, con);
            foreach (var p in parametreler)
            {
                if (p == null || p == DBNull.Value)
                {
                    cmd.Parameters.AddWithValue("?", DBNull.Value);
                }
                else if (p is TimeSpan ts)
                {
                    var prm = cmd.Parameters.Add("?", OleDbType.Date);
                    prm.Value = SaatTarihi(ts);
                }
                else if (p is DateTime dt)
                {
                    var prm = cmd.Parameters.Add("?", OleDbType.Date);
                    prm.Value = dt;
                }
                else if (p is decimal dec)
                {
                    var prm = cmd.Parameters.Add("?", OleDbType.Currency);
                    prm.Value = dec;
                }
                else
                {
                    cmd.Parameters.AddWithValue("?", p);
                }
            }
            return cmd;
        }

        // Telefon tablolarındaki kayıtları virgülle birleştirir
        public Dictionary<int, string> CokDegerliHarita(string tablo, string anahtarKolon, string degerKolon)
        {
            var dt = Tablo($"SELECT {anahtarKolon}, {degerKolon} FROM {tablo} ORDER BY {degerKolon}");
            var sonuc = new Dictionary<int, List<string>>();
            foreach (DataRow r in dt.Rows)
            {
                int k = Convert.ToInt32(r[anahtarKolon]);
                if (!sonuc.TryGetValue(k, out var liste))
                    sonuc[k] = liste = new List<string>();
                liste.Add(r[degerKolon].ToString());
            }
            var birlesik = new Dictionary<int, string>();
            foreach (var kv in sonuc)
                birlesik[kv.Key] = string.Join(", ", kv.Value);
            return birlesik;
        }

        // Gösterim eklenirken vizyon tarihi ve salon saat çakışması kontrolü
        public void GosterimDogrula(int filmNo, int sinemaNo, int salonNo, DateTime tarih, TimeSpan saat, int haricGosterimNo = 0)
        {
            var film = Satir("SELECT ad, sure_dk, vizyon_tarihi FROM FILM WHERE film_no = ?", filmNo);
            if (film == null) throw new InvalidOperationException("Seçilen film bulunamadı.");

            var vizyon = Convert.ToDateTime(film["vizyon_tarihi"]).Date;
            if (tarih.Date < vizyon)
                throw new InvalidOperationException($"Film vizyon tarihinden ({Bicim.Tarih(vizyon)}) önce gösterime konamaz.");

            int sure = Convert.ToInt32(film["sure_dk"]);
            var yeniBas = tarih.Date + saat;
            var yeniBit = yeniBas.AddMinutes(sure + 15);

            var mevcutlar = Tablo(@"
                SELECT g.gosterim_no, g.tarih, g.baslangic_saati, f.ad AS film_adi, f.sure_dk
                FROM GOSTERIM AS g INNER JOIN FILM AS f ON g.film_no = f.film_no
                WHERE g.sinema_no = ? AND g.salon_no = ? AND g.gosterim_no <> ?",
                sinemaNo, salonNo, haricGosterimNo);

            foreach (DataRow m in mevcutlar.Rows)
            {
                var mBas = Convert.ToDateTime(m["tarih"]).Date + Bicim.SaatDegeri(m["baslangic_saati"]);
                var mBit = mBas.AddMinutes(Convert.ToInt32(m["sure_dk"]) + 15);
                if (mBas < yeniBit && mBit > yeniBas)
                {
                    throw new InvalidOperationException(
                        $"Bu salonda {Bicim.Saat(mBas)} - {Bicim.Saat(mBit)} arasında '{m["film_adi"]}' gösterimi var.");
                }
            }
        }

        // Koltuk kapasite ve doluluk kontrolü yaparak rezervasyon ekler
        public long RezervasyonYap(int musteriNo, int gosterimNo, int koltukNo)
        {
            var g = Satir(@"
                SELECT g.gosterim_no, sa.kapasite
                FROM GOSTERIM AS g
                INNER JOIN SALON AS sa ON (g.sinema_no = sa.sinema_no AND g.salon_no = sa.salon_no)
                WHERE g.gosterim_no = ?", gosterimNo);
            if (g == null) throw new InvalidOperationException("Gösterim bulunamadı.");

            int kapasite = Convert.ToInt32(g["kapasite"]);
            if (koltukNo < 1 || koltukNo > kapasite)
                throw new InvalidOperationException($"Koltuk numarası 1 ile {kapasite} arasında olmalıdır.");

            var dolu = Convert.ToInt32(Deger(@"
                SELECT COUNT(*) FROM REZERVASYON
                WHERE gosterim_no = ? AND koltuk_no = ? AND odeme_durumu <> 'Iptal'",
                gosterimNo, koltukNo));
            if (dolu > 0)
                throw new InvalidOperationException("Bu koltuk bu gösterim için zaten dolu.");

            return Ekle(@"
                INSERT INTO REZERVASYON (musteri_no, gosterim_no, rezervasyon_tarihi, koltuk_no, odeme_durumu)
                VALUES (?, ?, ?, ?, 'Beklemede')",
                musteriNo, gosterimNo, DateTime.Now, koltukNo);
        }

        public static string HataMesaji(Exception ex)
        {
            var m = ex.Message ?? "İşlem sırasında hata oluştu.";
            if (m.Contains("unique", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("duplicate", StringComparison.OrdinalIgnoreCase))
                return "Bu kayıt zaten var (benzersiz olması gereken bir alan tekrar edildi).";
            if (m.Contains("integrity", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("foreign key", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("related record", StringComparison.OrdinalIgnoreCase))
                return "Bu kayda bağlı başka kayıtlar olduğu için silinemez.";
            return m;
        }
    }
}
