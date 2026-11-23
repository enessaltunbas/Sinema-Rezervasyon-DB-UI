using System;
using System.Data;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace SinemaApp.Data
{
    // Veritabanı işlemleri için
    // Sayfalarda tekrar tekrar connection açıp kapatmamak için
    public class Db
    {
        private readonly string baglantiCumlesi;

        public Db(IConfiguration config)
        {
            baglantiCumlesi = config.GetConnectionString("SinemaDb");
        }

        public MySqlConnection BaglantiAc()
        {
            var con = new MySqlConnection(baglantiCumlesi);
            con.Open();
            return con;
        }


        public DataTable Tablo(string sql, params MySqlParameter[] parametreler)
        {
            using var con = BaglantiAc();
            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddRange(parametreler);
            var dt = new DataTable();
            using var reader = cmd.ExecuteReader();
            dt.Load(reader);
            return dt;
        }

        // Tek satır gerekiyorsa
        public DataRow Satir(string sql, params MySqlParameter[] parametreler)
        {
            var dt = Tablo(sql, parametreler);
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        // Tek değer döndüren sorgular
        public object Deger(string sql, params MySqlParameter[] parametreler)
        {
            using var con = BaglantiAc();
            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddRange(parametreler);
            var sonuc = cmd.ExecuteScalar();
            return sonuc == DBNull.Value ? null : sonuc;
        }

        // INSERT / UPDATE / DELETE / CALL
        public int Calistir(string sql, params MySqlParameter[] parametreler)
        {
            using var con = BaglantiAc();
            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddRange(parametreler);
            return cmd.ExecuteNonQuery();
        }

        // INSERT sonrası oluşan AUTO_INCREMENT değerini almak 
        public long Ekle(string sql, params MySqlParameter[] parametreler)
        {
            using var con = BaglantiAc();
            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddRange(parametreler);
            cmd.ExecuteNonQuery();
            return cmd.LastInsertedId;
        }

        public static MySqlParameter P(string ad, object deger)
        {
            return new MySqlParameter(ad, deger ?? DBNull.Value);
        }

        // MySQL hatalarını kullanıcıya gösterilecek hale getirir
        // Trigger SIGNAL mesajlar zaten okunabilir olduğu için direkt gösteriliyor
        public static string HataMesaji(MySqlException ex)
        {
            if (ex.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
                return "Bu kayıt zaten var (benzersiz olması gereken bir alan tekrar edildi).";

            if (ex.ErrorCode == MySqlErrorCode.RowIsReferenced2)
                return "Bu kayda bağlı başka kayıtlar olduğu için silinemez.";


            if (ex.Number == 3819)
                return "Girilen değer kurallara uymuyor: " + ex.Message;

            return ex.Message;
        }
    }
}
