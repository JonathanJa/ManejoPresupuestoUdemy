using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ManejoPresupuesto.Controllers
{
    public class TiposCuentasController:Controller
    {
        private readonly IRepositorioTiposCuentas repositorioTiposCuentas;
        private readonly IServicioUsuario servicioUsuario;

        public TiposCuentasController(IRepositorioTiposCuentas repositorioTiposCuentas,IServicioUsuario servicioUsuario)
        {
            this.repositorioTiposCuentas = repositorioTiposCuentas;
            this.servicioUsuario = servicioUsuario;
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = servicioUsuario.obtenerUsuarioId();
            var obtener = await repositorioTiposCuentas.Obtener(usuarioId);
            return View(obtener);
        }

        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> crear(TiposCuentas tiposCuentas)
        {
            if (!ModelState.IsValid) //(validacion) El modelState lo que hace es prevenir que los datos fueran enviados a la bd 
            {
                return View(tiposCuentas);
            }
            tiposCuentas.UsuarioId = servicioUsuario.obtenerUsuarioId();
            var ExisteUsuario = await repositorioTiposCuentas.Existe(tiposCuentas.Nombre, tiposCuentas.UsuarioId);//verificamos para no repetir los datos guardados
            if (ExisteUsuario)
            {
                ModelState.AddModelError(nameof(tiposCuentas.Nombre), $"El nombre {tiposCuentas.Nombre} ya existe");//creamos un error de estado de modelo para mostrarlo por pantalla
                return View(tiposCuentas);//si existe el usuario lo volvemos a mostrar por pantalla
            }

            await repositorioTiposCuentas.Crear(tiposCuentas);//y si no existe el usuario lo cargamos en la bd

            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> VerificarExisteTipoCuenta(string nombre)
        {
            var usuarioId = servicioUsuario.obtenerUsuarioId();
            var existeCuenta = await repositorioTiposCuentas.Existe(nombre, usuarioId);

            if (existeCuenta)
            {
                return Json($"El nombre {nombre} ya existe");
            }

            return Json(true);
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var usuarioId = servicioUsuario.obtenerUsuarioId();
            var TipoCuenta = await repositorioTiposCuentas.ObtenerPorId(id, usuarioId);
            if(TipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(TipoCuenta);
        }
        [HttpPost]
        public async Task<IActionResult> Editar(TiposCuentas tiposCuentas)
        {
            var usuarioId = servicioUsuario.obtenerUsuarioId();
            var TipoCuenta = await repositorioTiposCuentas.ObtenerPorId(tiposCuentas.Id, usuarioId);
            if(TipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioTiposCuentas.Actualizar(tiposCuentas);
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Borrar(int id)
        {
            var usuarioId = servicioUsuario.obtenerUsuarioId();
            var TipoCuenta = await repositorioTiposCuentas.ObtenerPorId(id, usuarioId);
            if(TipoCuenta is null)
            {
                return RedirectToAction("NoEncontrrado", "Home");
            }
            return View(TipoCuenta);
        }

        [HttpPost]
        public async Task<IActionResult> BorrarTipoCuenta(int id)
        {
            var usuarioId = servicioUsuario.obtenerUsuarioId();
            var TipoCuenta = await repositorioTiposCuentas.ObtenerPorId(id, usuarioId);
            if (TipoCuenta is null)
            {
                return RedirectToAction("NoEncontrrado", "Home");
            }

            await repositorioTiposCuentas.Borrar(id);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Ordenar([FromBody] int[] ids)
        {
            var usuarioId = servicioUsuario.obtenerUsuarioId();
            var TiposCuentasIds = await repositorioTiposCuentas.Obtener(usuarioId);
            var modeloLinq = TiposCuentasIds.Select(x => x.Id);
            var EstadoCuentaIds = ids.Except(modeloLinq).ToList();

            if(EstadoCuentaIds.Count > 0)
            {
                return Forbid();
            }

            var tiposCuentasOrdenados = ids
                                        .Select((valor,orden) => new TiposCuentas() { Id = valor, Orden = orden + 1 })
                                        .AsEnumerable();

            await repositorioTiposCuentas.Ordenar(tiposCuentasOrdenados);

            return Ok();
        }
    }
}
