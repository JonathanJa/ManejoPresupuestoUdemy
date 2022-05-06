create procedure CrearDatosUsuariosNuevos
@usuarioId int
as 
begin
	declare @efectivo nvarchar(50)='Efectivo';
	declare @cuentaDeBanco nvarchar(50) = 'Cuenta de Banco';
	declare @tarjeta nvarchar(50) = 'Tarjeta';

	insert into TiposCuentas(Nombre,UsuarioId,Orden)values(@efectivo,@usuarioId,1),
	(@cuentaDeBanco,@usuarioId,2),
	(@tarjeta,@usuarioId,3);

	insert into Cuentas(Nombre,Balance,TipoCuentaId)
	select Nombre,0,Id from TiposCuentas where UsuarioId = @usuarioId;

	insert into Categorias (Nombre,TipoOperacionId,UsuarioId)values
	('Libros',2,@usuarioId),('Salario',1,@usuarioId),('Mesada',1,@usuarioId),('Comida',2,@usuarioId);

end