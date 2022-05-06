using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Validacion
{
    public class PrimeraLetraMayusculaAttribute:ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if(value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return ValidationResult.Success;
            }

            var primeraLetraMyuscula = value.ToString()[0].ToString();
            if(primeraLetraMyuscula != primeraLetraMyuscula.ToUpper())
            {
                return new ValidationResult("La primera letra debe de estar en mayuscula");
            }
            return ValidationResult.Success;
        }
    }
}
