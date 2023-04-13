using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.BL.Entities
{
    [Table("Organizations")]
    public class OrganizationEntity : IEntity
    {
        public string Name { get; set; }
        public string SubLink { get; set; }
        public decimal Rating { get; set; }
        public int EmployeesNumber { get; set; }
        public int ReviewNumber { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string ContactInfoNotParsed { get; set; }
        public int HowManyTimesWasViewedByPeople { get; set; }
        public ICollection<ContactEntity> Contacts { get; set; } = new List<ContactEntity>();
        public string Description { get; set; }

        public Guid Id { get; set; }
        public DateTime CreatedDateUtc { get; set; }
        public DateTime LastUpdatedDateUtc { get; set; }
    }
}
