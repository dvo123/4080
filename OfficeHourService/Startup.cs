using OfficeHourService.Controllers;

namespace OfficeHourService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add controllers and related services
            services.AddControllers();

            // Add your other services here, if needed.
            // For example, you can add EmailService as a scoped service:
            // services.AddScoped<EmailService>();

            // Enable CORS (if needed)
            services.AddCors(options =>
            {
                options.AddPolicy("AllowClientApp", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyHeader()
                           .AllowAnyMethod();
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios.
                app.UseHsts();
            }

            // Enable CORS (if needed)
            // Initialize officeHours when the application starts
            var officeHourController = new OfficeHourController();
            officeHourController.GetOfficeHours("path/to/office-hours-file.csv");
            app.UseCors("AllowClientApp");

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}