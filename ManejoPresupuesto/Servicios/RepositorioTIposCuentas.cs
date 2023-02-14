using Dapper;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios.Interfaces;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public class RepositorioTiposCuentas : IRepositorioTiposCuentas
     {
        private readonly string ConnectionString;
        public RepositorioTiposCuentas(IConfiguration configuration)
        {
            ConnectionString = configuration.GetConnectionString("DefaulConnection");
        }


        public async Task Crear (TipoCuenta tipocuenta) {
            using var connection = new SqlConnection(ConnectionString);
            var id = await connection.QuerySingleAsync<int>("SP_TiposCuentas_Insertar", new { usuarioId = tipocuenta.UsuarioId , 
                                                                                            nombre = tipocuenta.Nombre},
                                                                                            commandType: System.Data.CommandType.StoredProcedure);


//            CREATE PROCEDURE SP_TiposCuentas_Insertar

//@Nombre nvarchar(50),
//@UsuarioId int

//AS
//BEGIN

//    SET NOCOUNT ON;

//            DECLARE @Orden int;
//            SELECT @Orden = COALESCE(MAX(Orden), 0) + 1
//   FROM TiposCuentas
//   WHERE UsuarioId = @UsuarioId-- Por cada usuario

//   INSERT INTO TiposCuentas(Nombre, UsuarioId, Orden) VALUES(@Nombre, @UsuarioId, @Orden);

//            SELECT SCOPE_IDENTITY();

//            END
//            GO


            tipocuenta.Id= id;
        }

        public async Task<bool> Existe(string nombre, int usuarioId)
        {
            using var connection = new SqlConnection(ConnectionString);
            var existe = await connection.QueryFirstOrDefaultAsync<int>(
                                            @"SELECT 1 FROM TiposCuentas
                                            WHERE  Nombre = @Nombre AND UsuarioId = @UsuarioId;",
                                            new { nombre, usuarioId });
            return existe == 1;
        }

        public async Task<IEnumerable<TipoCuenta>> ObtenerTodos (int usuarioId)
        {
            using var connection = new SqlConnection(ConnectionString);
            return await connection.QueryAsync<TipoCuenta>(@"SELECT Id, Nombre, Orden FROM TiposCuentas WHERE UsuarioId = @usuarioId ORDER BY Orden", new {usuarioId});
        }

        public async Task Actualizar (TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(ConnectionString);
            await connection.ExecuteAsync(@"UPDATE TiposCuentas SET Nombre = @Nombre WHERE Id = @Id", tipoCuenta);
        }

        public async Task<TipoCuenta> ObtenerCuentaPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(ConnectionString);
            return await connection.QueryFirstOrDefaultAsync<TipoCuenta>
                (@"SELECT Id, Nombre, Orden FROM TiposCUentas 
                WHERE Id = @Id AND UsuarioId = @UsuarioId", new { id, usuarioId});
        }

        public async Task Borrar (int id)
        {
            using var connection = new SqlConnection(ConnectionString);
            await connection.ExecuteAsync(@"DELETE TiposCuentas WHERE Id = @Id", new { id });
        }

        public async Task Ordenar(IEnumerable<TipoCuenta> tiposCuentasOrdenados)
        {
            var query = "UPDATE TiposCuentas SET Orden = @Orden WHERE Id = @Id;";
            using var connection = new SqlConnection(ConnectionString);
            await connection.ExecuteAsync(query, tiposCuentasOrdenados);
        }
    }
}
