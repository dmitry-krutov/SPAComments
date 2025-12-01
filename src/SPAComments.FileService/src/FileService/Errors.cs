using SPAComments.SharedKernel;

namespace FileService;

public static class Errors
{
    public static class Files
    {
        public static Error FailUpload() =>
            Error.Failure("file.upload", "Fail to upload file");

        public static Error FailRemove() =>
            Error.Failure("file.remove", "Fail to remove file");

        public static Error FailGetPresignedUrl() =>
            Error.Failure("file.presigned", "Fail to get presigned url for file(s)");
    }
}