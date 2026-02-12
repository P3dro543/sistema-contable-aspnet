using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using SistemaContable.Entities;

namespace SistemaContable.Services
{
    public class AsientoService
    {
        private readonly string _cadena;


        public AsientoService(IConfiguration configuration)
        {
            _cadena = configuration.GetConnectionString("MySqlConnection");
        }

        // ============================
        // CREAR ASIENTO CON DETALLE
        // ============================
        public void Crear(Asiento asiento)
        {
            using var con = new MySqlConnection(_cadena);
            con.Open();

            using var tx = con.BeginTransaction();

            try
            {
                var cmd = new MySqlCommand(@"
                    INSERT INTO asientos
                    (consecutivo, fecha, codigo, referencia, estado)
                    VALUES (@c,@f,@co,@r,@e)", con, tx);

                cmd.Parameters.AddWithValue("@c", asiento.Consecutivo);
                cmd.Parameters.AddWithValue("@f", asiento.Fecha);
                cmd.Parameters.AddWithValue("@co", asiento.Codigo);
                cmd.Parameters.AddWithValue("@r", asiento.Referencia);
                cmd.Parameters.AddWithValue("@e", asiento.Estado);

                cmd.ExecuteNonQuery();

                int id = (int)cmd.LastInsertedId;

                foreach (var d in asiento.Detalles)
                {
                    var det = new MySqlCommand(@"
                        INSERT INTO detalle_asiento
                        (asiento_id, cuenta, tipo, monto, descripcion)
                        VALUES (@a,@cu,@t,@m,@d)", con, tx);

                    det.Parameters.AddWithValue("@a", id);
                    det.Parameters.AddWithValue("@cu", d.Cuenta);
                    det.Parameters.AddWithValue("@t", d.TipoMovimiento);
                    det.Parameters.AddWithValue("@m", d.Monto);
                    det.Parameters.AddWithValue("@d", d.Descripcion);

                    det.ExecuteNonQuery();
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        // ============================
        // OBTENER ASIENTOS
        // ============================
        public List<Asiento> Obtener()
        {
            var lista = new List<Asiento>();

            using var con = new MySqlConnection(_cadena);
            con.Open();

            var cmd = new MySqlCommand("SELECT * FROM asientos", con);
            using var dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(new Asiento
                {
                    Id = dr.GetInt32("id_asiento"),

                    // Consecutivo: Es int en DB. Usamos Convert para manejar nulos y convertir a string
                    Consecutivo = dr["consecutivo"] != DBNull.Value ? dr["consecutivo"].ToString() : "0",

                    Fecha = dr.GetDateTime("fecha"),

                    // Codigo: Si es nulo, le ponemos un texto vacío ""
                    Codigo = dr["codigo"] != DBNull.Value ? dr["codigo"].ToString() : "",

                    // Referencia: Si es nulo, le ponemos un texto vacío ""
                    Referencia = dr["referencia"] != DBNull.Value ? dr["referencia"].ToString() : "",

                    // Estado: Usamos el nombre real de la columna 'id_estado'
                    Estado = dr["id_estado"] != DBNull.Value ? dr["id_estado"].ToString() : "0"
                });
            }

            return lista;
        }

        // ============================
        // CAMBIOS DE ESTADO
        // ============================
        public void Aprobar(int id) => CambiarEstado(id, "Aprobado");
        public void Rechazar(int id) => CambiarEstado(id, "Rechazado");
        public void Anular(int id) => CambiarEstado(id, "Anulado");

        private void CambiarEstado(int id, string estado)
        {
            using var con = new MySqlConnection(_cadena);
            con.Open();

            var cmd = new MySqlCommand(
                "UPDATE asientos SET estado=@e WHERE id=@i", con);

            cmd.Parameters.AddWithValue("@e", estado);
            cmd.Parameters.AddWithValue("@i", id);

            cmd.ExecuteNonQuery();
        }
    }
}
