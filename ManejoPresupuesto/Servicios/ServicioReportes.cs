using ManejoPresupuesto.Models;

namespace ManejoPresupuesto.Servicios
{
    public interface IServicioReportes
    {
        Task<IEnumerable<ResultadoObtenerPorSemana>> obtenerPorSemana(int UsuarioId, int mes, int año, dynamic ViewBag);
        Task<reporteTransaccionesDetalladas> obtenerReportesTransaccionesDetalladas(int UsuarioId, int mes, int año, dynamic ViewBag);
        Task<reporteTransaccionesDetalladas> obtenerReporteTransaccionesDetalladasPorCuenta(int usuarioId, int CuentaId, int mes, int año, dynamic ViewBag);
    }
    public class ServicioReportes: IServicioReportes
    {
        private readonly IRepositorioTransacciones repositorioTransacciones;
        private readonly HttpContext httpContext;

        public ServicioReportes(IRepositorioTransacciones repositorioTransacciones,IHttpContextAccessor  httpContextAccessor)
        {
            this.repositorioTransacciones = repositorioTransacciones;
            this.httpContext = httpContextAccessor.HttpContext;
        }

        public async Task<IEnumerable<ResultadoObtenerPorSemana>> obtenerPorSemana(int UsuarioId,int mes,int año,dynamic ViewBag)
        {
            (DateTime FechaInicio, DateTime FechaFin) = GenerarFechaInicioYFin(mes, año);
            var parametroUsuario = new ParametroObtenerTransaccionesPorUsuario
            {
                UsuarioId = UsuarioId,
                FechaInicio = FechaInicio,
                FechaFin = FechaFin
            };
            AsignarValoresAlViewBag(ViewBag, FechaInicio);
            var modelo = await repositorioTransacciones.obtenerPorSemana(parametroUsuario);
            return modelo;

        }



        public async Task<reporteTransaccionesDetalladas> obtenerReportesTransaccionesDetalladas(int UsuarioId,int mes,int año,dynamic ViewBag)
        {
            (DateTime FechaInicio, DateTime FechaFin) = GenerarFechaInicioYFin(mes, año);
            var parametroUsuario = new ParametroObtenerTransaccionesPorUsuario
            {
                UsuarioId = UsuarioId,
                FechaInicio = FechaInicio,
                FechaFin = FechaFin
            };
            var transacciones = await repositorioTransacciones.ObtenerPorUsuario(parametroUsuario);
            var modelo = GenerarReportesTransaccionesDetalladas(FechaInicio, FechaFin, transacciones);
            AsignarValoresAlViewBag(ViewBag, FechaInicio);
            return modelo;
        }


        public async Task<reporteTransaccionesDetalladas> obtenerReporteTransaccionesDetalladasPorCuenta(int usuarioId,int CuentaId,int mes,int año,dynamic ViewBag)
        {
            (DateTime FechaInicio, DateTime FechaFin) = GenerarFechaInicioYFin(mes, año);
            var cuentaTransaccion = new obtenerTransaccionesPorCuenta
            {
                CuentaId = CuentaId,
                usuarioId = usuarioId,

                FechaInicio = FechaInicio,
                FechaFin = FechaFin,
            };
            var transacciones = await repositorioTransacciones.ObtenerPorCuenta(cuentaTransaccion);
            var modelo = GenerarReportesTransaccionesDetalladas(FechaInicio, FechaFin, transacciones);
            AsignarValoresAlViewBag(ViewBag, FechaInicio);

            return modelo;
        }

        private void AsignarValoresAlViewBag(dynamic ViewBag, DateTime FechaInicio)
        {
            ViewBag.mesAnterior = FechaInicio.AddMonths(-1).Month;
            ViewBag.añoAnterior = FechaInicio.AddMonths(-1).Year;
            ViewBag.mesPosterior = FechaInicio.AddMonths(1).Month;
            ViewBag.añoPosterior = FechaInicio.AddMonths(1).Year;
            ViewBag.UrlRetorno = httpContext.Request.Path + httpContext.Request.QueryString;
        }

        private static reporteTransaccionesDetalladas GenerarReportesTransaccionesDetalladas(DateTime FechaInicio, DateTime FechaFin, IEnumerable<Transaccion> transacciones)
        {
            var modelo = new reporteTransaccionesDetalladas();

            var transaccionesPorFecha = transacciones.OrderByDescending(x => x.FechaTransaccion)
                .GroupBy(x => x.FechaTransaccion)
                .Select(grupo => new reporteTransaccionesDetalladas.TransaccionesPorFecha()
                {
                    FechaTransaccion = grupo.Key,
                    Transacciones = grupo.AsEnumerable()
                });

            modelo.TransaccionesObtenidos = transaccionesPorFecha;
            modelo.FechaInicio = FechaInicio;
            modelo.FechaFin = FechaFin;
            return modelo;
        }

        private (DateTime FechaInicio,DateTime FechaFin) GenerarFechaInicioYFin(int mes,int año)
        {
            DateTime fechaInicio;
            DateTime fechaFin;

            if (mes <= 0 || mes > 12 || año <= 1900)
            {
                var hoy = DateTime.Today;
                fechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
            }
            else
            {
                fechaInicio = new DateTime(año, mes, 1);
            }
            fechaFin = fechaInicio.AddMonths(1).AddDays(-1); //muestra la fecha en pantalla del ultimo mes
            return (fechaInicio, fechaFin);
        }
    }
}
