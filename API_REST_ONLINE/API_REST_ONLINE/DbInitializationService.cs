using API_REST_ONLINE.Models;
using Microsoft.EntityFrameworkCore;

namespace API_REST_ONLINE
{
    public static class DbInitializationService
    {

        public static void AddValuesOnStartup(this IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();
            using ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                if (!dbContext.rank.Any())
                {
                    // Add ranks only if the table is empty
                    dbContext.rank.AddRange(
                        new Rank { name = "No Rank" },
                        new Rank { name = "Bronze" },
                        new Rank { name = "Silver" },
                        new Rank { name = "Gold" },
                        new Rank { name = "Platinum" },
                        new Rank { name = "Diamond" }
                    );


                    // Save changes to the database
                    dbContext.SaveChanges();
                }
                if (!dbContext.success.Any())
                {
                    // Add ranks only if the table is empty
                    dbContext.success.AddRange(new[]
                    {
                        new Success { name = "First Blood", description = "You killed someone.", imageurl = "image1url" },
                        new Success { name = "First Death", description = "Someone killed you.", imageurl = "image2url" },
                        // Add more Success objects as needed
                    });


                    dbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the process
                // You can log the exception or take appropriate action based on your requirements
                Console.WriteLine($"An error occurred while adding values on startup");
            }
        }

    }
}
