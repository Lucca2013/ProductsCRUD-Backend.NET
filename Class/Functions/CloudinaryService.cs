using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

public class CloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(string cloudinaryUrl)
    {
        _cloudinary = new Cloudinary(cloudinaryUrl);
    }

    public async Task<string> UploadImageAsync(byte[] imageBytes, string id)
    {
        using var stream = new MemoryStream(imageBytes);
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(id, stream),
            Folder = "products"
        };

        var result = await _cloudinary.UploadAsync(uploadParams);
        return result.SecureUrl.ToString();
    }
}
