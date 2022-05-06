using System.Security.Claims;

namespace ManejoPresupuesto.Servicios
{
    public interface IServicioUsuario
    {
        int obtenerUsuarioId();
    }
    public class ServicioUsuario: IServicioUsuario
    {
        private readonly HttpContext httpContext;
        public ServicioUsuario(IHttpContextAccessor httpContextAccessor)
        {
            httpContext = httpContextAccessor.HttpContext;
        }
        public int obtenerUsuarioId()
        {
            if (httpContext.User.Identity.IsAuthenticated)
            {
                var idClaim = httpContext.User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault();
                var id = int.Parse(idClaim.Value);
                return id;
            }
            else
            {
                throw new ApplicationException("El Usuario no esta Autenticado"); 
            }
           
        }
    }
}
