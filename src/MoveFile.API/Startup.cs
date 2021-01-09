using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MoveFile.API.Infra;
using MoveFile.API.Services;
using RabbitMQ.Client;

namespace MoveFile.API
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
            services.AddTransient(sp => new ConnectionFactory()
            {
                HostName = Configuration.GetSection("Connections:RabbitMq:HostName").Value,
                Port = Configuration.GetSection("Connections:RabbitMq:Port").Get<int>(),
                UserName = Configuration.GetSection("Connections:RabbitMq:UserName").Value,
                Password = Configuration.GetSection("Connections:RabbitMq:Password").Value,
            });

            services.AddTransient<IConnection>(sp => sp.GetRequiredService<ConnectionFactory>().CreateConnection());

            services.AddTransient<IModel>(sp => sp.GetRequiredService<IConnection>().CreateModel());

            services.AddTransient(typeof(IQueueConsumer<>), typeof(QueueConsumer<>));

            services.AddTransient<IMoveFileAppService, MoveFileAppService>();

            services.AddTransient<ApplicationInitializer>();

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.ApplicationServices.GetRequiredService<ApplicationInitializer>().InitializeRabbitMQ();
            app.ApplicationServices.GetRequiredService<IMoveFileAppService>().Consumer();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
