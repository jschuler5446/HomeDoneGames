using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PoolHouseStudio.HomeDoneGames.DataAccessLayer;
using PoolHouseStudio.HomeDoneGames.DataAccessLayer.Repositories;
using PoolHouseStudio.HomeDoneGames.Service.Services;
using PoolHouseStudio.HomeDoneGames.Web.Extensions;
using PoolHouseStudio.HomeDoneGames.Web.Hubs;

namespace PoolHouseStudio.HomeDoneGames
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
            services.AddCors(options =>
            {
                options.AddPolicy("Default",
                builder =>
                {
                    builder
                    .WithOrigins("http://localhost:3000", "http://localhost:5000")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            services.AddHealthChecks();
            services.AddDbContext<DataDbContext>(options => 
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // Repositories

            services.AddScoped(typeof(IAsyncRepository<>), typeof(Repository<>));
            services.AddScoped<IGameTypeRepository, GameTypeRepository>();
            services.AddScoped<IRoomRepository, RoomRepository>();

            // Services

            services.AddTransient<IHubService, HubService>();
            services.AddTransient<IGameTypeService, GameTypeService>();
            services.AddTransient<IRoomService, RoomService>();

            services.AddControllers();
            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseCors("Default");
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.ConfigureCustomExceptionMiddleware();
            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/api/healthcheck");
                endpoints.MapControllers();
                endpoints.MapHub<GameHub>("/gamehub");
            });
        }
    }
}
