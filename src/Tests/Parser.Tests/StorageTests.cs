using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parser.BL.Entities;

namespace Parser.Tests
{
    [TestClass]
    public class StorageTests : BaseTest
    {
        [TestMethod]
        public async Task AddDeleteOrganizationTest()
        {
            var organizations = await Storage.Organizations.ToListAsync();
            Storage.Organizations.RemoveRange(organizations);
            await Storage.SaveChangesAsync();

            var organizationId = Guid.NewGuid();
            var contactId = Guid.NewGuid();
            var creatingOrganization = new OrganizationEntity
            {
                Id = organizationId,
                Name = "Test org",
                Contacts = new List<ContactEntity>
                {
                    new ContactEntity
                    {
                        Id = contactId,
                        Name = "test contact"
                    }
                }
            };

            Storage.Organizations.Add(creatingOrganization);
            await Storage.SaveChangesAsync();

            var createdOrganization = await Storage.Organizations
                .Where(e => e.Id == organizationId)
                .Include(e => e.Contacts)
                .FirstAsync();

            Assert.IsNotNull(createdOrganization);
            Assert.IsTrue(createdOrganization.Name == creatingOrganization.Name);
            Assert.IsTrue(createdOrganization.Contacts.Count == 1);
            Assert.IsTrue(createdOrganization.Contacts.First().Name == creatingOrganization.Contacts.First().Name);

            Storage.Contacts.RemoveRange(createdOrganization.Contacts);
            Storage.Organizations.Remove(createdOrganization);
            await Storage.SaveChangesAsync();
        }
    }
}
