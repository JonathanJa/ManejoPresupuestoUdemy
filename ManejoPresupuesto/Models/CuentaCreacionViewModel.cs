using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManejoPresupuesto.Models
{
    public class CuentaCreacionViewModel:Cuenta
    {
        public IEnumerable<SelectListItem> tipocuenta { get; set; } //IEnumerable<SelectListItem> es una funcion predefinida de c# para hace mas facil el seleccionado de datos 
    }
}
