using Microsoft.AspNetCore.Identity;
using EllisHope.Models.Domain;

namespace EllisHope.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

        // Create roles
        string[] roleNames = { "Admin", "Editor", "User" };
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
            adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
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
    }
}
