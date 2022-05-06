using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioTiposCuentas
    {
        Task Actualizar(TiposCuentas tiposCuentas);
        Task Borrar(int id);
        Task Crear(TiposCuentas tiposCuentas);
        Task<bool> Existe(string Nombre, int UsuarioId);
        Task<IEnumerable<TiposCuentas>> Obtener(int UsuarioId);
        Task<TiposCuentas> ObtenerPorId(int Id, int UsuarioId);
        Task Ordenar(IEnumerable<TiposCuentas> tiposCuentasOrdenar);
    }
    public class RepositorioTiposCuentas:IRepositorioTiposCuentas
    {
        private readonly string ConnectionString;
        public RepositorioTiposCuentas(IConfiguration configuration)
        {
            ConnectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(TiposCuentas tiposCuentas)
        {
            using var connection = new SqlConnection(ConnectionString);
            var id = await connection.QuerySingleAsync<int>("TiposCuentas_Insertar",
                new{UsuarioId= tiposCuentas.UsuarioId,Nombre=tiposCuentas.Nombre},
                commandType:System.Data.CommandType.StoredProcedure);//Query single se utiliza para extraer por id es decir guarda datos de acuedo al id
            tiposCuentas.Id = id;
        }

        public async Task<bool> Existe(string Nombre,int UsuarioId)
        {
            using var connection = new SqlConnection(ConnectionString);
            var exito = await connection.QueryFirstOrDefaultAsync<int>(@"select 1 from TiposCuentas 
                                                                where Nombre = @Nombre 
                                                                and UsuarioId=@UsuarioId", new { Nombre, UsuarioId });
            return exito == 1;
        }

        public async Task<IEnumerable<TiposCuentas>> Obtener(int UsuarioId)
        {
            using var connection = new SqlConnection(ConnectionString);
            return await connection.QueryAsync<TiposCuentas>(@"select Id,Nombre,Orden 
                                                            from TiposCuentas 
                                                            where UsuarioId=@UsuarioId order by Orden", new { UsuarioId });
        }

        public async Task Actualizar(TiposCuentas tiposCuentas)
        {
            using var connection = new SqlConnection(ConnectionString);
            await connection.ExecuteAsync(@"update TiposCuentas 
                                            set Nombre = @nombre 
                                            where Id = @id;", tiposCuentas);//ExecuteAsync no devuelve nada es por eso que se lo utiliza para hacer una actualizacion
        }

        public async Task<TiposCuentas> ObtenerPorId(int Id, int UsuarioId)
        {
            using var connection = new SqlConnection(ConnectionString);
            return await connection.QueryFirstOrDefaultAsync<TiposCuentas>(@"select Id,Nombre,Orden from TiposCuentas 
                                                                           where Id=@Id and UsuarioId=@UsuarioId", new { Id, UsuarioId });

        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(ConnectionString);
            await connection.ExecuteAsync(@"delete from TiposCuentas where Id=@id", new { id });
        }


        public async Task Ordenar(IEnumerable<TiposCuentas> tiposCuentasOrdenar)
        {
            var query = "update TiposCuentas set Orden = @Orden where Id = @Id";
            using var connection = new SqlConnection(ConnectionString);
            await connection.ExecuteAsync(query, tiposCuentasOrdenar);
        }
    }
}
