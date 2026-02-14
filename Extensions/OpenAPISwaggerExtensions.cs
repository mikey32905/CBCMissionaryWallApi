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
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });



                string[] hiddenEndpoints = [
                         "api/auth/register",
                        "api/auth/refresh",
                        "api/auth/confirmemail",
                        "api/auth/resendconfirmationemail",
                        "api/auth/forgotpassword",
                        "api/auth/resetpassword",
                        "api/auth/manage",
                        "api/auth/manage/info",
                        "api/auth/manage/2fa"
                     ];

                c.DocInclusionPredicate((docName, apiDesc) =>
                {
                    var path = apiDesc.RelativePath?.ToLowerInvariant();

                    if (path is null)
                        return false;

                    if (hiddenEndpoints.Contains(path, StringComparer.OrdinalIgnoreCase))
                    {
                        return false;
                    }

                    return true;

                });


            });


            return services;
        }
    }
}
