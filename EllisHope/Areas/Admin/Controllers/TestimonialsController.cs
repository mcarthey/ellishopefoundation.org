using EllisHope.Areas.Admin.Models;
using EllisHope.Models.Domain;
using EllisHope.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EllisHope.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "CanManageTestimonials")]
[ApiExplorerSettings(IgnoreApi = true)]
public class TestimonialsController : Controller
{
    private readonly ITestimonialService _testimonialService;
    private readonly IMediaService _mediaService;
    private readonly IResponsibilityService _responsibilityService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<TestimonialsController> _logger;

    public TestimonialsController(
        ITestimonialService testimonialService,
        IMediaService mediaService,
        IResponsibilityService responsibilityService,
        UserManager<ApplicationUser> userManager,
        ILogger<TestimonialsController> logger)
    {
        _testimonialService = testimonialService;
        _mediaService = mediaService;
        _responsibilityService = responsibilityService;
        _userManager = userManager;
        _logger = logger;
    }

    // GET: Admin/Testimonials
    [SwaggerOperation(Summary = "List all testimonials. Roles: Admin, TestimonialManager.")]
    public async Task<IActionResult> Index()
    {
        var testimonials = await _testimonialService.GetAllTestimonialsAsync();
        var pendingApproval = await _testimonialService.GetPendingApprovalAsync();

        var viewModel = new TestimonialListViewModel
        {
            Testimonials = testimonials,
            PendingApprovalCount = pendingApproval.Count()
        };

        return View(viewModel);
    }

    // GET: Admin/Testimonials/Create
    [SwaggerOperation(Summary = "Create testimonial form.")]
    public IActionResult Create()
    {
        var viewModel = new TestimonialCreateViewModel
        {
            IsPublished = false,
            IsFeatured = false,
            DisplayOrder = 0
        };

        return View(viewModel);
    }

    // POST: Admin/Testimonials/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [SwaggerOperation(Summary = "Create testimonial. Anti-forgery required.")]
    public async Task<IActionResult> Create(TestimonialCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // Check if user can auto-approve (Admin always can, or via responsibility setting)
        var canAutoApprove = User.IsInRole("Admin") ||
            await _responsibilityService.CanAutoApproveAsync(userId, Responsibility.TestimonialManager);

        var testimonial = new Testimonial
        {
            Quote = model.Quote,
            AuthorName = model.AuthorName,
            AuthorRole = model.AuthorRole,
            AuthorPhotoId = model.AuthorPhotoId,
            IsPublished = model.IsPublished,
            IsFeatured = model.IsFeatured,
            DisplayOrder = model.DisplayOrder
        };

        await _testimonialService.CreateAsync(testimonial, userId, canAutoApprove);

        if (!canAutoApprove && testimonial.RequiresApproval)
        {
            TempData["WarningMessage"] = "Testimonial created and submitted for approval.";
        }
        else
        {
            TempData["SuccessMessage"] = "Testimonial created successfully!";
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Admin/Testimonials/Edit/5
    [SwaggerOperation(Summary = "Edit testimonial form.")]
    public async Task<IActionResult> Edit(int id)
    {
        var testimonial = await _testimonialService.GetByIdAsync(id);
        if (testimonial == null)
        {
            return NotFound();
        }

        var viewModel = new TestimonialEditViewModel
        {
            Id = testimonial.Id,
            Quote = testimonial.Quote,
            AuthorName = testimonial.AuthorName,
            AuthorRole = testimonial.AuthorRole,
            AuthorPhotoId = testimonial.AuthorPhotoId,
            IsPublished = testimonial.IsPublished,
            IsFeatured = testimonial.IsFeatured,
            DisplayOrder = testimonial.DisplayOrder,
            RequiresApproval = testimonial.RequiresApproval,
            CreatedByName = testimonial.CreatedBy?.FullName ?? testimonial.CreatedBy?.UserName,
            CreatedDate = testimonial.CreatedDate,
            ApprovedByName = testimonial.ApprovedBy?.FullName ?? testimonial.ApprovedBy?.UserName,
            ApprovedDate = testimonial.ApprovedDate,
            CurrentPhoto = testimonial.AuthorPhoto
        };

        return View(viewModel);
    }

    // POST: Admin/Testimonials/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [SwaggerOperation(Summary = "Update testimonial. Anti-forgery required.")]
    public async Task<IActionResult> Edit(int id, TestimonialEditViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var testimonial = await _testimonialService.GetByIdAsync(id);
        if (testimonial == null)
        {
            return NotFound();
        }

        testimonial.Quote = model.Quote;
        testimonial.AuthorName = model.AuthorName;
        testimonial.AuthorRole = model.AuthorRole;
        testimonial.AuthorPhotoId = model.AuthorPhotoId;
        testimonial.IsPublished = model.IsPublished;
        testimonial.IsFeatured = model.IsFeatured;
        testimonial.DisplayOrder = model.DisplayOrder;

        await _testimonialService.UpdateAsync(testimonial);

        TempData["SuccessMessage"] = "Testimonial updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    // GET: Admin/Testimonials/Delete/5
    [SwaggerOperation(Summary = "Delete testimonial confirmation page.")]
    public async Task<IActionResult> Delete(int id)
    {
        var testimonial = await _testimonialService.GetByIdAsync(id);
        if (testimonial == null)
        {
            return NotFound();
        }

        return View(testimonial);
    }

    // POST: Admin/Testimonials/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [SwaggerOperation(Summary = "Delete testimonial. Anti-forgery required.")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _testimonialService.DeleteAsync(id);
        if (!result)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Testimonial deleted successfully!";
        return RedirectToAction(nameof(Index));
    }

    // POST: Admin/Testimonials/Publish/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [SwaggerOperation(Summary = "Publish testimonial. Anti-forgery required.")]
    public async Task<IActionResult> Publish(int id)
    {
        var result = await _testimonialService.PublishAsync(id);
        if (!result)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Testimonial published!";
        return RedirectToAction(nameof(Index));
    }

    // POST: Admin/Testimonials/Unpublish/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [SwaggerOperation(Summary = "Unpublish testimonial. Anti-forgery required.")]
    public async Task<IActionResult> Unpublish(int id)
    {
        var result = await _testimonialService.UnpublishAsync(id);
        if (!result)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Testimonial unpublished.";
        return RedirectToAction(nameof(Index));
    }

    // POST: Admin/Testimonials/Feature/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [SwaggerOperation(Summary = "Toggle testimonial featured status. Anti-forgery required.")]
    public async Task<IActionResult> Feature(int id, bool featured)
    {
        var result = await _testimonialService.FeatureAsync(id, featured);
        if (!result)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = featured ? "Testimonial featured on home page!" : "Testimonial unfeatured.";
        return RedirectToAction(nameof(Index));
    }

    // POST: Admin/Testimonials/Approve/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Approve testimonial. Admin only. Anti-forgery required.")]
    public async Task<IActionResult> Approve(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _testimonialService.ApproveAsync(id, userId);
        if (!result)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Testimonial approved!";
        return RedirectToAction(nameof(Index));
    }

    // POST: Admin/Testimonials/UpdateOrder
    [HttpPost]
    [ValidateAntiForgeryToken]
    [SwaggerOperation(Summary = "Update testimonial display order. Anti-forgery required.")]
    public async Task<IActionResult> UpdateOrder(int id, int order)
    {
        var result = await _testimonialService.UpdateDisplayOrderAsync(id, order);
        if (!result)
        {
            return NotFound();
        }

        return Ok();
    }
}
