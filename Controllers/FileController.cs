using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using HAN.Demo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Net.Http.Headers;
using Azure.Core;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace HAN.Demo.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class FileController : Controller
    {
        IConfiguration _configuration;
        private readonly string STORAGE_CONNECTION_STRING;

        private readonly string CONTAINER_NAME = "handemo";

        private readonly ILogger<FileController> _logger;

        public FileController(ILogger<FileController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            STORAGE_CONNECTION_STRING = _configuration.GetConnectionString("Blobs");
        }
        
        public IActionResult Index(CancellationToken cancellationToken, int count = 5)
        {
            DateTime requestOn = DateTime.Now;

            List<FileDetails> files = new List<FileDetails>();            

            Stopwatch sw = new Stopwatch();
            sw.Start();

            for(var i = 0; i < count; i++)
            {
                FileDetails file = new FileDetails();

                using FileStream source = System.IO.File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), "LoremIpsum.txt"));
                                
                file.Title = Guid.NewGuid().ToString();
                file.UploadStarted = sw.Elapsed;

                UploadBlob(file.Title, source,cancellationToken).Wait();

                file.UploadEnded = sw.Elapsed;
                files.Add(file);

                TraceUpload(i, count, file);
            }

            sw.Stop();

            return View(new FilesModel()
            {
                Title = "Sequential and synchronous",
                Files = files,
                RequestedOn = requestOn,
                GeneratedIn = sw.Elapsed
            }) ;
        }

        public async Task<IActionResult> Asynchronous(CancellationToken cancellationToken, int count = 5)
        {
            DateTime requestOn = DateTime.Now;
            List<FileDetails> files = new List<FileDetails>();
            
            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (var i = 0; i < count; i++)
            {
                FileDetails file = new FileDetails();

                using FileStream source = System.IO.File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), "LoremIpsum.txt"));

                file.Title = Guid.NewGuid().ToString();
                file.UploadStarted = sw.Elapsed;

                await UploadBlob(file.Title, source, cancellationToken);

                file.UploadEnded = sw.Elapsed;
                files.Add(file);

                TraceUpload(i, count, file);
            }

            sw.Stop();

            return View("Index", new FilesModel()
            {
                Title = "Sequential and asynchronous",
                Files = files,
                RequestedOn = requestOn,
                GeneratedIn = sw.Elapsed
            });
        }

        public IActionResult Concurrent(CancellationToken cancellationToken, int count = 5)
        {
            DateTime requestOn = DateTime.Now;

            List<FileDetails> files = new List<FileDetails>();
            string directory = Directory.GetCurrentDirectory();

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Parallel.ForEach(Enumerable.Range(0, count), i =>
            {
                FileDetails file = new FileDetails();

                using FileStream source = System.IO.File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), "LoremIpsum.txt"));

                file.Title = Guid.NewGuid().ToString();
                file.UploadStarted = sw.Elapsed;

                UploadBlob(file.Title, source, cancellationToken).Wait();

                file.UploadEnded = sw.Elapsed;
                files.Add(file);
                
                TraceUpload(i, count, file);
            });
                        
            sw.Stop();

            return View("Index", new FilesModel()
            {
                Title = "In parallel",
                Files = files,
                RequestedOn = requestOn,
                GeneratedIn = sw.Elapsed
            });
        }

        private async Task UploadBlob(string fileName, Stream stream, CancellationToken cancellationToken)
        {
            // Configure an exponential backoff strategy, should a download be failing due to transient errors.
            BlobClientOptions options = new BlobClientOptions();
            options.Retry.Mode = RetryMode.Exponential;
            options.Retry.MaxRetries = 3;
            options.Retry.Delay = new TimeSpan(0, 0, 5);

            // Create a BlobServiceClient object which will be used to create a container client
            BlobServiceClient blobServiceClient = new BlobServiceClient(STORAGE_CONNECTION_STRING);

            // connect to the container
            BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(CONTAINER_NAME);
            await blobContainerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

            await blobContainerClient.UploadBlobAsync(fileName, stream, cancellationToken);
        }

        public async Task<IActionResult> Download(string file, CancellationToken cancellationToken)
        {
            try
            {
                // Configure an exponential backoff strategy, should a download be failing due to transient errors.
                BlobClientOptions options = new BlobClientOptions();
                options.Retry.Mode = RetryMode.Exponential;
                options.Retry.MaxRetries = 3;
                options.Retry.Delay = new TimeSpan(0, 0, 5);

                // Create a BlobServiceClient object which will be used to create a container client
                BlobServiceClient blobServiceClient = new BlobServiceClient(STORAGE_CONNECTION_STRING, options);

                // connect to the container
                BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(CONTAINER_NAME);

                // SKIP validation - Let this be an exception, as the 'check if exists' is an additional roundtrip
                //if (!await blobContainerClient.ExistsAsync())
                //{
                //    return NotFound(fileName);
                //}

                var blobClient = blobContainerClient.GetBlobClient(file);

                // SKIP validation - Let this be an exception, as the 'check if exists' is an additional roundtrip
                //if (!await blobClient.ExistsAsync())
                //{
                //    return NotFound(fileName);
                //}

                var download = await blobClient.DownloadAsync(cancellationToken);

                return new FileStreamResult(download.Value.Content, "text/plain");

            } catch (Exception e)
            {
                _logger.LogError(e, "Download");
                return NotFound();
            }            
        }


        private void TraceUpload(int index, int count, FileDetails file)
        {
            using (_logger.BeginScope(new Dictionary<string, object> {
                    { "Index", index },
                    { "Count", count },
                    { "UploadStart", file.UploadStarted },
                    { "UploadEnded", file.UploadEnded },
                    { "Duration", file.Elapsed } }))
            {
                _logger.LogInformation($"File {index+1} of {count}. Duration: {file.Elapsed}");
            }
        }
    }
}