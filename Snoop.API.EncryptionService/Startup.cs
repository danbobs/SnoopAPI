using System.Security.Cryptography;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Snoop.API.EncryptionService.Models;
using Snoop.API.EncryptionService.Services;
using Snoop.API.EncryptionService.Services.Interfaces;

namespace Snoop.API.EncryptionService
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

            services.AddControllers();
            services.AddResponseCompression();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "EncryptionService", Version = "v1" });
            });

            services.AddAWSService<IAmazonS3>(new AWSOptions
            {
                // credentials omitted!
                Region = Amazon.RegionEndpoint.GetBySystemName("eu-west-2")
            });

            /*============= Dependency Injection ============= */

            // Stub implementations
            //services.AddScoped<IEncrypter, StubEncrypter>();
            //services.AddScoped<IKeyStore<SimpleKey>, FileKeyStore<SimpleKey>>();

            services.AddScoped<IEncrypter, SymmetricKeyEncrypter>();
            services.AddTransient<SymmetricAlgorithm, AesCryptoServiceProvider>();      // could switch this to DES, RC2, Rijndael or TripleDES
            services.AddScoped<IKeyStore<SymmetricKey>, S3KeyStore<SymmetricKey>>();
            services.AddScoped<IKeyGenerator, KeyGenerator>();


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "EncryptionService v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseResponseCompression();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
