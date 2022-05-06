create procedure Transacciones_Borrar
@id int
as
begin
	
	declare @monto decimal(18,2);
	declare @cuentaId int;
	declare @tipoOperacionId int;

	select @monto = Monto,@cuentaId=CuentaId,@tipoOperacionId=cat.TipoOperacionId from Transacciones 
	inner join Categorias cat on cat.Id = Transacciones.CategoriaId where Transacciones.Id = @id

	declare @factorMultiplicativo int = -1;

	if (@factorMultiplicativo = 2)
		
		set @factorMultiplicativo = -1;
	set @monto = @monto * @factorMultiplicativo;

	update Cuentas set Balance -= @monto where Id = @cuentaId;

	delete Transacciones where Id = @id; 
end