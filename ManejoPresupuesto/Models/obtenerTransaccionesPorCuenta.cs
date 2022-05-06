namespace ManejoPresupuesto.Models
{
    public class obtenerTransaccionesPorCuenta
    {
        public int usuarioId { get; set; }
        public int CuentaId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }
}
