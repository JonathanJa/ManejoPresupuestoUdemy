using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;
using Dapper;
namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioCuenta
    {
        Task Actualizar(CuentaCreacionViewModel cuenta);
        Task Borrar(int id);
        Task<IEnumerable<Cuenta>> Buscar(int usuarioId);
        Task Crear(Cuenta cuenta);
        Task<Cuenta> ObtenerPorId(int id, int usuarioId);
    }
    public class RepositorioCuenta:IRepositorioCuenta
    {
        private readonly string StringConnection;

        public RepositorioCuenta(IConfiguration configuration)
        {
            StringConnection = configuration.GetConnectionString("DefaultConnection");
            
        }

        public async Task Crear(Cuenta cuenta)
        {
            using var connection = new SqlConnection(StringConnection);
            var id = await connection.QuerySingleAsync<int>(@"insert into Cuentas (nombre,tipocuentaid,balance,descripcion) values(@nombre,@tipocuentaid,@balance,@descripcion)
                                                        select SCOPE_IDENTITY();", cuenta);
            cuenta.Id = id;
        }

        public async Task<IEnumerable<Cuenta>> Buscar(int usuarioId)
        {
            using var connection = new SqlConnection(StringConnection);
            return await connection.QueryAsync<Cuenta>(@"select Cuentas.id, Cuentas.nombre,tc.nombre as TipoCuenta, balance,descripcion from Cuentas 
                                                            inner join TiposCuentas tc on tc.id = Cuentas.tipocuentaid where usuarioid = @usuarioid order by orden", new { usuarioId });
        }

        public async Task<Cuenta>ObtenerPorId(int id,int usuarioId)
        {
            using var connection = new SqlConnection(StringConnection);
            return await connection.QueryFirstOrDefaultAsync<Cuenta>(@" select Cuentas.id,Cuentas.nombre, balance,descripcion,TipoCuentaId from Cuentas 
                                                                     inner join TiposCuentas tc on tc.id = Cuentas.tipocuentaid 
                                                                     where usuarioid = @usuarioid and Cuentas.id = @id", new {id,usuarioId});
        }

        public async Task Actualizar(CuentaCreacionViewModel cuenta)
        {
            using var connection = new SqlConnection(StringConnection);
            await connection.ExecuteAsync(@"update Cuentas set nombre = @nombre,tipocuentaid = @tipocuentaid, 
                                            balance = @balance, descripcion = @descripcion where id = @id", cuenta);
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(StringConnection);
            await connection.ExecuteAsync(@"delete from Cuentas where Id = @Id", new { id });
        }
    }
}
