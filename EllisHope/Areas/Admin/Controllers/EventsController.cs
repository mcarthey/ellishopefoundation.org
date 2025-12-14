using EllisHope.Areas.Admin.Models;
using EllisHope.Models.Domain;
using EllisHope.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EllisHope.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editor")]
public class EventsController : Controller
{
    private readonly IEventService _eventService;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public EventsController(IEventService eventService, IWebHostEnvironment environment, IConfiguration configuration)
    {
        _eventService = eventService;
        _environment = environment;
        _configuration = configuration;
    }

    // GET: Admin/Events
    public async Task<IActionResult> Index(string? searchTerm, bool showUnpublished = true, bool showUpcomingOnly = false)
    {
        IEnumerable<Event> events;

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            events = await _eventService.SearchEventsAsync(searchTerm);
        }
        else if (showUpcomingOnly)
        {
            events = await _eventService.GetUpcomingEventsAsync(100);
        }
        else
        {
            events = await _eventService.GetAllEventsAsync(showUnpublished);
        }

        var viewModel = new EventListViewModel
        {
            Events = events,
            SearchTerm = searchTerm,
            ShowUnpublished = showUnpublished,
            ShowUpcomingOnly = showUpcomingOnly
        };

        return View(viewModel);
    }

    // GET: Admin/Events/Create
    public IActionResult Create()
    {
        SetTinyMceApiKey();
        
        var viewModel = new EventViewModel
        {
            EventDate = DateTime.Now.AddDays(7),
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0)
        };

        return View(viewModel);
    }

    // POST: Admin/Events/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventViewModel model)
    {
        if (ModelState.IsValid)
        {
            var eventItem = new Event
            {
                Title = model.Title,
                Slug = string.IsNullOrWhiteSpace(model.Slug)
                    ? _eventService.GenerateSlug(model.Title)
                    : model.Slug,
                Description = model.Description,
                EventDate = model.EventDate,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                Location = model.Location,
                OrganizerName = model.OrganizerName,
                OrganizerEmail = model.OrganizerEmail,
                OrganizerPhone = model.OrganizerPhone,
                RegistrationUrl = model.RegistrationUrl,
                IsPublished = model.IsPublished,
                Tags = model.Tags,
                MaxAttendees = model.MaxAttendees
            };

            // Handle featured image - prioritize FeaturedImageUrl (from Media Library) over file upload
            if (!string.IsNullOrWhiteSpace(model.FeaturedImageUrl))
            {
                // Image selected from Media Library
                eventItem.FeaturedImageUrl = model.FeaturedImageUrl;
            }
            else if (model.FeaturedImageFile != null)
            {
                // Legacy: File uploaded directly (bypassing Media Library)
                eventItem.FeaturedImageUrl = await SaveFeaturedImageAsync(model.FeaturedImageFile);
            }

            await _eventService.CreateEventAsync(eventItem);

            TempData["SuccessMessage"] = "Event created successfully!";
            return RedirectToAction(nameof(Index));
        }

        SetTinyMceApiKey();
        return View(model);
    }

    // GET: Admin/Events/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        SetTinyMceApiKey();
        
        var eventItem = await _eventService.GetEventByIdAsync(id);
        if (eventItem == null)
        {
            return NotFound();
        }

        var viewModel = new EventViewModel
        {
            Id = eventItem.Id,
            Title = eventItem.Title,
            Slug = eventItem.Slug,
            Description = eventItem.Description,
            FeaturedImageUrl = eventItem.FeaturedImageUrl,
            EventDate = eventItem.EventDate,
            StartTime = eventItem.StartTime,
            EndTime = eventItem.EndTime,
            Location = eventItem.Location,
            OrganizerName = eventItem.OrganizerName,
            OrganizerEmail = eventItem.OrganizerEmail,
            OrganizerPhone = eventItem.OrganizerPhone,
            RegistrationUrl = eventItem.RegistrationUrl,
            IsPublished = eventItem.IsPublished,
            Tags = eventItem.Tags,
            MaxAttendees = eventItem.MaxAttendees
        };

        return View(viewModel);
    }

    // POST: Admin/Events/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EventViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var eventItem = await _eventService.GetEventByIdAsync(id);
            if (eventItem == null)
            {
                return NotFound();
            }

            eventItem.Title = model.Title;
            eventItem.Slug = string.IsNullOrWhiteSpace(model.Slug)
                ? _eventService.GenerateSlug(model.Title)
                : model.Slug;
            eventItem.Description = model.Description;
            eventItem.EventDate = model.EventDate;
            eventItem.StartTime = model.StartTime;
            eventItem.EndTime = model.EndTime;
            eventItem.Location = model.Location;
            eventItem.OrganizerName = model.OrganizerName;
            eventItem.OrganizerEmail = model.OrganizerEmail;
            eventItem.OrganizerPhone = model.OrganizerPhone;
            eventItem.RegistrationUrl = model.RegistrationUrl;
            eventItem.IsPublished = model.IsPublished;
            eventItem.Tags = model.Tags;
            eventItem.MaxAttendees = model.MaxAttendees;

            // Handle featured image - prioritize FeaturedImageUrl (from Media Library) over file upload
            if (!string.IsNullOrWhiteSpace(model.FeaturedImageUrl) && model.FeaturedImageUrl != eventItem.FeaturedImageUrl)
            {
                // New image selected from Media Library
                // Only delete old if it was from local uploads, not Media Library
                if (!string.IsNullOrEmpty(eventItem.FeaturedImageUrl) && 
                    (eventItem.FeaturedImageUrl.Contains("/uploads/causes/") || 
                     eventItem.FeaturedImageUrl.Contains("/uploads/blog/") || 
                     eventItem.FeaturedImageUrl.Contains("/uploads/events/")))
                {
                    DeleteFeaturedImage(eventItem.FeaturedImageUrl);
                }
                eventItem.FeaturedImageUrl = model.FeaturedImageUrl;
            }
            else if (model.FeaturedImageFile != null)
            {
                // Legacy: New file uploaded directly (bypassing Media Library)
                // Delete old image if exists and was from local uploads
                if (!string.IsNullOrEmpty(eventItem.FeaturedImageUrl) && 
                    (eventItem.FeaturedImageUrl.Contains("/uploads/causes/") || 
                     eventItem.FeaturedImageUrl.Contains("/uploads/blog/") || 
                     eventItem.FeaturedImageUrl.Contains("/uploads/events/")))
                {
                    DeleteFeaturedImage(eventItem.FeaturedImageUrl);
                }
                eventItem.FeaturedImageUrl = await SaveFeaturedImageAsync(model.FeaturedImageFile);
            }

            await _eventService.UpdateEventAsync(eventItem);

            TempData["SuccessMessage"] = "Event updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        SetTinyMceApiKey();
        return View(model);
    }

    // POST: Admin/Events/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var eventItem = await _eventService.GetEventByIdAsync(id);
        if (eventItem == null)
        {
            return NotFound();
        }

        // Delete featured image if exists
        if (!string.IsNullOrEmpty(eventItem.FeaturedImageUrl))
        {
            DeleteFeaturedImage(eventItem.FeaturedImageUrl);
        }

        await _eventService.DeleteEventAsync(id);

        TempData["SuccessMessage"] = "Event deleted successfully!";
        return RedirectToAction(nameof(Index));
    }

    // Helper methods
    private void SetTinyMceApiKey()
    {
        ViewData["TinyMceApiKey"] = _configuration["TinyMCE:ApiKey"] ?? "no-api-key";
    }

    private async Task<string> SaveFeaturedImageAsync(IFormFile file)
    {
        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "events");
        Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        return $"/uploads/events/{uniqueFileName}";
    }

    private void DeleteFeaturedImage(string imageUrl)
    {
        var imagePath = Path.Combine(_environment.WebRootPath, imageUrl.TrimStart('/'));
        if (System.IO.File.Exists(imagePath))
        {
            System.IO.File.Delete(imagePath);
        }
    }
}
