namespace SchoolManagement.Web.Helpers
{
    public interface IBlobHelper
    {
        Task<Guid> UploadBlobAsync(IFormFile file, string containerName); //Usa o ficheiro que vem da web, de um formulario, azure blob

    }
}
