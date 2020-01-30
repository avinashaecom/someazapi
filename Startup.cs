using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using AECOM.GNGApi.Models;
using JwtAuthentication.Server.Service;

namespace AECOM.GNGApi
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
            // Add the database to the API context
            services.AddDbContext<GoNoGoContext>(options =>
               options.UseSqlServer(Configuration.GetConnectionString("GoNoGoDatabase")));

            // Add all the controllers
            services.AddControllers();
            services.AddControllers().AddJsonOptions(options =>
                options.JsonSerializerOptions.PropertyNamingPolicy = null
            );
            
            // Add Authentication and Authorization methods
            
            // Set Microsoft AAD as in https://docs.microsoft.com/en-us/azure/active-directory/develop/tutorial-v2-javascript-spa
            if (true)
            {
                services.AddAuthentication(options =>
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme
                )
                .AddJwtBearer( options => 
                {
                    string authority = Configuration["Authentication:AzureAD:Instance"] + "/" + Configuration["Authentication:AzureAD:TenantId"] + "/";
                    string issuer = authority + "v2.0";
                    options.Authority = issuer;
                    options.Audience = Configuration["Authentication:AzureAD:ClientId"];
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidIssuer = issuer
                     };
                });
            }

            // Set Jwt via Local Token generator as the default auth scheme
            // To use this, visit {localhost:44331/token  
            if (false) // WORKS!
            {
                // Prevent Mapping https://mderriey.com/2019/06/23/where-are-my-jwt-claims/
                JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

                //Setting up Jwt Authentication
                services.AddTransient<IJwtTokenService, JwtTokenService>();
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = Configuration["Authentication:Jwt:Issuer"],
                            ValidAudience = Configuration["Authentication:Jwt:Audience"],
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Authentication:Jwt:Key"]))
                        };
                    });
            }

            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy(JwtBearerDefaults.AuthenticationScheme,
            //        builder =>
            //        {
            //            builder.
            //            AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).
            //            RequireAuthenticatedUser().
            //            Build();
            //        }
            //    );
            //}
            //);

            // Register the HTTP Context
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Register the Swagger services
            services.AddSwaggerDocument(config =>
            {
                config.PostProcess = document =>
                {
                    document.Info.Version = "GNG API v1.0";
                    document.Info.Title = "GNG-DEV API";
                    document.Info.Description = "GNG APIs Built On .NETCore 3.0";
                    document.Info.TermsOfService = "None";
                   
                    //document.Info.Contact = new NSwag.OpenApiContact
                    //{
                    //    Name = "AECOM Development Team",
                    //    Email = "jake.white@aecom.com",
                    //    Url = ""
                    //};
                    //document.Info.License = new NSwag.OpenApiLicense
                    //{
                    //    Name = "AECOM on Azure ",
                    //    Url = "AECOM"
                    //};
                };
            });

            //Register the Swagger Generator

            //services.AddSwaggerGen(c => 
            //{ c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
            //    { Title = "GNG \"DEV\" API", Description = "APIs Developed Using DotNet Core Api 3.0 - with swagger" 
            //    }); 
            //});



        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            
            // global cors policy
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
            
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseHttpsRedirection();

            app.UseEndpoints(endpoints =>
                endpoints.MapControllers()
            );

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            // app.UseSwagger();

            // Register the Swagger generator and the Swagger UI middlewares
            app.UseOpenApi();
            app.UseSwaggerUi3();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "GNG API V1");
            });

            // Support logging into AAD via this webapp

            app.UseDefaultFiles();
            app.UseStaticFiles();

        }
    }
}
