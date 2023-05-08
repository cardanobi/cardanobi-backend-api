using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ApiCore.Models;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
// builder.Services.AddDbContext<cardanobiContext>(options =>
    // options.UseSqlServer(builder.Configuration.GetConnectionString("cardanobiContext") ?? throw new InvalidOperationException("Connection string 'cardanobiContext' not found.")));

var logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);
builder.Host.UseSerilog();

Log.Logger = logger;

Log.Information("Starting up");

// Log all environment variables
Log.Information("Environment Variables:");
foreach (var c in builder.Configuration.AsEnumerable())
{
    Log.Information(c.Key + " = " + c.Value);
}

static IEdmModel GetEdmModel()
{
    ODataConventionModelBuilder builder = new();
    builder.EntitySet<Epoch>("Epochs");
    builder.EntitySet<EpochParam>("EpochsParams");
    builder.EntitySet<EpochStake>("EpochsStakes");
    builder.EntitySet<PoolHash>("PoolsHashes");
    builder.EntitySet<PoolMetadata>("PoolsMetadata");
    builder.EntitySet<PoolOfflineData>("PoolsOfflineData");
    builder.EntitySet<PoolOfflineFetchError>("PoolsOfflineFetchErrors");
    builder.EntitySet<PoolUpdate>("PoolsUpdates");
    builder.EntitySet<PoolRelay>("PoolsRelays");
    builder.EntitySet<PoolStat>("PoolsStats");
    builder.EntitySet<AddressStat>("AddressesStats");
    builder.EntitySet<AddressInfo>("AddressesInfo");
    builder.EntitySet<Block>("Blocks");
    builder.EntitySet<SlotLeader>("SlotsLeaders");
    builder.EntitySet<Transaction>("Transactions");
    builder.EntitySet<TransactionOutput>("TransactionsOutputs");
    builder.EntitySet<MultiAsset>("MultiAssets");
    builder.EntitySet<MultiAssetTransactionOutput>("MultiAssetsTransactoinsOutputs");
    builder.EntitySet<TransactionInput>("TransactionInputs");
    builder.EntitySet<Datum>("Datums");
    builder.EntitySet<Script>("Scripts");
    builder.EntitySet<CollateralTransactionInput>("CollateralTransactionInputs");
    builder.EntitySet<CollateralTransactionOutput>("CollateralTransactionOutputs");
    builder.EntitySet<ReferenceTransactionInput>("ReferenceTransactionInputs");
    builder.EntitySet<Withdrawal>("Withdrawals");
    builder.EntitySet<MultiAssetTransactionMint>("MultiAssetTransactionMints");
    builder.EntitySet<TransactionMetadata>("TransactionMetadata");
    builder.EntitySet<StakeRegistration>("StakeRegistrations");
    builder.EntitySet<StakeDeregistration>("StakeDeregistrations");
    builder.EntitySet<Delegation>("Delegations");
    builder.EntitySet<Treasury>("TreasuryPayments");
    builder.EntitySet<Reserve>("ReservePayments");
    builder.EntitySet<PotTransfer>("PotTransfers");
    builder.EntitySet<ParamProposal>("ParamProposals");
    builder.EntitySet<PoolRetire>("PoolRetirements");
    builder.EntitySet<Redeemer>("Redeemers");
    builder.EntitySet<RedeemerData>("RedeemerData");
    builder.EntitySet<StakeAddress>("StakeAddresses");
    builder.EntitySet<PoolOwner>("PoolOwners");
    builder.EntitySet<MultiAssetCache>("MultiAssetCache");
    builder.EntitySet<MultiAssetAddressCache>("MultiAssetAddressCache");
    return builder.GetEdmModel();
}

// Add services to the container.
// https://docs.microsoft.com/en-gb/ef/core/performance/advanced-performance-topics?tabs=with-di%2Cwith-constant#dbcontext-pooling
builder.Services.AddDbContextPool<cardanobiCoreContext>(options => 
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("DbSyncPgsqlDatabase"))
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
);
builder.Services.AddDbContextPool<cardanobiCoreContext2>(options => 
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("DbSyncPgsqlDatabase"))
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
);
builder.Services.AddDbContextPool<cardanobiCoreContext3>(options => 
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("DbSyncPgsqlDatabase"))
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
);
builder.Services.AddDbContextPool<cardanobiBIContext>(options => 
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("DbSyncPgsqlDatabase"))
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
);

// builder.Services.AddControllers();
builder.Services.AddControllers().AddOData(opt => opt.AddRouteComponents("api/core/odata", GetEdmModel()).Select().Filter().OrderBy().SetMaxTop(20).Count());
builder.Services.AddControllers().AddOData(opt => opt.AddRouteComponents("api/bi/odata", GetEdmModel()).Select().Filter().OrderBy().SetMaxTop(20).Count());

// // Add API versioning capabilities
// builder.Services.AddApiVersioning(
//                 options =>
//                 {
//                     // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
//                     options.ReportApiVersions = true;
//                 });
// builder.Services.AddVersionedApiExplorer(
//     options =>
//     {
//         // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
//         // note: the specified format code will format the version as "'v'major[.minor][-status]"
//         options.GroupNameFormat = "'v'VVV";

//         // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
//         // can also be used to control the format of the API version in route templates
//         options.SubstituteApiVersionInUrl = true;
//     });


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();

    options.DocInclusionPredicate((name, api) => api.HttpMethod != null);

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "CardanoBI API",
        Description = "A fully open-source Business Intelligence API for Cardano.",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "Example Contact",
            Url = new Uri("https://example.com/contact")
        },
        License = new OpenApiLicense
        {
            Name = "Example License",
            Url = new Uri("https://example.com/license")
        }
    });

    options.TagActionsBy(api =>
    {
        if (api.GroupName != null)
        {
            return new[] { api.GroupName };
        }

        var controllerActionDescriptor = api.ActionDescriptor as ControllerActionDescriptor;
        if (controllerActionDescriptor != null)
        {
            return new[] { controllerActionDescriptor.ControllerName };
        }

        throw new InvalidOperationException("Unable to determine tag for endpoint.");
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.OAuth2,
        Scheme = "bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        }, new List<string>()
                    }
                });

    // using System.Reflection;
    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["DuendeIdentitySettings:Authority"];
        // options.RequireHttpsMetadata = false; // to support non https request in dev

        options.TokenValidationParameters.ValidateAudience = false;
        // options.TokenValidationParameters = new TokenValidationParameters
        // {
        //     ValidateAudience = false
        // };
    });
builder.Services.AddAuthorization(options =>
    options.AddPolicy("core-read", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "cardanobi-core-read");
    })
);
builder.Services.AddAuthorization(options =>
    options.AddPolicy("bi-read", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "cardanobi-bi-read");
    })
);

builder.Services.AddAuthorization(options =>
{
    var allowedNetworkType = builder.Configuration["NetworkType"] ?? string.Empty;
    if (string.IsNullOrEmpty(allowedNetworkType))
    {
        throw new Exception("Allowed network type cannot be null or empty.");
    }
    //Client claims key starts with client_
    options.AddPolicy("GlobalAuthRule", policy => policy.RequireClaim("client_network-type", allowedNetworkType));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // app.UseExceptionHandler("/Error");
    app.UseHsts(); // to signal to clients that only secure resource requests should be sent to the app
}

app.UseODataRouteDebug();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// app.MapControllers().RequireAuthorization("ApiScope");
app.MapControllers().RequireAuthorization("GlobalAuthRule");

app.Run();
