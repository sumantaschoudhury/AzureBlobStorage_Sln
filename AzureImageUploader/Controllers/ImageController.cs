using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AzureImageUploader.Models;
using System.Web;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Extensions.Configuration;

namespace AzureImageUploader.Controllers
{
    public class ImageController : Controller
    {
        private readonly ILogger<ImageController> _logger;
        private readonly IConfiguration _config;

        public ImageController(ILogger<ImageController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Upload(IFormFile file)
        {

            //Getting FileName
            var fileName = Path.GetFileName(file.FileName);

            //Assigning Unique Filename (Guid)
            var myUniqueFileName = Convert.ToString(Guid.NewGuid());

            //Getting file Extension
            var fileExtension = Path.GetExtension(fileName);

            // concatenating  FileName + FileExtension
            var newFileName = String.Concat(myUniqueFileName,Path.GetFileNameWithoutExtension(fileName), fileExtension);

            await UploadFileToBlobAsync(file, newFileName);

            // Combines two strings into a path.
            //var filepath =
            //    new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(),"Images")).Root + $@"\{newFileName}";
            //using (FileStream fs = System.IO.File.Create(filepath))
            //{
            //    files.CopyTo(fs);
            //    fs.Flush();
            //}
            return RedirectToAction("Index");
        }

        private async Task UploadFileToBlobAsync(IFormFile file, string fileName)
        {
            var fileMimeType = file.ContentType;
            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                var fileData = ms.ToArray();
               


                string accessKey = _config.GetConnectionString("accesskey");
                try
                {
                    var cloudStorageAccount = CloudStorageAccount.Parse(accessKey);
                    var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
                    var strContainerName = "sounakcontainer";
                    var cloudBlobContainer = cloudBlobClient.GetContainerReference(strContainerName);

                    if (await cloudBlobContainer.CreateIfNotExistsAsync())
                    {
                        await cloudBlobContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
                    }

                    if (fileName != null && fileData != null)
                    {
                        var cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);
                        cloudBlockBlob.Properties.ContentType = fileMimeType;
                        await cloudBlockBlob.UploadFromByteArrayAsync(fileData, 0, fileData.Length);
                    }
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
            }
        }
    }

}

