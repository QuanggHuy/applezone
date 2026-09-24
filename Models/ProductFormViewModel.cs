using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppleZone.Models
{
    public class ProductFormViewModel
    {
        public Product Product { get; set; } = new Product();

        public List<SelectListItem> CategoryList { get; set; } = new List<SelectListItem>();

        [ValidateNever]
        public IFormFile? ImageFile { get; set; }
    }
}
