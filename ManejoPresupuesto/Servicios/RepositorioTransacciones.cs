using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioTransacciones
    {
        Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaIdAnterior);
        Task Borrar(int id);
        Task Crear(Transaccion transaccion);
        Task<IEnumerable<Transaccion>> ObtenerPorCuenta(obtenerTransaccionesPorCuenta cuenta);
        Task<Transaccion> ObtenerPorId(int id, int UsuarioId);
        Task<IEnumerable<ResultadoObtenerPorMes>> ObtenerPorMes(int usuarioId, int año);
        Task<IEnumerable<ResultadoObtenerPorSemana>> obtenerPorSemana(ParametroObtenerTransaccionesPorUsuario modelo);
        Task<IEnumerable<Transaccion>> ObtenerPorUsuario(ParametroObtenerTransaccionesPorUsuario cuenta);
    }
    public class RepositorioTransacciones: IRepositorioTransacciones
    {
        public readonly string connectionString;
        public RepositorioTransacciones(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Transaccion transaccion)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(@"Transaccion_Crear",
                new
                {
                    transaccion.UsuarioId,
                    transaccion.FechaTransaccion,
                    transaccion.Monto,
                    transaccion.CuentaId,
                    transaccion.CategoriaId,
                    transaccion.Nota
                }, commandType : System.Data.CommandType.StoredProcedure);
            transaccion.Id = id;
        }

        public async Task Actualizar(Transaccion transaccion,decimal montoAnterior,int cuentaIdAnterior)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"Transaccion_Actualizar", new
            {
                transaccion.Id,
                transaccion.FechaTransaccion,
                transaccion.CuentaId,
                transaccion.CategoriaId,
                transaccion.Monto,
                transaccion.Nota,
                montoAnterior,
                cuentaIdAnterior
            }, commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<Transaccion> ObtenerPorId(int id, int UsuarioId)
        {
            using var connection = new SqlConnection(connectionString);
           return await connection.QueryFirstOrDefaultAsync<Transaccion>(@"select Transacciones.*,cat.TipoOperacionId 
                                                            from Transacciones 
                                                            inner join Categorias cat 
                                                            on cat.Id = Transacciones.CategoriaId
                                                            where Transacciones.Id = @id 
                                                            and Transacciones.UsuarioId = @usuarioId;", new { id, UsuarioId });
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("Transacciones_Borrar", new { id }, commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorCuenta(obtenerTransaccionesPorCuenta cuenta)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Transaccion>(@"select t.Id,t.Monto, t.FechaTransaccion,
                                                            c.Nombre as categoria,
                                                            cu.Nombre as cuenta,
                                                            c.TipoOperacionId 
                                                            from Transacciones t 
                                                            inner join Categorias c on c.Id = t.CategoriaId
                                                            inner join Cuentas cu on cu.Id = t.CuentaId 
                                                            where t.CuentaId = @CuentaId and t.UsuarioId = @usuarioId 
                                                            and FechaTransaccion between @FechaInicio and @FechaFin", cuenta);
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorUsuario(ParametroObtenerTransaccionesPorUsuario cuenta)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Transaccion>(@"select t.Id,t.Monto, t.FechaTransaccion,
                                                            c.Nombre as categoria,
                                                            cu.Nombre as cuenta,
                                                            c.TipoOperacionId,Nota 
                                                            from Transacciones t 
                                                            inner join Categorias c on c.Id = t.CategoriaId
                                                            inner join Cuentas cu on cu.Id = t.CuentaId 
                                                            where t.UsuarioId = @usuarioId 
                                                            and FechaTransaccion between @FechaInicio and @FechaFin order by t.FechaTransaccion desc", cuenta);
        }

        public async Task<IEnumerable <ResultadoObtenerPorSemana>> obtenerPorSemana(ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<ResultadoObtenerPorSemana>(@"select DATEDIFF(d,@FechaInicio,FechaTransaccion)/7+1 as Semana,
                                                                   SUM(Monto) as Monto,cat.TipoOperacionId 
                                                                   from Transacciones inner join Categorias cat on cat.Id = Transacciones.CategoriaId 
                                                                   where Transacciones.UsuarioId = @usuarioId and 
                                                                   FechaTransaccion between @FechaInicio and @FechaFin  
                                                                   group by  DATEDIFF(d,@FechaInicio,FechaTransaccion)/7, cat.TipoOperacionId", modelo);
        }

        public async Task<IEnumerable<ResultadoObtenerPorMes>>ObtenerPorMes(int usuarioId, int año)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<ResultadoObtenerPorMes>(@"select MONTH(FechaTransaccion) as Mes, SUM(Monto) as Monto,
                                                                        cat.TipoOperacionId from Transacciones 
                                                                        inner join Categorias cat on cat.Id = Transacciones.CategoriaId 
                                                                        where Transacciones.UsuarioId = @usuarioId 
                                                                        and YEAR(FechaTransaccion) = @Año 
                                                                        group by MONTH(FechaTransaccion),cat.TipoOperacionId;", new { usuarioId, año });
        }
    }
}
