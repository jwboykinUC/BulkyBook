using Microsoft.AspNetCore.Mvc;

using BulkyBook.DataAccess.Data;
using BulkyBook.Models;
using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc.Rendering;
using BulkyBook.Models.ViewModels;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

        //default constructor
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnviornment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnviornment;
        }
        public IActionResult Index()
        {
             return View();

        }


       


        //GET
        public IActionResult Upsert(int? id)
        {
           ProductVM productVM = new()
           {
               Product = new(),
               CategoryList = _unitOfWork.Category.GetAll().Select(i => new SelectListItem
               {
                   Text = i.Name,
                   Value = i.Id.ToString()
               }),
               CoverTypeList = _unitOfWork.CoverType.GetAll().Select(i => new SelectListItem
               {
                   Text = i.Name,
                   Value = i.Id.ToString()
               }),

           };
            if (id == null || id == 0)
            {
                //create product
                //ViewBag.CategoryList = CategoryList;
                //ViewData["CoverTypeList"] = CoverTypeList;
                return View(productVM);
            }
            else 
            {
                //update product
                return View(productVM);

            }

            
        }


        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"images\products");
                    var extension = Path.GetExtension(file.FileName);

                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        file.CopyTo(fileStreams);
                    }
                    obj.Product.ImageUrl = @"\images\products\" + fileName + extension;
                }        
                _unitOfWork.Product.Add(obj.Product);
                _unitOfWork.Save();
                    TempData["success"] = "Product created successfully!";
                    return RedirectToAction("Index");
            }

            return View(obj);
        }


        //GET
        public IActionResult delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var coverFromDbFirst = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);
            if (coverFromDbFirst == null) return NotFound();

            return View(coverFromDbFirst);
        }


        //POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int? id)
        {
            var obj = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);
            if (id == null)
            {
                return NotFound();
            }

            _unitOfWork.Product.Remove(obj);
            _unitOfWork.Save();
            TempData["success"] = "Book " + obj.Title + " deleted successfully!";
            return RedirectToAction("Index");
        }


        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var productList = _unitOfWork.Product.GetAll(includeProperties:"Category,CoverType");
            return Json(new { data = productList });
        }
        #endregion
    }
}
