namespace ManejoPresupuesto.Models
{
    public class reporteTransaccionesDetalladas
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public IEnumerable<TransaccionesPorFecha> TransaccionesObtenidos { get; set; }
        public decimal BalancesDepositos => TransaccionesObtenidos.Sum(x => x.BalancesDepositos);
        public decimal BalancesRetiros => TransaccionesObtenidos.Sum(x => x.BalancesRetiros);
        public decimal Total => BalancesDepositos - BalancesRetiros;
        public class TransaccionesPorFecha 
        {

            public DateTime FechaTransaccion { get; set; }
            public IEnumerable<Transaccion> Transacciones { get; set; }

            public decimal BalancesDepositos => Transacciones.Where(x => x.TipoOperacionId == TipoOperacion.Ingresos).Sum(x => x.Monto);
            public decimal BalancesRetiros => Transacciones.Where(x => x.TipoOperacionId == TipoOperacion.Gastos).Sum(x => x.Monto);
        }


    }
}
