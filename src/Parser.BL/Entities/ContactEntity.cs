using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.BL.Entities
{
    public class ContactEntity : IEntity
    {
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public string Url { get; set; }

        public Guid OrganizationId { get; set; }
        [ForeignKey("OrganizationId")]
        public OrganizationEntity Organization { get; set; }

        public Guid Id { get; set; }
        public DateTime CreatedDateUtc { get; set; }
        public DateTime LastUpdatedDateUtc { get; set; }
    }
}
