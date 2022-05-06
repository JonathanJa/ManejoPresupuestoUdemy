 create procedure TiposCuentas_Insertar
  @nombre nvarchar(50),
  @usuarioId int
  as 
  begin
	declare @Orden int;
	select  @Orden = Coalesce(MAX(Orden),0) + 1  from TiposCuentas where UsuarioId = @usuarioId
	insert into TiposCuentas(Nombre,UsuarioId,Orden) values (@nombre,@usuarioId,@Orden)
	select SCOPE_IDENTITY();
  end