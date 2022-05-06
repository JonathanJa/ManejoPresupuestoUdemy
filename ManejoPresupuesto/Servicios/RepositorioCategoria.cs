using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioCategoria
    {
        Task actualizar(Categoria categoria);
        Task Borrar(int id);
        Task<IEnumerable<Categoria>> Buscar(int id, PaginacionViewModel pagina);
        Task<IEnumerable<Categoria>> Buscar(int usuarioid, TipoOperacion tipoOperacionid);
        Task<int> Contar(int usuarioId);
        Task Crear(Categoria categoria);
        
        Task<Categoria> ObtenerId(int id, int usuarioId);
    }
    public class RepositorioCategoria:IRepositorioCategoria
    {
        private readonly string connecionString;

        public RepositorioCategoria(IConfiguration configuration)
        {
            connecionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Categoria categoria)
        {
            using var connection = new SqlConnection(connecionString);
            var Id = await connection.QuerySingleAsync<int>(@"insert into Categorias (nombre,tipooperacionid,usuarioid)
                                                        values (@nombre,@tipooperacionid,@usuarioid);
                                                        select SCOPE_IDENTITY();", categoria);
            categoria.Id = Id;
        }

        public async Task<IEnumerable<Categoria>> Buscar(int usuarioid, PaginacionViewModel pagina)
        {
            using var connection = new SqlConnection(connecionString);
            return await connection.QueryAsync<Categoria>(@$"select * from Categorias 
                                                             where usuarioid = @usuarioid order by Nombre
                                                             OFFSET {pagina.RecordsAsaltar} ROWS FETCH NEXT {pagina.RecordsPorPagina} ROWS ONLY", new { usuarioid });
        }

        public async Task<int>Contar(int usuarioId)
        {
            using var connection = new SqlConnection(connecionString);
            return await connection.ExecuteScalarAsync<int>(@"select count(*) from Categorias where usuarioid = @usuarioId", new { usuarioId });
        }
        public async Task<IEnumerable<Categoria>> Buscar(int usuarioid,TipoOperacion tipoOperacionid)
        {
            using var connection = new SqlConnection(connecionString);
            return await connection.QueryAsync<Categoria>(@"select * from Categorias 
                                                          where usuarioid = @usuarioid 
                                                          and tipooperacionid = @tipooperacionid ", new { usuarioid,tipoOperacionid });
        }
        public async Task<Categoria> ObtenerId (int id,int usuarioId)
        {
            using var connection = new SqlConnection(connecionString);
            return await connection.QueryFirstOrDefaultAsync<Categoria>(@"select * from Categorias where id=@id and usuarioid=@usuarioid", new {id,usuarioId});
        }

        public async Task actualizar(Categoria categoria)
        {
            using var connection = new SqlConnection(connecionString);
            await connection.ExecuteAsync(@"update Categorias set nombre = @nombre,tipooperacionid=@tipooperacionid where id = @id", categoria);
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connecionString);
            await connection.ExecuteAsync(@"delete from Categorias where id = @id", new { id });
        }
    }
}
