using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Azure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Azure.Storage.Sas;
using Azure.Storage;

namespace LearniVerseNew.Models.Helpers
{
    public class BlobHelper
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName = "classroom-file-container";

        public BlobHelper()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["BlobConnectionString"].ConnectionString; //change this in prod
            _blobServiceClient = new BlobServiceClient(connectionString);

        }
        public async Task CreateContainerAsync()
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

            if (!await blobContainerClient.ExistsAsync())
            {
                await blobContainerClient.CreateAsync();
                Console.WriteLine($"Container '{_containerName}' created successfully.");
            }
            else
            {
                Console.WriteLine($"Container '{_containerName}' already exists.");
            }
        }

        public IEnumerable<BlobItem> ListBlobs(string courseFolder)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobItems = new List<BlobItem>();

            foreach (BlobItem blobItem in containerClient.GetBlobs(prefix: courseFolder))
            {
                blobItems.Add(blobItem);
            }

            return blobItems;
        }

        public (bool Success, string Uri) UploadBlob(string fileName, Stream content)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

                if (!containerClient.Exists())
                {
                    containerClient.Create();
                }

                var blobClient = containerClient.GetBlobClient(fileName);
                blobClient.Upload(content, true);

                var uri = blobClient.Uri.ToString();

                return (true, uri);
            }
            catch (RequestFailedException ex)
            {
                return (false, $"Error uploading file: {ex.Message}");
            }
        }

        public Stream DownloadBlob(string fileName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient(fileName);

                if (!blobClient.Exists())
                {
                    throw new FileNotFoundException("Blob not found.");
                }

                // Create a memory stream to store the blob content
                var memoryStream = new MemoryStream();

                // Download the blob content to the memory stream
                blobClient.DownloadTo(memoryStream);

                // Reset the position of the memory stream to the beginning
                memoryStream.Seek(0, SeekOrigin.Begin);

                // Return the memory stream containing the blob content
                return memoryStream;
            }
            catch (RequestFailedException ex)
            {
                throw new Exception($"Error downloading blob: {ex.Message}");
            }
        }

        public bool DeleteBlob(string fileName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient(fileName);

                if (blobClient.Exists())
                {
                    blobClient.Delete();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (RequestFailedException ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error deleting blob: {ex.Message}");
                return false;
            }
        }

        public Uri GetBlobSasUri(string blobName)
        {
            // Get a reference to the blob container
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

            // Get a reference to the blob
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            // Create the SAS token parameters
            BlobSasBuilder sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _containerName,
                BlobName = blobName,
                Resource = "b", // "b" indicates the SAS applies to the blob resource
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1), // Expiry time for the SAS token
                                                               // Set permissions for the SAS token
                StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5), // Start time for the SAS token
                Protocol = SasProtocol.Https // HTTPS should be used for security reasons
            };

            // Set permissions for the SAS token
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            // Generate the SAS token
            string sasToken = sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential("learniversestorage", "qgf9TcU5BnZaxPSkIyFsOyBznjo1/cM+k6xjdCW1v0A7isd/BB9VibIrrEP0YR5q9EKogIwOPV+D+AStCnCHnA==")).ToString(); //change in prod

            // Construct the full URI with SAS token
            UriBuilder uriBuilder = new UriBuilder(blobClient.Uri)
            {
                Query = sasToken
            };

            return uriBuilder.Uri;
        }

    }
}