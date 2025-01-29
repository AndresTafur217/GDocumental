using System.ComponentModel.DataAnnotations;

namespace Gestion_documental.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }

        public virtual ICollection<Documento> Documentos { get; set; }
    }
}
