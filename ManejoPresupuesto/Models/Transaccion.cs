using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
    public class Transaccion
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        [Display(Name = "Fecha Transacción")]
        [DataType(DataType.Date)]
        public DateTime FechaTransaccion { get; set; } = DateTime.Now;

        public decimal Monto { get; set; }
        [StringLength(maximumLength:1000,ErrorMessage ="No debe pasar a mas de {1} caracteres")]
        public string Nota { get; set; }
        [Range(1, maximum: int.MaxValue, ErrorMessage = "Debe seleccionar una cuenta")]
        [Display(Name ="Cuenta")]
        public int CuentaId { get; set; }
        [Range(1,maximum:int.MaxValue,ErrorMessage ="Debe seleccionar una categoria" )]
        [Display(Name ="Categoria")]
        public int CategoriaId { get; set; }
        [Display(Name = "Operación")]
        public TipoOperacion TipoOperacionId { get; set; } = TipoOperacion.Ingresos;
        
        public string categoria { get; set; }
        public string cuenta { get; set; }
    }
}
