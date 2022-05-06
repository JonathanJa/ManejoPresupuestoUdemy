using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioUsuario
    {
        Task<Usuario> BuscarUsuarioPorEmail(string EmailNormalizado);
        Task<int> CrearUsuario(Usuario usuario);
    }
    public class RepositorioUsuario: IRepositorioUsuario
    {
        private readonly string connectioString;
        public RepositorioUsuario(IConfiguration configuration)
        {
            connectioString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<int> CrearUsuario(Usuario usuario)
        {
            
            using var connection = new SqlConnection(connectioString);
            var usuarioId = await connection.QuerySingleAsync<int>(@"insert into Usuarios(Email,EmailNormalizado,PasswordHash)
                                                            values(@Email,@EmailNormalizado,@PasswordHash);
                                                            select SCOPE_IDENTITY();", usuario);
            await connection.ExecuteAsync("CrearDatosUsuariosNuevos", new { usuarioId }, commandType:System.Data.CommandType.StoredProcedure);
            return usuarioId;
        }

        public async Task<Usuario> BuscarUsuarioPorEmail(string EmailNormalizado)
        {
            using var connection = new SqlConnection(connectioString);
            var usuario = await connection.QueryFirstOrDefaultAsync<Usuario>("select * from Usuarios where EmailNormalizado = @EmailNormalizado", new {EmailNormalizado});
            return usuario;
        }
    }
}
