using Microsoft.AspNetCore.Identity;
using EllisHope.Models.Domain;

namespace EllisHope.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

        // Create roles - Updated to match new UserRole enum
        string[] roleNames = { "Admin", "BoardMember", "Sponsor", "Client", "Member", "Editor" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Create default admin user
        var adminEmail = "admin@ellishope.org";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Administrator",
                UserRole = UserRole.Admin,
                Status = MembershipStatus.Active,
                IsActive = true,
                JoinedDate = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123456");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                Console.WriteLine($"Admin user created: {adminEmail}");
                Console.WriteLine("Default password: Admin@123456");
                Console.WriteLine("IMPORTANT: Change this password after first login!");
            }
        }
        else
        {
            // Update existing admin user if needed
            bool needsUpdate = false;
            
            if (adminUser.UserRole != UserRole.Admin)
            {
                adminUser.UserRole = UserRole.Admin;
                needsUpdate = true;
            }
            
            if (adminUser.Status != MembershipStatus.Active)
            {
                adminUser.Status = MembershipStatus.Active;
                needsUpdate = true;
            }
            
            if (!adminUser.IsActive)
            {
                adminUser.IsActive = true;
                needsUpdate = true;
            }
            
            // Ensure name fields are set
            if (string.IsNullOrEmpty(adminUser.FirstName))
            {
                adminUser.FirstName = "System";
                needsUpdate = true;
            }
            
            if (string.IsNullOrEmpty(adminUser.LastName))
            {
                adminUser.LastName = "Administrator";
                needsUpdate = true;
            }
            
            if (needsUpdate)
            {
                await userManager.UpdateAsync(adminUser);
                Console.WriteLine($"Admin user updated: {adminEmail}");
            }
            
            // Ensure admin is in Admin role
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                Console.WriteLine($"Added Admin role to: {adminEmail}");
            }
        }

        // Seed default blog categories
        if (!context.BlogCategories.Any())
        {
            var categories = new[]
            {
                new BlogCategory { Name = "Children", Slug = "children", Description = "Programs and initiatives focused on children" },
                new BlogCategory { Name = "Education", Slug = "education", Description = "Educational programs and resources" },
                new BlogCategory { Name = "Healthcare", Slug = "healthcare", Description = "Health and medical support initiatives" },
                new BlogCategory { Name = "Community", Slug = "community", Description = "Community outreach and support" },
                new BlogCategory { Name = "Fundraising", Slug = "fundraising", Description = "Fundraising events and campaigns" },
                new BlogCategory { Name = "Volunteer", Slug = "volunteer", Description = "Volunteer opportunities and stories" }
            };

            context.BlogCategories.AddRange(categories);
            await context.SaveChangesAsync();
            Console.WriteLine("Default blog categories created.");
        }

        // Seed default image sizes
        if (!context.ImageSizes.Any())
        {
            var imageSizes = new[]
            {
                // Page sizes
                new ImageSize { Name = "Page Header", Description = "Page breadcrumb backgrounds", Width = 1800, Height = 540, Category = MediaCategory.Page, IsActive = true },
                new ImageSize { Name = "About Portrait", Description = "Vertical about/story images", Width = 587, Height = 695, Category = MediaCategory.Page, IsActive = true },
                new ImageSize { Name = "About Landscape", Description = "Landscape about/story images", Width = 663, Height = 839, Category = MediaCategory.Page, IsActive = true },
                new ImageSize { Name = "Cause Card", Description = "Cause/program cards", Width = 450, Height = 300, Category = MediaCategory.Page, IsActive = true },
                new ImageSize { Name = "Social Share", Description = "Open Graph/social media images", Width = 1200, Height = 630, Category = MediaCategory.Page, IsActive = true },

                // Event sizes
                new ImageSize { Name = "Event List", Description = "Event cards in list view", Width = 630, Height = 450, Category = MediaCategory.Event, IsActive = true },
                new ImageSize { Name = "Event Featured", Description = "Event detail page featured image", Width = 1320, Height = 743, Category = MediaCategory.Event, IsActive = true },

                // Blog sizes
                new ImageSize { Name = "Blog Featured", Description = "Blog post featured images", Width = 425, Height = 500, Category = MediaCategory.Blog, IsActive = true },
                new ImageSize { Name = "Blog Thumbnail", Description = "Blog post thumbnails", Width = 425, Height = 500, Category = MediaCategory.Blog, IsActive = true },
                new ImageSize { Name = "Blog Mini", Description = "Sidebar/related post thumbnails", Width = 100, Height = 84, Category = MediaCategory.Blog, IsActive = true },

                // Team sizes
                new ImageSize { Name = "Team Full", Description = "Full team member photo", Width = 400, Height = 500, Category = MediaCategory.Team, IsActive = true },
                new ImageSize { Name = "Team Headshot Small", Description = "Small avatar/headshot", Width = 70, Height = 70, Category = MediaCategory.Team, IsActive = true },
                new ImageSize { Name = "Team Headshot Medium", Description = "Medium avatar/headshot", Width = 80, Height = 80, Category = MediaCategory.Team, IsActive = true },
                new ImageSize { Name = "Team Headshot Large", Description = "Large avatar/headshot", Width = 90, Height = 90, Category = MediaCategory.Team, IsActive = true },

                // Hero/Background sizes
                new ImageSize { Name = "Hero Background", Description = "Background sections and hero images", Width = 1800, Height = 855, Category = MediaCategory.Hero, IsActive = true },
                new ImageSize { Name = "Hero Main", Description = "Main hero image", Width = 1920, Height = 1080, Category = MediaCategory.Hero, IsActive = true },

                // Gallery sizes
                new ImageSize { Name = "Gallery Large", Description = "Gallery large images", Width = 1200, Height = 800, Category = MediaCategory.Gallery, IsActive = true },
                new ImageSize { Name = "Gallery Thumbnail", Description = "Gallery thumbnails", Width = 300, Height = 300, Category = MediaCategory.Gallery, IsActive = true }
            };

            context.ImageSizes.AddRange(imageSizes);
            await context.SaveChangesAsync();
            Console.WriteLine("Default image sizes created.");
        }
    }
}
