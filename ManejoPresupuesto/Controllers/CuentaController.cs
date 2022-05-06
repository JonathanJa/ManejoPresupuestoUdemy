
using AutoMapper;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManejoPresupuesto.Controllers
{
    public class CuentaController:Controller
    {
        private readonly IRepositorioTiposCuentas repositorioTiposCuentas;
        private readonly IServicioUsuario servicioUsuario;
        private readonly IRepositorioCuenta repositorioCuenta;
        private readonly IMapper mapper;

      private readonly IRepositorioTransacciones repositorioTransacciones;
        private readonly IServicioReportes servicioReportes;

        //private readonly IMapper mapper;
        //private readonly IRepositorioTransaccion repositorioTransaccion;

        public CuentaController(IRepositorioTiposCuentas repositorioTiposCuentas, IServicioUsuario servicioUsuario,IRepositorioCuenta repositorioCuenta,IMapper mapper,IRepositorioTransacciones repositorioTransacciones,IServicioReportes servicioReportes)
        {
            this.repositorioTiposCuentas = repositorioTiposCuentas;
            this.servicioUsuario = servicioUsuario;
            this.repositorioCuenta = repositorioCuenta;
            this.mapper = mapper;
            this.repositorioTransacciones = repositorioTransacciones;
            this.servicioReportes = servicioReportes;
            //this.mapper = mapper;
            //this.repositorioTransaccion = repositorioTransaccion;
        }
        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var usuarioId = servicioUsuario.obtenerUsuarioId();
            //var tipoCuenta = await repositorioCuenta.obtener(usuarioId);
            var modelo = new CuentaCreacionViewModel();
            modelo.tipocuenta =await ObtenerTipoCuenta(usuarioId);
            return View(modelo);
        }
        [HttpPost]
        public async Task<IActionResult> Crear(CuentaCreacionViewModel cuenta)
        {
            var usuarioId = servicioUsuario.obtenerUsuarioId();
            var TipoCuenta = await repositorioTiposCuentas.ObtenerPorId(cuenta.TipoCuentaId,usuarioId);
            if(TipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            if (!ModelState.IsValid)
            {
                cuenta.tipocuenta = await ObtenerTipoCuenta(usuarioId);
                return View(cuenta);
            }

            await repositorioCuenta.Crear(cuenta);
            return RedirectToAction("Index");

        }

        public async Task<IEnumerable<SelectListItem>>ObtenerTipoCuenta(int usuarioId)
        {
            var tiposCuenta = await repositorioTiposCuentas.Obtener(usuarioId);
            return tiposCuenta.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }


        public async Task<IActionResult> Index()
        {
            var usuarioId = servicioUsuario.obtenerUsuarioId();
            var cuentaporTipoCuenta = await repositorioCuenta.Buscar(usuarioId);

            var modelo = cuentaporTipoCuenta
                .GroupBy(x => x.TipoCuenta)
                .Select(grupo => new IndiceCuentaViewModel
            {
                  TipoCuenta = grupo.Key,
                  cuentas = grupo.AsEnumerable()

            }).ToList();

            return View(modelo);
        }
        public async Task<IActionResult> Editar(int id)
        {
            var usuarioId = servicioUsuario.obtenerUsuarioId();
            var cuenta = await repositorioCuenta.ObtenerPorId(id,usuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            //var modelo = mapper.Map<CuentaCreacionViewModel>(cuenta);
            var modelo = mapper.Map<CuentaCreacionViewModel>(cuenta);
            modelo.tipocuenta = await ObtenerTipoCuenta(usuarioId);
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(CuentaCreacionViewModel obtenerid) 
        {
            var usuarioId = servicioUsuario.obtenerUsuarioId();
            var validarCuenta = await repositorioCuenta.ObtenerPorId(obtenerid.Id, usuarioId);
            if (validarCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var ValidarTipoCuenta = await repositorioTiposCuentas.ObtenerPorId(obtenerid.TipoCuentaId, usuarioId);
            if(validarCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioCuenta.Actualizar(obtenerid);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Borrar(int id)
        {
            var UsuarioId = servicioUsuario.obtenerUsuarioId();
            var Cuenta = await repositorioCuenta.ObtenerPorId(id, UsuarioId);
            if(Cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            return View(Cuenta);
        }

        [HttpPost]
        public async Task<IActionResult> BorrarCuenta(int id)
        {
            var UsuarioId = servicioUsuario.obtenerUsuarioId();
            var Cuenta = await repositorioCuenta.ObtenerPorId(id, UsuarioId);
            if (Cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            await repositorioCuenta.Borrar(id);
            return RedirectToAction("Index");
        }


        //public async Task<IActionResult> Borrar(int id)
        //{
        //    var usuarioId = servicioUsuario.obtenerUsuarioId();
        //    var cuenta = await repositorioCuenta.ObtenerPorId(usuarioId, id);
        //    if(cuenta is null)
        //    {
        //        return RedirectToAction("NoEncontrado", "Home");
        //    }

        //    return View(cuenta);
        //}

        
        public async Task<IActionResult>Detalle(int id,int mes,int año)
        {
            var usuarioId = servicioUsuario.obtenerUsuarioId();
            var cuenta = await repositorioCuenta.ObtenerPorId(id, usuarioId);
            if(cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            ViewBag.cuenta = cuenta.Nombre;
            var modelo = await servicioReportes.obtenerReporteTransaccionesDetalladasPorCuenta(usuarioId, id, mes, año,ViewBag);
            return View(modelo);
        }




        //public async Task<IActionResult> Detalle (int id, int mes, int año)
        //{
        //    var usuarioId = servicioUsuario.obtenerUsuarioId();
        //    var cuentas = await repositorioCuenta.ObtenerPorId(id, usuarioId);
        //    if(cuentas is null)
        //    {
        //        return RedirectToAction("NoEncontrado", "Home");
        //    }

        //    DateTime fechaInicio;
        //    DateTime fechaFin;
        //    if(mes <= 0 || mes > 12 || año <= 1900)
        //    {
        //        var hoy = DateTime.Today;
        //        fechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
        //    }
        //    else
        //    {
        //        fechaInicio = new DateTime(año, mes, 1);
        //    }

        //    fechaFin = fechaInicio.AddMonths(1).AddDays(-1);
        //    var cuentaPorTransaccion = new ObtenerTransaccionPorCuenta()
        //    {
        //        cuentasId = id,
        //        usuarioId = usuarioId,
        //        fechaInicio = fechaInicio,
        //        fechaFin = fechaFin
        //    };

        //    var transacciones = await repositorioTransaccion.obtenerPorCuentaId(cuentaPorTransaccion);
        //    var modelo = new ReporteTransaccionesDetalladas();
        //    ViewBag.cuenta = cuentas.nombre;
        //    var transaccionesPorFecha = transacciones.OrderByDescending(x => x.fechatransaccion)
        //        .GroupBy(x => x.fechatransaccion)
        //        .Select(grupo => new ReporteTransaccionesDetalladas.TransaccionesPorFecha()
        //        {
        //            FechaTransaccion = grupo.Key,
        //            transacciones = grupo.AsEnumerable()
        //        });

        //    modelo.TransaccionesAgrupadas = transaccionesPorFecha;
        //    modelo.FechaInicio = fechaInicio;
        //    modelo.FechaFin = fechaFin;
        //    ViewBag.mesAnterior = fechaInicio.AddMonths(-1).Month;
        //    ViewBag.añoAnterior = fechaInicio.AddMonths(-1).Year;
        //    ViewBag.mesPosterior = fechaInicio.AddMonths(1).Month;
        //    ViewBag.añoPosterior = fechaInicio.AddMonths(1).Year;

        //    ViewBag.retornourl = HttpContext.Request.Path + HttpContext.Request.QueryString;

        //    return View(modelo);
        //}
    }

    
}
