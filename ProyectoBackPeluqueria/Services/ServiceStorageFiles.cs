using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;

namespace ProyectoBackPeluqueria.Services
{
    public class ServiceStorageFiles
    {
        //TODO SERVICIO STORAGE SIEMPRE UTILIZA CLIENTS 
        //PARA TRABAJAR 
        //UN CLIENT PUEDE SER DIRECTAMENTE SOBRE UN SHARED O  
        //PODRIA SER SOBRE TODO EL SERVICIO DE FILES 
        private ShareDirectoryClient root;

        public ServiceStorageFiles(IConfiguration configuration)
        {
            string keys = configuration.GetValue<string>
            ("AzureKeys:StorageAccount");
            //NUESTRO CLIENTE TRABAJARA SOBRE UN SHARED  
            //DETERMINADO 
            ShareClient client =
    new ShareClient(keys, "peluqueria");
            this.root = client.GetRootDirectoryClient();
        }

        //METODO PARA RECUPERAR TODOS LOS FICHEROS 
        //DE LA RAIZ DE SHARED 
        public async Task<List<string>>
    GetFilesAsync()
        {
            List<string> files =
            new List<string>();
            await foreach (ShareFileItem item in
            this.root.GetFilesAndDirectoriesAsync())
            {
                files.Add(item.Name);
            }
            return files;
        }

        public async Task<string> ReadFileAsync(string fileName)
        {
            ShareFileClient fileClient =
            this.root.GetFileClient(fileName);
            ShareFileDownloadInfo data =
            await fileClient.DownloadAsync();
            Stream stream = data.Content;
            string contenido = "";
            using (StreamReader reader = new StreamReader(stream))
            {
                contenido = await reader.ReadToEndAsync();
            }
            return contenido;
        }

        public async Task UploadFileAsync
        (string fileName, Stream stream)
        {
            ShareFileClient fileClient =
            this.root.GetFileClient(fileName);
            await fileClient.CreateAsync(stream.Length);
            await fileClient.UploadAsync(stream);
        }

        public async Task DeleteFileAsync(string fileName)
        {
            ShareFileClient fileClient =
            this.root.GetFileClient(fileName);
            await fileClient.DeleteAsync();
        }
    }
}
