using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using SistemaContable.Entities;
using System.Data;

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
                // Si no hay periodo, buscar el activo
                if (asiento.IdPeriodo == 0)
                {
                    var cmdP = new MySqlCommand("SELECT id_periodo FROM periodos_contables WHERE estado = 1 ORDER BY anio DESC, mes DESC LIMIT 1", con, tx);
                    var resP = cmdP.ExecuteScalar();
                    if (resP != null) asiento.IdPeriodo = Convert.ToInt32(resP);
                    else throw new Exception("No hay un periodo contable activo.");
                }

                var cmd = new MySqlCommand(@"
                    INSERT INTO asientos
                    (consecutivo, fecha, referencia, id_estado, id_periodo)
                    VALUES (@c,@f,@r,@e,@p)", con, tx);

                cmd.Parameters.AddWithValue("@c", asiento.Consecutivo);
                cmd.Parameters.AddWithValue("@f", asiento.Fecha);
                cmd.Parameters.AddWithValue("@r", asiento.Referencia ?? "");
                cmd.Parameters.AddWithValue("@e", asiento.IdEstado == 0 ? 1 : asiento.IdEstado); // Default 1=Borrador
                cmd.Parameters.AddWithValue("@p", asiento.IdPeriodo);

                cmd.ExecuteNonQuery();

                int id = (int)cmd.LastInsertedId;

                foreach (var d in asiento.Detalles)
                {
                    var det = new MySqlCommand(@"
                        INSERT INTO asiento_detalle
                        (id_asiento, id_cuenta, tipo_movimiento, monto)
                        VALUES (@a,@cu,@t,@m)", con, tx);

                    det.Parameters.AddWithValue("@a", id);
                    det.Parameters.AddWithValue("@cu", Convert.ToInt32(d.Cuenta));
                    det.Parameters.AddWithValue("@t", d.TipoMovimiento);
                    det.Parameters.AddWithValue("@m", d.Monto);

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

            var cmd = new MySqlCommand("SELECT * FROM asientos ORDER BY fecha DESC, id_asiento DESC", con);
            using var dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(new Asiento
                {
                    Id = dr.GetInt32("id_asiento"),
                    Consecutivo = dr.GetInt32("consecutivo"),
                    Fecha = dr.GetDateTime("fecha"),
                    Referencia = dr["referencia"] != DBNull.Value ? dr["referencia"].ToString() : "",
                    IdEstado = dr.GetInt32("id_estado"),
                    IdPeriodo = dr.GetInt32("id_periodo")
                });
            }
            dr.Close();

            // Cargar detalles
            foreach (var a in lista)
            {
                a.Detalles = ObtenerDetalles(a.Id, con);
            }

            return lista;
        }

        private List<DetalleAsiento> ObtenerDetalles(int asientoId, MySqlConnection con)
        {
            List<DetalleAsiento> detalles = new();

            string sql = "SELECT * FROM asiento_detalle WHERE id_asiento=@id";

            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@id", asientoId);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                DetalleAsiento d = new DetalleAsiento
                {
                    Id = reader.GetInt32("id_detalle"),
                    AsientoId = asientoId,
                    Cuenta = reader.GetInt32("id_cuenta").ToString(),
                    TipoMovimiento = reader.GetString("tipo_movimiento"),
                    Monto = reader.GetDecimal("monto"),
                    Descripcion = "" // No existe en DB de Geral
                };

                detalles.Add(d);
            }

            reader.Close();

            return detalles;
        }

        // ============================
        // CAMBIOS DE ESTADO
        // ============================
        public void Aprobar(int id) => CambiarEstado(id, 3); // 3=Aprobado
        public void Rechazar(int id) => CambiarEstado(id, 4); // 4=Rechazado
        public void Anular(int id) => CambiarEstado(id, 5); // 5=Anulado

        private void CambiarEstado(int id, int estado)
        {
            using var con = new MySqlConnection(_cadena);
            con.Open();

            var cmd = new MySqlCommand(
                "UPDATE asientos SET id_estado=@e WHERE id_asiento=@i", con);

            cmd.Parameters.AddWithValue("@e", estado);
            cmd.Parameters.AddWithValue("@i", id);

            cmd.ExecuteNonQuery();
        }
    }
}
