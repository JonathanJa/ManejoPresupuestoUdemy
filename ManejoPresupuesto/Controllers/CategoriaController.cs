using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ManejoPresupuesto.Controllers
{
    public class CategoriaController:Controller
    {
        private readonly IRepositorioCategoria repositorioCategoria;
        private readonly IServicioUsuario serviciosUsuarios;

        public CategoriaController(IRepositorioCategoria repositorioCategoria,IServicioUsuario serviciosUsuarios)
        {
            this.repositorioCategoria = repositorioCategoria;
            this.serviciosUsuarios = serviciosUsuarios;
        }

        [HttpGet]

        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Categoria categoria)
        {
            if (!ModelState.IsValid)
            {
                return View(categoria);
            }
            var usuarioId = serviciosUsuarios.obtenerUsuarioId();
            categoria.UsuarioId= usuarioId;
            await repositorioCategoria.Crear(categoria);
            return RedirectToAction("Index");
        }

      
        public async Task<IActionResult> Index(PaginacionViewModel paginacion)
        {
            var usuarioId = serviciosUsuarios.obtenerUsuarioId();
            var categoria = await repositorioCategoria.Buscar(usuarioId, paginacion);
            var TotalCategorias = await repositorioCategoria.Contar(usuarioId);
            var RespuestaVM = new PaginacionRespuesta<Categoria>
            {
                Elementos = categoria,
                Pagina = paginacion.Pagina,
                RecordsPorPaginas = paginacion.RecordsPorPagina,
                CantidadTotalDeRecords = TotalCategorias,
                BaseURL = Url.Action()

            };
            return View(RespuestaVM);
        }

        public async Task<IActionResult> Editar(int id)
        {
            var usuarioId = serviciosUsuarios.obtenerUsuarioId();
            var buscarCategoria = await repositorioCategoria.ObtenerId(id,usuarioId);
            if(buscarCategoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(buscarCategoria);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Categoria categoriaEditar)
        {
            if (!ModelState.IsValid)
            {
                return View(categoriaEditar);
            }
            var usuarioId = serviciosUsuarios.obtenerUsuarioId();
            var buscarCategoria = await repositorioCategoria.ObtenerId(categoriaEditar.Id, usuarioId);
            if (buscarCategoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            categoriaEditar.UsuarioId = usuarioId;
            await repositorioCategoria.actualizar(categoriaEditar);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Borrar(int id)
        {
            var usuarioId = serviciosUsuarios.obtenerUsuarioId();
            var buscarCategoria = await repositorioCategoria.ObtenerId(id, usuarioId);
            if (buscarCategoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(buscarCategoria);
        }

        [HttpPost]
        public async Task<IActionResult> BorrarCategoria(int id)
        {
            var usuarioId = serviciosUsuarios.obtenerUsuarioId();
            var buscarCategoria = await repositorioCategoria.ObtenerId(id, usuarioId);
            if (buscarCategoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            await repositorioCategoria.Borrar(id);
            return RedirectToAction("Index");
        }
    }
}
