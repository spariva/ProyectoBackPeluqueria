using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ProyectoBackPeluqueria.Models;

namespace ProyectoBackPeluqueria.Services
{
    public class ServiceStorageBlobs
    {
        private BlobServiceClient _blobServiceClient;

        public ServiceStorageBlobs(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        public async Task DeleteBlobAsync(string containerName, string blobName)
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();
        }

        public async Task UploadBlobAsync(string containerName, string blobName, Stream contenido)
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(contenido, true);
        }
    }
}
