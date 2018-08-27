﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OrderApi.Data;
using OrderApi.Infrastructure.Filters;
using Swashbuckle.AspNetCore.Swagger;

namespace OrderApi
{
    public class Startup
    {
        // private readonly string _connectionString;
        ILogger _logger;

        public IConfiguration Configuration { get; }
        public Startup(ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger<Startup>();
            Configuration = configuration;
        }





        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {



            services.AddMvcCore(
                 options => options.Filters.Add(typeof(HttpGlobalExceptionFilter))
                 )
                  .AddJsonFormatters(
                   Options =>
                   {
                       Options.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                       Options.ContractResolver = new CamelCasePropertyNamesContractResolver();
                   }
                )
                  .AddApiExplorer();


            services.Configure<OrderSettings>(Configuration);


            ConfigureAuthService(services);

            // WaitForDBInit(_connectionString);
            //For Docker
            var server = Configuration["DatabaseServer"];
            var database = Configuration["DatabaseName"];
            var user = Configuration["DatabaseUser"];
            var password = Configuration["DatabaseUserPassword"];
            var connectionString = String.Format("Server={0};Database={1};User={2};Password={3};", server, database, user, password);

            services.AddDbContext<OrderDbContext>(
                options => options.UseSqlServer(connectionString));

            ////for iis
            //services.AddDbContext<OrderDbContext>(
            //    options => options.UseSqlServer(Configuration["ConnectionString"]));


            services.AddSwaggerGen(options =>
            {
                options.DescribeAllEnumsAsStrings();
                options.SwaggerDoc("v1", new Info
                {
                    Title = "Ordering HTTP API",
                    Version = "v1",
                    Description = "The Ordering Service HTTP API",
                    TermsOfService = "Terms Of Service"
                });
                options.AddSecurityDefinition("oauth2", new OAuth2Scheme
                {
                    Type = "oauth2",
                    Flow = "implicit",
                    AuthorizationUrl = $"{Configuration.GetValue<string>("IdentityUrl")}/connect/authorize",
                    TokenUrl = $"{Configuration.GetValue<string>("IdentityUrl")}/connect/token",
                    Scopes = new Dictionary<string, string>()
                    {
                        { "order", "Order Api" }
                    }

                });
                options.OperationFilter<AuthorizeCheckOperationFilter>();
            });
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });

        }
        private void ConfigureAuthService(IServiceCollection services)
        {
            // prevent from mapping "sub" claim to nameidentifier.
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            var identityUrl = Configuration.GetValue<string>("IdentityUrl");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(options =>
            {
                options.Authority = identityUrl;
                options.RequireHttpsMetadata = false;
                options.Audience = "order";

            });
        }
        private void WaitForDBInit(string connectionString)
        {
            var connection = new MySqlConnection(connectionString);
            int retries = 1;
            while (retries < 7)
            {
                try
                {
                    Console.WriteLine("Connecting to db. Trial: {0}", retries);
                    connection.Open();
                    connection.Close();
                    break;
                }
                catch (MySqlException)
                {
                    Thread.Sleep((int)Math.Pow(2, retries) * 1000);
                    retries++;
                }
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            var pathBase = Configuration["PATH_BASE"];
            if (!string.IsNullOrEmpty(pathBase))
            {
                app.UsePathBase(pathBase);
            }


            app.UseCors("CorsPolicy");
            app.UseAuthentication();

            app.UseSwagger()
              .UseSwaggerUI(c =>
              {
                  c.SwaggerEndpoint($"{ (!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty) }/swagger/v1/swagger.json", "OrderApi V1");
                  //c.ConfigureOAuth2("orderswaggerui", "", "", "Ordering Swagger UI");
              });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}/{id?}");
            });

        }
    }
}