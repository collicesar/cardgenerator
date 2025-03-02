using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using LambdaFunction.Storage;

namespace CardGenerator;

[ExcludeFromCodeCoverage]
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Getting Env Variables
        var awsServiceUrl = Environment.GetEnvironmentVariable("AWS_SERVICE_URL") ?? "";
        var awsAccessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY") ?? "";
        var awsSecretKey = Environment.GetEnvironmentVariable("AWS_SECRET_KEY") ?? "";

        var useCredentials = awsAccessKey != string.Empty
                             && awsSecretKey != string.Empty
                             && awsServiceUrl != string.Empty;

        if (useCredentials)
        {
            var awsOptions = new AWSOptions
            {
                DefaultClientConfig =
                {
                    ServiceURL = awsServiceUrl,
                    AuthenticationRegion = "us-east-1"
                },
                Credentials = new BasicAWSCredentials(awsAccessKey, awsSecretKey)
            };
            services.AddAWSService<IAmazonS3>(awsOptions);
            services.AddSingleton<IBucketStorage, FileSystemStorage>();
        }
        else
        {
            services.AddAWSService<IAmazonS3>();
            services.AddSingleton<IBucketStorage, S3BucketStorage>();            
        }
    }
}