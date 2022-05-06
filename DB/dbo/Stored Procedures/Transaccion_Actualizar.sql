 create procedure Transaccion_Actualizar
  @Id int,
  @fechaTransaccion datetime,
  @monto decimal(18,2),
  @montoAnterior decimal(18,2),
  @cuentaId int,
  @cuentaIdAnterior int,
  @CategoriaId int,
  @nota nvarchar(1000) = null
  as
  begin
  -- transacion anterior--
  update Cuentas set Balance -=@montoAnterior where Id = @cuentaIdAnterior;
  --transaccion nueva
  update Cuentas set Balance += @monto where Id = @cuentaId;

  update Transacciones set Monto = abs(@monto),FechaTransaccion = @fechaTransaccion, CuentaId=@cuentaId, CategoriaId=@CategoriaId, Nota=@nota where Id = @Id; 
  end