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
    private readonly IMediaService _mediaService;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public EventsController(
        IEventService eventService, 
        IMediaService mediaService,
        IWebHostEnvironment environment, 
        IConfiguration configuration)
    {
        _eventService = eventService;
        _mediaService = mediaService;
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
        // Log all ModelState errors for debugging
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .Select(x => new
                {
                    Field = x.Key,
                    Errors = x.Value?.Errors.Select(e => e.ErrorMessage ?? e.Exception?.Message).ToList()
                })
                .ToList();

            var errorMessage = string.Join("; ", errors.Select(e => 
                $"{e.Field}: {string.Join(", ", e.Errors ?? new List<string>())}"));

            TempData["ErrorMessage"] = $"Validation failed: {errorMessage}";
            SetTinyMceApiKey();
            return View(model);
        }

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

        // Handle featured image - prioritize Media Library
        if (!string.IsNullOrWhiteSpace(model.FeaturedImageUrl))
        {
            eventItem.FeaturedImageUrl = model.FeaturedImageUrl;
        }
        else if (model.FeaturedImageFile != null)
        {
            // Upload to centralized Media Manager
            var uploadedBy = User.Identity?.Name ?? "System";
            var media = await _mediaService.UploadLocalImageAsync(
                model.FeaturedImageFile,
                $"Event: {model.Title}",
                model.Title,
                MediaCategory.Event,
                model.Tags,
                uploadedBy);
            
            if (media != null)
            {
                eventItem.FeaturedImageUrl = media.FilePath;
                
                // Track usage
                await _eventService.CreateEventAsync(eventItem);
                await _mediaService.TrackMediaUsageAsync(media.Id, "Event", eventItem.Id, UsageType.Featured);
                
                TempData["SuccessMessage"] = "Event created successfully!";
                return RedirectToAction(nameof(Index));
            }
        }

        await _eventService.CreateEventAsync(eventItem);

        TempData["SuccessMessage"] = "Event created successfully!";
        return RedirectToAction(nameof(Index));
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

        // Log all ModelState errors for debugging
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .Select(x => new
                {
                    Field = x.Key,
                    Errors = x.Value?.Errors.Select(e => e.ErrorMessage ?? e.Exception?.Message).ToList()
                })
                .ToList();

            var errorMessage = string.Join("; ", errors.Select(e => 
                $"{e.Field}: {string.Join(", ", e.Errors ?? new List<string>())}"));

            TempData["ErrorMessage"] = $"Validation failed: {errorMessage}";
            SetTinyMceApiKey();
            return View(model);
        }

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

        // Handle featured image update
        if (!string.IsNullOrWhiteSpace(model.FeaturedImageUrl) && model.FeaturedImageUrl != eventItem.FeaturedImageUrl)
        {
            // New image from Media Library - just update URL
            eventItem.FeaturedImageUrl = model.FeaturedImageUrl;
        }
        else if (model.FeaturedImageFile != null)
        {
            // New file upload - use Media Manager
            var uploadedBy = User.Identity?.Name ?? "System";
            var media = await _mediaService.UploadLocalImageAsync(
                model.FeaturedImageFile,
                $"Event: {model.Title}",
                model.Title,
                MediaCategory.Event,
                model.Tags,
                uploadedBy);
            
            if (media != null)
            {
                eventItem.FeaturedImageUrl = media.FilePath;
                
                // Save first to get the ID
                await _eventService.UpdateEventAsync(eventItem);
                
                // Then track usage
                await _mediaService.TrackMediaUsageAsync(media.Id, "Event", eventItem.Id, UsageType.Featured);
                
                TempData["SuccessMessage"] = "Event updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to upload image. Please try again.";
                SetTinyMceApiKey();
                return View(model);
            }
        }
        // If neither condition is met, image stays the same (which is correct)

        await _eventService.UpdateEventAsync(eventItem);

        TempData["SuccessMessage"] = "Event updated successfully!";
        return RedirectToAction(nameof(Index));
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

        // Remove media usage tracking
        if (!string.IsNullOrEmpty(eventItem.FeaturedImageUrl))
        {
            // Find media by path
            var allMedia = await _mediaService.GetAllMediaAsync();
            var media = allMedia.FirstOrDefault(m => m.FilePath == eventItem.FeaturedImageUrl);
            if (media != null)
            {
                await _mediaService.RemoveMediaUsageAsync(media.Id, "Event", id);
            }
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
}
