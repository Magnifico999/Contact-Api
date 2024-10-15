using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MainAssignment.Model;
using MainAssignment.Data;
using Microsoft.EntityFrameworkCore;

namespace MainAssignment.Controllers
{

    [ApiController]
    [Route("api[controller]")]

    public class ContactController : Controller
    {
        private readonly ContactDbContext dbContext;
        private readonly Cloudinary cloudinary;

        public ContactController(ContactDbContext dbContext, Cloudinary cloudinary)
        {
            this.dbContext = dbContext;
            this.cloudinary = cloudinary;
        }

        [AllowAnonymous]
        //GET ALL CONTACTS
        [HttpGet]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> GetAllContacts()
        {
            return Ok(await dbContext.Contact.ToListAsync());

        }

        //POST 
        [AllowAnonymous]
        [HttpPost("Add contact")]
        [Authorize(Policy = "RequireRegularRole")]

        public async Task<IActionResult> AddContact(AddContactRequest addContactRequest)
        {
            var contact = new Contact()
            {
                id = Guid.NewGuid(),
                FullName = addContactRequest.FullName,
                Email = addContactRequest.Email,
                PhoneNumber = addContactRequest.PhoneNumber,
                Address = addContactRequest.Address,
            };

            await dbContext.Contact.AddAsync(contact);
            await dbContext.SaveChangesAsync();
            return Ok(contact);

        }
        [AllowAnonymous]
        [HttpPatch("UploadPhoto")]
        [Authorize(Policy = "RequireRegularRole")]
        public async Task<string> UpdateUserPhotosAsync(string userId, IFormFile[] images)
        {

            if (images == null || images.Length == 0)
            {
                 return "no image";
            }
             var user = dbContext.UserTable.FirstOrDefault(c => c.Id == userId);

             if (user == null)
             {
                 return "Couldnt find user";
             }
             await cloudinary.DeleteResourcesAsync(user.Avatar);

             string avatar = "";
             foreach (var image in images)
             {
                 var result = await cloudinary.UploadAsync(new ImageUploadParams
                 {
                     File = new FileDescription(image.FileName, image.OpenReadStream())
                 }).ConfigureAwait(false);
                 avatar += result.Url;
             }
             if (avatar.Length == 0) return "Failed to upload";

             user.Avatar = avatar;
             dbContext.UserTable.Update(user);
             await dbContext.SaveChangesAsync();
             return "Done"; 
        }


        [HttpGet]
        [Route("{id:guid}")]
        [Authorize(Roles = "Admin, Regular")]
        public async Task<IActionResult> GetContact([FromRoute] Guid id)
        {
            var contact = await dbContext.Contact.FindAsync(id);

            if (contact == null)
            {
                return NotFound();
            }
            return Ok(contact);
        }
        [HttpPut]
        [Route("{id:guid}")]
        [Authorize(Policy = "RequireRegularRole")]
        public async Task<IActionResult> UpdateContact([FromRoute] Guid id, UpdateContactRequest updateContactRequest)
        {
            var contact = await dbContext.Contact.FindAsync(id);

            if (contact != null)
            {
                contact.FullName = updateContactRequest.FullName;
                contact.PhoneNumber = updateContactRequest.PhoneNumber;
                contact.Address = updateContactRequest.Address;
                contact.Email = updateContactRequest.Email;

                await dbContext.SaveChangesAsync();

                return Ok(contact);
            }

            return NotFound();
        }
        [HttpDelete]
        [Route("{id:guid}")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> DeleteContact([FromRoute] Guid id)
        {
            var contact = await dbContext.Contact.FindAsync(id);

            if (contact != null)
            {
                dbContext.Remove(contact);
                await dbContext.SaveChangesAsync();
                return Ok(contact);
            }

            return NotFound();
        }
        [HttpGet("{page}")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> GetContactPage(int page)
        {
            var pageResults = 3f;
            var pageCount = Math.Ceiling(dbContext.Contact.Count() / pageResults);

            var contact = await dbContext.Contact
                .Skip((page - 1) * (int)pageResults)
                .Take((int)pageResults)
                .ToListAsync();

            var response = new ContactResponse
            {
                Contacts = contact,
                CurrentPage = page,
                Pages = (int)pageCount

            };

            return Ok(response);

        }

    }
}
