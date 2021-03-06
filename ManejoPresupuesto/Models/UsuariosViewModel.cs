using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
    public class UsuariosViewModel
    {
        [Required(ErrorMessage ="El campo {0} es requerido")]
        [EmailAddress(ErrorMessage ="Debe ser un correo electronico valido")]
        public string Email { get; set; }
        [Required( ErrorMessage = "El campo {0} es requerido")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
