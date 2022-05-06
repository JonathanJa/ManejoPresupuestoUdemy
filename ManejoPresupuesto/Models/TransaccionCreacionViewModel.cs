using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
    public class TransaccionCreacionViewModel:Transaccion
    {
        public IEnumerable<SelectListItem> Categoria { get; set; }
        public IEnumerable<SelectListItem> Cuenta { get; set; }
      

    }
}
