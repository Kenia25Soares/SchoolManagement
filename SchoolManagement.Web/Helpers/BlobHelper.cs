using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.WindowsAzure.Storage;

namespace SchoolManagement.Web.Helpers
{
    public class BlobHelper : IBlobHelper
    {
        private readonly BlobServiceClient _blobClient;

        // Define o limite de tamanho das image 2MB
        private const long MaxFileSize = 2 * 1024 * 1024; 

        public BlobHelper(IConfiguration configuration)
        {
            string keys = configuration["Blob:ConnectionString"];
            _blobClient = new BlobServiceClient(keys);
        }

        public async Task<Guid> UploadBlobAsync(IFormFile file, string containerName)
        {
            if (file.Length > MaxFileSize)
            {
                throw new InvalidOperationException($"The file exceeds the limit {MaxFileSize / 1024 / 1024} MB.");
            }

            var containerClient = _blobClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            Guid name = Guid.NewGuid();
            var blobClient = containerClient.GetBlobClient(name.ToString());

            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, overwrite: true);

            return name;
        }
    }
}
