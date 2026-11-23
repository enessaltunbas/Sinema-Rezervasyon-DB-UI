using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;
using SinemaApp.Data;

namespace SinemaApp.Pages.Musteriler
{
    public class DuzenleModel : PageModel
    {
        private readonly Db db;

        public DuzenleModel(Db db)
        {
            this.db = db;
        }

        [BindProperty]
        public MusteriForm Musteri { get; set; } = new MusteriForm();

        public DataTable Telefonlar { get; set; }
        public DataTable Rezervasyonlar { get; set; }

        public bool YeniKayit => Musteri.MusteriNo == 0;

        public IActionResult OnGet(int? id)
        {
            if (id == null) return Page();

            var satir = db.Satir("SELECT * FROM musteri WHERE musteri_no = @id", Db.P("@id", id));
            if (satir == null) return NotFound();

            Musteri.MusteriNo = Convert.ToInt32(satir["musteri_no"]);
            Musteri.Ad = satir["ad"].ToString();
            Musteri.Soyad = satir["soyad"].ToString();
            Musteri.Eposta = satir["eposta"].ToString();
            AltTablolariYukle();
            return Page();
        }

        private void AltTablolariYukle()
        {
            Telefonlar = db.Tablo("SELECT * FROM musteri_telefon WHERE musteri_no = @id",
                Db.P("@id", Musteri.MusteriNo));

            Rezervasyonlar = db.Tablo(
                "SELECT * FROM v_rezervasyon_detay WHERE musteri_no = @id ORDER BY tarih DESC, baslangic_saati DESC",
                Db.P("@id", Musteri.MusteriNo));
        }

        public IActionResult OnPostKaydet(string ilkTelefon)
        {
            if (!ModelState.IsValid)
            {
                if (!YeniKayit) AltTablolariYukle();
                return Page();
            }

            try
            {
                if (YeniKayit)
                {
                    // müşteri ve ilk telefonu aynı transaction içinde eklensin
                    using var con = db.BaglantiAc();
                    using var tr = con.BeginTransaction();

                    var cmd = new MySqlCommand(
                        "INSERT INTO musteri (ad, soyad, eposta) VALUES (@ad, @soyad, @eposta)", con, tr);
                    cmd.Parameters.AddWithValue("@ad", Musteri.Ad.Trim());
                    cmd.Parameters.AddWithValue("@soyad", Musteri.Soyad.Trim());
                    cmd.Parameters.AddWithValue("@eposta", Musteri.Eposta.Trim().ToLower());
                    cmd.ExecuteNonQuery();
                    long yeniNo = cmd.LastInsertedId;

                    if (!string.IsNullOrWhiteSpace(ilkTelefon))
                    {
                        var cmdTel = new MySqlCommand(
                            "INSERT INTO musteri_telefon (musteri_no, telefon) VALUES (@no, @tel)", con, tr);
                        cmdTel.Parameters.AddWithValue("@no", yeniNo);
                        cmdTel.Parameters.AddWithValue("@tel", ilkTelefon.Trim());
                        cmdTel.ExecuteNonQuery();
                    }

                    tr.Commit();
                    TempData["Mesaj"] = "Müşteri kaydedildi.";
                    return RedirectToPage(new { id = yeniNo });
                }

                db.Calistir(
                    "UPDATE musteri SET ad = @ad, soyad = @soyad, eposta = @eposta WHERE musteri_no = @id",
                    Db.P("@ad", Musteri.Ad.Trim()),
                    Db.P("@soyad", Musteri.Soyad.Trim()),
                    Db.P("@eposta", Musteri.Eposta.Trim().ToLower()),
                    Db.P("@id", Musteri.MusteriNo));
                TempData["Mesaj"] = "Müşteri güncellendi.";
                return RedirectToPage(new { id = Musteri.MusteriNo });
            }
            catch (MySqlException ex)
            {
                ModelState.AddModelError("", Db.HataMesaji(ex));
                if (!YeniKayit) AltTablolariYukle();
                return Page();
            }
        }

        public IActionResult OnPostTelefonEkle(int id, string telefon)
        {
            if (string.IsNullOrWhiteSpace(telefon))
            {
                TempData["Hata"] = "Telefon numarası boş olamaz.";
                return RedirectToPage(new { id });
            }

            try
            {
                db.Calistir("INSERT INTO musteri_telefon (musteri_no, telefon) VALUES (@id, @tel)",
                    Db.P("@id", id), Db.P("@tel", telefon.Trim()));
            }
            catch (MySqlException ex)
            {
                TempData["Hata"] = Db.HataMesaji(ex);
            }
            return RedirectToPage(new { id });
        }

        public IActionResult OnPostTelefonSil(int id, string telefon)
        {
            db.Calistir("DELETE FROM musteri_telefon WHERE musteri_no = @id AND telefon = @tel",
                Db.P("@id", id), Db.P("@tel", telefon));
            return RedirectToPage(new { id });
        }

        public class MusteriForm
        {
            public int MusteriNo { get; set; }

            [Required(ErrorMessage = "Ad boş olamaz")]
            [StringLength(50)]
            public string Ad { get; set; }

            [Required(ErrorMessage = "Soyad boş olamaz")]
            [StringLength(50)]
            public string Soyad { get; set; }

            [Required(ErrorMessage = "E-posta boş olamaz")]
            [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz")]
            [StringLength(100)]
            public string Eposta { get; set; }
        }
    }
}
