namespace EventosPeruIA.Domain.Core.Entities;
public partial class Categorias
{
 public int CategoriaId {get;set;}
 public string Nombre {get;set;} = null!;
 public string? Descripcion {get;set;}
 public bool Activo {get;set;}
}
