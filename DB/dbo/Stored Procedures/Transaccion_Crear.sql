create procedure Transaccion_Crear
@UsuarioId int,
@fechaTransaccion datetime,
@monto decimal(18,2),
@Nota nvarchar(1000),
@cuentaId int,
@categoriaId int
as
begin
	insert into Transacciones(UsuarioId,FechaTransaccion,Monto,Nota,CuentaId,CategoriaId)
	values(@UsuarioId,@fechaTransaccion,abs(@monto),@Nota,@cuentaId,@categoriaId);
	
	update Cuentas set Balance += @monto where Id = @cuentaId;

	select SCOPE_IDENTITY();
end