using MySql.Data.MySqlClient;
using SistemaContable.Entities;

namespace SistemaContable.Repository
{
    public class AsientoRepository
    {
        private readonly string _connectionString;

        public AsientoRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // =============================
        // OBTENER TODOS LOS ASIENTOS
        // =============================
        public List<Asiento> Obtener()
        {
            List<Asiento> lista = new();

            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            string sql = "SELECT * FROM asientos";

            using var cmd = new MySqlCommand(sql, conn);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Asiento a = new Asiento
                {
                    Id = reader.GetInt32("Id"),
                    Consecutivo = reader.GetString("Consecutivo"),
                    Fecha = reader.GetDateTime("Fecha"),
                    Codigo = reader.GetString("Codigo"),
                    Referencia = reader.GetString("Referencia"),
                    Estado = reader.GetString("Estado"),
                    Detalles = new List<DetalleAsiento>()
                };

                lista.Add(a);
            }

            reader.Close();

            // Cargar detalles
            foreach (var a in lista)
            {
                a.Detalles = ObtenerDetalles(a.Id, conn);
            }

            return lista;
        }

        // =============================
        // OBTENER DETALLES
        // =============================
        private List<DetalleAsiento> ObtenerDetalles(int asientoId, MySqlConnection conn)
        {
            List<DetalleAsiento> detalles = new();

            string sql = "SELECT * FROM detalle_asientos WHERE AsientoId=@id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", asientoId);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                DetalleAsiento d = new DetalleAsiento
                {
                    Id = reader.GetInt32("Id"),
                    AsientoId = asientoId,
                    Cuenta = reader.GetString("Cuenta"),
                    TipoMovimiento = reader.GetString("TipoMovimiento"),
                    Monto = reader.GetDecimal("Monto"),
                    Descripcion = reader.GetString("Descripcion")
                };

                detalles.Add(d);
            }

            reader.Close();

            return detalles;
        }

        // =============================
        // GUARDAR ASIENTO
        // =============================
        public void Guardar(Asiento asiento)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            using var tran = conn.BeginTransaction();

            try
            {
                // Insertar encabezado
                string sql = @"
                INSERT INTO asientos
                (Consecutivo, Fecha, Codigo, Referencia, Estado)
                VALUES
                (@c,@f,@co,@r,@e);
                SELECT LAST_INSERT_ID();";

                using var cmd = new MySqlCommand(sql, conn, tran);

                cmd.Parameters.AddWithValue("@c", asiento.Consecutivo);
                cmd.Parameters.AddWithValue("@f", asiento.Fecha);
                cmd.Parameters.AddWithValue("@co", asiento.Codigo);
                cmd.Parameters.AddWithValue("@r", asiento.Referencia);
                cmd.Parameters.AddWithValue("@e", asiento.Estado);

                int id = Convert.ToInt32(cmd.ExecuteScalar());

                // Insertar detalles
                foreach (var d in asiento.Detalles)
                {
                    string sqlDet = @"
                    INSERT INTO detalle_asientos
                    (AsientoId, Cuenta, TipoMovimiento, Monto, Descripcion)
                    VALUES
                    (@a,@cu,@t,@m,@d)";

                    using var cmdDet = new MySqlCommand(sqlDet, conn, tran);

                    cmdDet.Parameters.AddWithValue("@a", id);
                    cmdDet.Parameters.AddWithValue("@cu", d.Cuenta);
                    cmdDet.Parameters.AddWithValue("@t", d.TipoMovimiento);
                    cmdDet.Parameters.AddWithValue("@m", d.Monto);
                    cmdDet.Parameters.AddWithValue("@d", d.Descripcion);

                    cmdDet.ExecuteNonQuery();
                }

                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        // =============================
        // VALIDAR BALANCE
        // =============================
        public bool EstaBalanceado(Asiento asiento)
        {
            decimal debe = asiento.Detalles
                .Where(d => d.TipoMovimiento == "Debe")
                .Sum(d => d.Monto);

            decimal haber = asiento.Detalles
                .Where(d => d.TipoMovimiento == "Haber")
                .Sum(d => d.Monto);

            return debe == haber;
        }
    }
}

