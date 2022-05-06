namespace ManejoPresupuesto.Models
{
    public class IndiceCuentaViewModel
    {
        public string TipoCuenta{ get; set; }
        public IEnumerable<Cuenta> cuentas { get; set; }
        public decimal balance => cuentas.Sum(x => x.Balance);
    }
}
