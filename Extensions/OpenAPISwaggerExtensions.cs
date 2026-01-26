using Microsoft.OpenApi;


namespace CBCMissionaryWallApi.Extensions
{
    public static class OpenAPISwaggerExtensions
    {
        public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo()
                {
                    Title = "CBC Missions Wall API",
                    Version = "v1",
                    Description = """
                    
                    <img src="/img/MWStudioNotes_main.jpg" height="120" />
                    
                    ## MW Dev Studio

                    Internal API for managing CBC Missionary Wall, missionaries, and their locationss.

                    ### Key Features:
                    - Missionary Location
                    - Stats on Missionary and family
                    - Secure media storage
                    - User role management

                    """,
                    Contact = new OpenApiContact
                    {
                        Name = "MW Dev Studio",
                        Url = new Uri("https://mwdevstudio.net/?blog=y"),
                        Email = "mikey32905@1791.com"
                    }
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' [space] and then your valid JWT token."
                });

                c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("bearer", document)] = []
                });

                //c.AddSecurityRequirement(new OpenApiSecurityRequirement
                //{
                //    {
                //        new OpenApiSecurityScheme
                //        {
                //             Reference = new OpenApiReference
                //            {
                //               Type = ReferenceType.SecurityScheme,
                //               Id = "Bearer"
                //            }
                //        },
                //        Array.Empty<string>()
                //    }
                //});


            });


            return services;
        }
    }
}
