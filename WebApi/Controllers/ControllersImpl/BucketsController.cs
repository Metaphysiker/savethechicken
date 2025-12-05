using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
[Route("api/buckets")]
[ApiController]
public class BucketsController : ControllerBase
{
    private readonly IAmazonS3 _s3Client;
    public BucketsController(IAmazonS3 s3Client)
    {
        _s3Client = s3Client;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateBucketAsync(string bucketName)
    {
        var bucketExists = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(s3Client: _s3Client, bucketName: bucketName);
        if (bucketExists) return BadRequest($"Bucket {bucketName} already exists.");
        await _s3Client.PutBucketAsync(bucketName);
        return Ok($"Bucket {bucketName} created.");
    }

    [HttpGet("createDefaultBucket")]
    public async Task<IActionResult> CreateDefaultBucket()
    {
        var bucketName = Environment.GetEnvironmentVariable("AWS_BUCKET_NAME");
        if (string.IsNullOrEmpty(bucketName)) return BadRequest("AWS_BUCKET_NAME environment variable not set.");

        var bucketExists = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(s3Client: _s3Client, bucketName: bucketName);
        if (bucketExists) return BadRequest($"Bucket {bucketName} already exists.");
        await _s3Client.PutBucketAsync(bucketName);
        return Ok($"Bucket {bucketName} created.");
    }

    [HttpGet("get-all")]
    public async Task<IActionResult> GetAllBucketAsync()
    {
        var data = await _s3Client.ListBucketsAsync();
        var buckets = data.Buckets.Select(b => { return b.BucketName; });
        return Ok(buckets);
    }
}
