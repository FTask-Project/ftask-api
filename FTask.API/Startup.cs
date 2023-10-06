using CloudinaryDotNet;
using FTask.API.Middleware;
using FTask.API.Permission;
using FTask.API.Service;
using FTask.Repository.application;
using FTask.API.Common;
using FTask.Repository.Data;
using FTask.Repository.Identity;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using static FTask.API.Middleware.GlobalExceptionMiddleware;
using HttpMethod = System.Net.Http.HttpMethod;
using FTask.Repository;
using FTask.Service;
using Microsoft.AspNetCore.Identity;
using FTask.API.Mapper;

namespace FTask.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }


        public void ConfigureServices(IServiceCollection services)
        {
            #region Identity
            services.AddIdentity<User, Role>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;

                //password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 5;
                options.Password.RequiredUniqueChars = 0;

                //Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                //UserName settings
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

                options.Tokens.EmailConfirmationTokenProvider = "UserTokenProvider";
                options.Tokens.ChangeEmailTokenProvider = "UserTokenProvider";
                options.Tokens.PasswordResetTokenProvider = "UserTokenProvider";
            })
                .AddEntityFrameworkStores<ApplicationDbContext>();
            //.AddTokenProvider<TokenProvider<User>>("UserTokenProvider");

            services.AddIdentityCore<Lecturer>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;

                //password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 5;
                options.Password.RequiredUniqueChars = 0;

                //Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                //UserName settings
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

                options.Tokens.EmailConfirmationTokenProvider = "UserTokenProvider";
                options.Tokens.ChangeEmailTokenProvider = "UserTokenProvider";
                options.Tokens.PasswordResetTokenProvider = "UserTokenProvider";
            })
                .AddEntityFrameworkStores<ApplicationDbContext>();
            //.AddTokenProvider<TokenProvider<User>>("UserTokenProvider");
            #endregion

            #region Cloudinary
            var cloudinaryConfig = Configuration.GetSection("CloudinaryConfig").Get<CloudinaryConfig>();
            Account cloudinaryAccount = new Account
            {
                Cloud = cloudinaryConfig.CloudName,
                ApiKey = cloudinaryConfig.ApiKey,
                ApiSecret = cloudinaryConfig.ApiSecret,
            };
            Cloudinary cloudinary = new Cloudinary(cloudinaryAccount);
            services.AddSingleton(cloudinary);
            #endregion

            services.AddSingleton<IKeyManager, KeyManager>();

            #region Email
            var emailConfig = Configuration.GetSection("EmailConfig").Get<EmailConfig>();
            services.AddSingleton(emailConfig);
            services.AddTransient<IMailService,MailService>();
            #endregion

            services.Configure<FormOptions>(o =>
            {
                o.ValueLengthLimit = int.MaxValue;
                o.MultipartBodyLengthLimit = int.MaxValue;
                o.MemoryBufferThreshold = int.MaxValue;
            });

            services.AddRepository(Configuration);
            services.AddService(Configuration);
            services.AddAutoMapper(typeof(ModelToResource), typeof(ResourceToModel));
            services.AddScoped<IBackgroundTaskService, BackgroundTaskService>();
            services.AddScoped<IJWTTokenService<IdentityUser<Guid>>, JWTTokenService<IdentityUser<Guid>>>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<ICurrentUserService,CurrentUserService>();
            services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
            services.AddScoped<IAuthorizationHandler, HasScopeHandler>();

            services.AddControllers(options =>
            {
                // Add Global Exception Filter here
                //options.Filters.Add<HttpResponseExceptionFilter>();
            })
                .AddJsonOptions(o =>
                {
                    o.JsonSerializerOptions.PropertyNamingPolicy = null;
                    o.JsonSerializerOptions.DictionaryKeyPolicy = null;
                })
                .AddNewtonsoftJson(option =>
            option.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            services.AddRazorPages();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "FTask.API", Version = "v1" });
                c.AddSecurityDefinition("Bearer",
                    new OpenApiSecurityScheme
                    {
                        Description = "JWT Authorization header using the Bearer scheme",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.Http,
                        BearerFormat = "JWT",
                        Scheme = "Bearer"
                    });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddCookie("cookie")
            .AddJwtBearer(x =>
            {
                var keyManager = new KeyManager();
                var key = keyManager.RsaKey;

                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = Configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = Configuration["Jwt:Issuer"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new RsaSecurityKey(key ?? throw new ArgumentException("Key not found for authentication scheme"))
                };
            })
            .AddOAuth("google", o =>
            {
                o.SignInScheme = "cookie";

                o.ClientId = "118248411285-pj96okif8j20j05g78kbhu5lghd5450l.apps.googleusercontent.com";
                o.ClientSecret = "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";

                o.AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/auth";
                o.TokenEndpoint = "https://oauth2.googleapis.com/token";
                o.UserInformationEndpoint = "https://www.googleapis.com/oauth2/v1/userinfo";

                o.CallbackPath = "/oauth/google-cb";
                o.SaveTokens = true;

                o.Scope.Add("profile");
                o.Scope.Add("email");

                o.ClaimActions.MapJsonKey("sub", "id");
                o.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                o.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");

                o.Events.OnCreatingTicket = async ctx =>
                {
                    // Can get service here
                    // ctx.HttpContext.RequestServices<>();
                    using var request = new HttpRequestMessage(HttpMethod.Get, ctx.Options.UserInformationEndpoint);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ctx.AccessToken);
                    using var result = ctx.Backchannel.Send(request);
                    var user = await result.Content.ReadFromJsonAsync<JsonElement>();
                    ctx.RunClaimActions(user);
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy =>
                {
                    policy.Requirements.Add(new HasScopeRequirement("Admin", Configuration["Jwt:Issuer"]!));
                });

                options.AddPolicy("Manager", policy =>
                {
                    policy.Requirements.Add(new HasScopeRequirement("Manager", Configuration["Jwt:Issuer"]!));
                });

                options.AddPolicy("Lecturer", policy =>
                {
                    policy.Requirements.Add(new HasScopeRequirement("Lecturer", Configuration["Jwt:Issuer"]!));
                });
            });

            /*services.AddCors(options =>
            {
                options.AddPolicy("ClientPermission", policy =>
                {
                    policy
                        .WithOrigins("http://vm.e-biz.com.vn",
                                     "http://fevm.e-biz.com.vn",
                                     "http://localhost:3000")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });*/
            services.AddCors(options =>
            {
                options.AddPolicy("All", policy =>
                {
                    policy.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin();
                });
            });

            services.AddHangfire(configuration =>
            {
                configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(Configuration.GetConnectionString("MsSQLConnection"), new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true,
                });
            });
            

            //services.AddHangfireServer();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (!env.IsDevelopment())
            {
            }

            app.UseStatusCodePages(async context =>
            {
                if(context.HttpContext.Response.StatusCode == 401)
                {
                    // Customize the response for 401 Unauthorized status code
                    context.HttpContext.Response.ContentType = "application/json";
                    var error = new ErrorDetails()
                    {
                        StatusCode = context.HttpContext.Response.StatusCode,
                        Message = "Unauthorize: You do not have permission to access this resource."
                    };
                    await context.HttpContext.Response.WriteAsync(error.ToString());
                }

                if(context.HttpContext.Response.StatusCode == 403)
                {
                    // Customize the response for 403 Forbidden status code
                    context.HttpContext.Response.ContentType = "application/json";
                    var error = new ErrorDetails()
                    {
                        StatusCode = context.HttpContext.Response.StatusCode,
                        Message = "Forbidden: You do not have sufficient privileges to access this resource."
                    };
                    await context.HttpContext.Response.WriteAsync(error.ToString());
                }
            });

            app.UseMiddleware<GlobalExceptionMiddleware>();

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FTask.API v1"));

            app.UseHttpsRedirection();

            /*app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] {new AuthorizationFilter()}
            });*/

            app.UseStaticFiles();
            // app.UseCookiePolicy();

            app.UseRouting();
            // app.UseRateLimiter();
            // app.UseRequestLocalization();

            app.UseCors("All");

            app.UseAuthentication();

            app.UseAuthorization();
            // app.UseSession();
            // app.UseResponseCompression();
            // app.UseResponseCaching();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();

                endpoints.MapGet("/login-google", context =>
                {
                    var linkGenerator = context.RequestServices.GetRequiredService<LinkGenerator>();
                    var redirectUri = linkGenerator.GetUriByAction(context, "UserInformation", "TestAuth");

                    return AuthenticationHttpContextExtensions
                        .ChallengeAsync(context, "google",
                        new AuthenticationProperties
                        {
                            RedirectUri = redirectUri,
                        });
                });
            });
        }
    }
}
