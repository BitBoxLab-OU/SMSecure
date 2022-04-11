﻿namespace Telegraph.Services
{
    public interface IImageCompressionService
    {
          byte[] CompressImage(byte[] imageData, int compressionPercentage, string destinationPath = null);
    }
}
