using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MyMDb.Models.Base
{
    public class BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public DateTime? DateCreated { get; set; }

        public DateTime? DateModified { get; set; }

        public BaseEntity() 
        {
            DateCreated = DateTime.Now;
            DateModified = DateTime.Now;
        }

        // Metodă care actualizează DateModified la fiecare modificare
        public virtual void UpdateDateModified()
        {
            DateModified = DateTime.Now;
        }
    }
}
