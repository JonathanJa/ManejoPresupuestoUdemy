using AutoMapper;
using ClosedXML.Excel;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace ManejoPresupuesto.Controllers
{
    public class TransaccionController:Controller
    {
        private readonly IRepositorioTransacciones repositorioTransacciones;
        private readonly IRepositorioCuenta repositorioCuenta;
        private readonly IServicioUsuario servicioUsuario;
        private readonly IRepositorioCategoria repositorioCategoria;
        private readonly IMapper mapper;
        private readonly IServicioReportes servicioReportes;

        public TransaccionController(IRepositorioTransacciones repositorioTransacciones,
            IRepositorioCuenta repositorioCuenta,IServicioUsuario servicioUsuario,
            IRepositorioCategoria repositorioCategoria,IMapper mapper,IServicioReportes servicioReportes)
        {
            this.repositorioTransacciones = repositorioTransacciones;
            this.repositorioCuenta = repositorioCuenta;
            this.servicioUsuario = servicioUsuario;
            this.repositorioCategoria = repositorioCategoria;
            this.mapper = mapper;
            this.servicioReportes = servicioReportes;
        }

        public async Task<IActionResult> Index(int mes,int año)
        {
            var usuarioId = servicioUsuario.obtenerUsuarioId();
            var modelo = await servicioReportes.obtenerReportesTransaccionesDetalladas(usuarioId, mes, año, ViewBag);
         
            return View(modelo);
        }

       
        public async Task<IActionResult> Semanal(int mes,int año)
        {
            var usuarioId = servicioUsuario.obtenerUsuarioId();
            IEnumerable<ResultadoObtenerPorSemana>transaccionesPorsemana = await servicioReportes.obtenerPorSemana(usuarioId, mes, año, ViewBag);
            var agrupado = transaccionesPorsemana.GroupBy(x => x.Semana).Select(x => new ResultadoObtenerPorSemana
            {
                Semana = x.Key,
                Ingresos = x.Where(x => x.TipooperacionId == TipoOperacion.Ingresos).Select(x=>x.Monto).FirstOrDefault(),
                Gastos = x.Where(x=>x.TipooperacionId == TipoOperacion.Gastos).Select(x=>x.Monto).FirstOrDefault()

            }).ToList();

            if(año == 0 || mes == 0)
            {
                var hoy = DateTime.Today;
                año = hoy.Year;
                mes = hoy.Month;

            }

            var fechaReferencia = new DateTime(año, mes, 1);
            var diaDelMes = Enumerable.Range(1, fechaReferencia.AddMonths(1).AddDays(-1).Day);

            var DiasSegmentados = diaDelMes.Chunk(7).ToList();
            for(var i=0;i<DiasSegmentados.Count(); i++)
            {
                var semana = i + 1;
                var fechaInicio = new DateTime(año, mes, DiasSegmentados[i].First());
                var fechaFin = new DateTime(año, mes, DiasSegmentados[i].Last());
                var grupoSemana = agrupado.FirstOrDefault(x => x.Semana == semana);
                if(grupoSemana is null)
                {
                    agrupado.Add(new ResultadoObtenerPorSemana()
                    {
                        Semana = semana,
                        FechaInicio = fechaInicio,
                        FechaFin = fechaFin
                    });
                }
                else
                {
                    
                    grupoSemana.FechaInicio= fechaInicio;
                    grupoSemana.FechaFin= fechaFin;
                }
            }

            agrupado = agrupado.OrderByDescending(x => x.Semana).ToList();
            var modelo = new ReporteSemanalViewModel();
            modelo.TransaccionesPorSemana = agrupado;
            modelo.FechaReferencia = fechaReferencia;

            return View(modelo);
        }
        public async Task<IActionResult> Mensual(int año)
        {
            var usuarioId = servicioUsuario.obtenerUsuarioId();
            if(año == 0)
            {
                año = DateTime.Today.Year;
            }

            var transaccionesPorMes = await repositorioTransacciones.ObtenerPorMes(usuarioId, año);
            var transaccionesAgrupadas = transaccionesPorMes.GroupBy(x=>x.Mes).Select(x=> new ResultadoObtenerPorMes()
            {
                Mes = x.Key,
                Ingresos = x.Where(x=>x.TipoOperacionId == TipoOperacion.Ingresos).Select(x => x.Monto).FirstOrDefault(),
                Gastos = x.Where(x => x.TipoOperacionId == TipoOperacion.Gastos).Select(x => x.Monto).FirstOrDefault()
            }).ToList();
            for (int mes = 1; mes <= 12; mes++)
            {
                var Transaccion = transaccionesAgrupadas.FirstOrDefault(x => x.Mes == mes);
                var fechaReferencia = new DateTime(año, mes, 1);
                if(Transaccion is null)
                {
                    transaccionesAgrupadas.Add(new ResultadoObtenerPorMes()
                    {
                        Mes = mes,
                        FechaReferencia = fechaReferencia,
                    });
                }
                else
                {
                    Transaccion.FechaReferencia = fechaReferencia;
                }
            }

            transaccionesAgrupadas = transaccionesAgrupadas.OrderByDescending(x => x.Mes).ToList();
            var modelo = new ReporteMensualViewModel();
            modelo.Año = año;
            modelo.TransaccionesPorMes = transaccionesAgrupadas;

            return View(modelo);
        }
        public IActionResult ReporteExcel()
        {
            return View();
        }
        public async Task<FileResult>ExportarExcelPorMes(int mes,int año)
        {
            var FechaInicio = new DateTime(año, mes, 1);
            var FechaFin = FechaInicio.AddMonths(1).AddDays(-1);
            var usuarioId = servicioUsuario.obtenerUsuarioId();

            var transacciones = await repositorioTransacciones.ObtenerPorUsuario(new ParametroObtenerTransaccionesPorUsuario
            {
                UsuarioId = usuarioId,
                FechaInicio = FechaInicio,
                FechaFin = FechaFin
            });

            var nombreArchivo = $"Manejo Presupuesto - {FechaInicio.ToString("MMM yyyy")}.xlsx";
            return GenerarExcel(nombreArchivo, transacciones);
        }

        [HttpGet]
        public async Task<FileResult>ExportarExcelPorAño(int año)
        {
            var FechaInicio = new DateTime(año, 1, 1);
            var FechaFin = FechaInicio.AddYears(1).AddDays(-1);
            var usuarioId = servicioUsuario.obtenerUsuarioId();

            var transacciones = await repositorioTransacciones.ObtenerPorUsuario(new ParametroObtenerTransaccionesPorUsuario
            {
                UsuarioId = usuarioId,
                FechaInicio = FechaInicio,
                FechaFin = FechaFin
            });

            var nombreArchivo = $"Manejo Presupuesto - {FechaInicio.ToString("yyyy")}.xlsx";
            return GenerarExcel(nombreArchivo, transacciones);
        }
        [HttpGet]
        public async Task<FileResult> ExportarExcelTodo()
        {
            var FechaInicio = DateTime.Today.AddYears(-100);
            var FechaFin = DateTime.Today.AddYears(1000);
            var usuarioId = servicioUsuario.obtenerUsuarioId();

            var transacciones = await repositorioTransacciones.ObtenerPorUsuario(new ParametroObtenerTransaccionesPorUsuario
            {
                UsuarioId = usuarioId,
                FechaInicio = FechaInicio,
                FechaFin = FechaFin
            });

            var nombreArchivo = $"Manejo Presupuesto - {FechaInicio.ToString("dd-MM-yyyy")}.xlsx";
            return GenerarExcel(nombreArchivo, transacciones);
        }
        private FileResult GenerarExcel(string nombreArchivo,IEnumerable<Transaccion> transacciones)
        {
            DataTable dataTable = new DataTable("Transacciones");
            dataTable.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("Fecha"),
                new DataColumn("Cuenta"),
                new DataColumn("Categoria"),
                new DataColumn("Nota"),
                new DataColumn("Monto"),
                new DataColumn("Ingreso/Gasto")
            }) ;

            foreach(var transaccion in transacciones)
            {
                dataTable.Rows.Add(transaccion.FechaTransaccion,
                                   transaccion.cuenta,
                                   transaccion.categoria,
                                   transaccion.Nota,
                                   transaccion.Monto,
                                   transaccion.TipoOperacionId);
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dataTable);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreArchivo);
                }
            }
        }

        public IActionResult Calendario()
        {
            return View();
        }

        public async Task<JsonResult> obtenerTransaccionesPorCalendario(DateTime Start,DateTime End)
        {
            var usuarioId = servicioUsuario.obtenerUsuarioId();
            var transacciones = await repositorioTransacciones.ObtenerPorUsuario(new ParametroObtenerTransaccionesPorUsuario
            {
                UsuarioId = usuarioId,
                FechaInicio = Start,
                FechaFin = End
            });

            var eventoCalendario = transacciones.Select(transaccion => new EventoCalendario()
            {
                Title = transaccion.Monto.ToString("N"),
                Start = transaccion.FechaTransaccion.ToString("yyyy-MM-dd"),
                End = transaccion.FechaTransaccion.ToString("yyyy-MM-dd"),
                Color = (transaccion.TipoOperacionId == TipoOperacion.Gastos)? "Red" : null,
            });

            return Json(eventoCalendario);

        }

        public async Task<JsonResult>MostrarTransaccionesPorFecha(DateTime fecha)
        {
            var usuarioId = servicioUsuario.obtenerUsuarioId();
            var transacciones = await repositorioTransacciones.ObtenerPorUsuario(new ParametroObtenerTransaccionesPorUsuario
            {
                UsuarioId = usuarioId,
                FechaInicio = fecha,
                FechaFin = fecha
            });

            return Json(transacciones);
        }
        public async Task<IActionResult> Crear()
        {
            var usuarioId = servicioUsuario.obtenerUsuarioId();
            var modelo = new TransaccionCreacionViewModel();
            modelo.Cuenta = await ObtenerCuenta(usuarioId);
            modelo.Categoria = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(TransaccionCreacionViewModel modelo)
        {
            var usuarioId = servicioUsuario.obtenerUsuarioId();
            if (!ModelState.IsValid)
            {
                modelo.Cuenta = await ObtenerCuenta(usuarioId);
                modelo.Categoria = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
                return View(modelo);
            }

            var cuenta = await repositorioCuenta.ObtenerPorId(modelo.CuentaId, usuarioId);
            if(cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            var categoria = await repositorioCategoria.ObtenerId(modelo.CategoriaId, usuarioId);
            if(categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            modelo.UsuarioId = usuarioId;

            if(modelo.TipoOperacionId == TipoOperacion.Gastos)
            {
                modelo.Monto *= -1;
            }

            await repositorioTransacciones.Crear(modelo);

            return RedirectToAction("Index");
        }
       

       [HttpGet]
       public async Task<IActionResult> Editar(int id, string UrlRetorno)
        {
            var usuarioId = servicioUsuario.obtenerUsuarioId();
            var transacciones = await repositorioTransacciones.ObtenerPorId(id, usuarioId);
            if(transacciones is null)
            {
                return RedirectToAction("NoEncontrado", "Home");

            }
            var modelo = mapper.Map<TransaccionActualizarViewModel>(transacciones);
            modelo.MontoAnterior = modelo.Monto;

            if(modelo.TipoOperacionId == TipoOperacion.Gastos)
            {
                modelo.MontoAnterior = modelo.Monto * -1;
            }
            modelo.CuentaAnteriorId = transacciones.CuentaId;


            modelo.Cuenta = await ObtenerCuenta(usuarioId);
            modelo.Categoria = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
            modelo.UrlRetorno = UrlRetorno;
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult>Editar(TransaccionActualizarViewModel modelo)
        {
            var usuarioId = servicioUsuario.obtenerUsuarioId();
            if (!ModelState.IsValid)
            {
                modelo.Cuenta = await ObtenerCuenta(usuarioId);
                modelo.Categoria = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
                return View(modelo);
            }
            
            var cuenta = await repositorioCuenta.ObtenerPorId(modelo.CuentaId, usuarioId);
            if(cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            var categorias = await repositorioCategoria.ObtenerId(modelo.CategoriaId, usuarioId);
            if(categorias is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            var mod = mapper.Map<Transaccion>(modelo);
            if(mod.TipoOperacionId == TipoOperacion.Gastos)
            {
                mod.Monto *= -1;
            }

            await repositorioTransacciones.Actualizar(mod,modelo.MontoAnterior, modelo.CuentaAnteriorId);
            if (String.IsNullOrEmpty(modelo.UrlRetorno))
            {
                return RedirectToAction("Index");
            }
            else
            {
                return LocalRedirect(modelo.UrlRetorno);
            }
            
        }
        [HttpPost]
        public async Task<ActionResult> Borrar(int id, string UrlRetorno)
        {
            var usuarioId = servicioUsuario.obtenerUsuarioId();
            var transacciones = await repositorioTransacciones.ObtenerPorId(id, usuarioId);
            if(transacciones is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            await repositorioTransacciones.Borrar(id);
            if (String.IsNullOrEmpty(UrlRetorno))
            {
                return RedirectToAction("Index");
            }
            else
            {
                return LocalRedirect(UrlRetorno);
            }
           
        }



        private async Task<IEnumerable<SelectListItem>> ObtenerCuenta(int usuarioId)
        {
            var cuenta = await repositorioCuenta.Buscar(usuarioId);
            var modelo = cuenta.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
            return modelo;
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCategorias(int usuarioId, TipoOperacion tipoOperacion)
        {
            var categorias = await repositorioCategoria.Buscar(usuarioId, tipoOperacion);
            var resultado = categorias.Select(x => new SelectListItem(x.Nombre, x.Id.ToString())).ToList();
            var opcionPorDefecto = new SelectListItem("--- Selecciones una Categoria ---","0", true);
            resultado.Insert(0, opcionPorDefecto);
            return resultado;
        }
        [HttpPost]
        public async Task<IActionResult> ObtenerCategorias([FromBody] TipoOperacion tipoOperacion)
        {
            var usuarioId = servicioUsuario.obtenerUsuarioId();
            var categorias = await ObtenerCategorias(usuarioId, tipoOperacion);
            return Ok(categorias);
        }
    }
}
