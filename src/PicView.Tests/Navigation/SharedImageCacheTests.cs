using PicView.Core.Models;
using PicView.Core.Navigation;

namespace PicView.Tests.Navigation;

public class SharedImageCacheTests
{
    private readonly SharedImageCache _cache;
    private readonly Func<FileInfo, ValueTask<ImageModel>> _mockLoader;

    public SharedImageCacheTests()
    {
        _mockLoader = f => new ValueTask<ImageModel>(new ImageModel { FileInfo = f });
        _cache = new SharedImageCache(_mockLoader, maxItems: 3);
    }

    [Fact]
    public async Task GetOrLoadAsync_ShouldLoadImage()
    {
        var file = new FileInfo("test.jpg");
        var model = await _cache.GetOrLoadAsync(file);

        Assert.NotNull(model);
        Assert.Equal(file.FullName, model.FileInfo.FullName);
        Assert.True(_cache.TryGet(file, out _));
    }

    [Fact]
    public void UpdatePriorities_ShouldTriggerLoad_AndEvict()
    {
        var owner1 = new object();
        var files = new List<string> { "1.jpg", "2.jpg", "3.jpg", "4.jpg" };

        // Cache size is 3. We request 4. 
        // 1, 2, 3 should be kept (indices 0, 1, 2).
        // 4 (index 3) is furthest, so if we strictly follow "keep low index", 1,2,3 are safer. 
        // Wait, maxItems is 3. 
        // Priority list: 1(0), 2(1), 3(2), 4(3).
        // 1, 2, 3 have lower scores. 4 has highest score (3).
        // So 4 should be evicted or never added?
        // UpdatePriorities triggers loads. So all 4 try to add.
        // Then Evict runs. 
        // Victim: Max Score.
        // Scores: 1->0, 2->1, 3->2, 4->3.
        // Victim is 4.
        
        _cache.UpdatePriorities(owner1, files);

        // Allow async loads to complete (simulated delay not needed but let's wait a bit)
        Thread.Sleep(100);

        // 1, 2, 3 should be in cache
        Assert.True(_cache.TryGet(new FileInfo("1.jpg"), out _));
        Assert.True(_cache.TryGet(new FileInfo("2.jpg"), out _));
        Assert.True(_cache.TryGet(new FileInfo("3.jpg"), out _));
        
        // 4 should be evicted
        Assert.False(_cache.TryGet(new FileInfo("4.jpg"), out _));
    }

    [Fact]
    public void MultiOwner_Eviction_ShouldKeepClosestToAnyOwner()
    {
        // Cache size 3.
        // Owner A: [1, 2, 3, 4, 5] (Focus 1) -> Wants 1, 2, 3
        // Owner B: [5, 4, 3, 2, 1] (Focus 5) -> Wants 5, 4, 3
        
        // Priorities A: 1:0, 2:1, 3:2, 4:3, 5:4
        // Priorities B: 5:0, 4:1, 3:2, 2:3, 1:4
        
        // Combined Scores (Min):
        // 1: Min(0, 4) = 0
        // 2: Min(1, 3) = 1
        // 3: Min(2, 2) = 2
        // 4: Min(3, 1) = 1
        // 5: Min(4, 0) = 0
        
        // Scores: 1(0), 2(1), 3(2), 4(1), 5(0).
        // Sorted: 1(0), 5(0), 2(1), 4(1), 3(2).
        // Victim should be 3 (Score 2).
        // Kept: 1, 5, 2 (or 4).
        
        var ownerA = new object();
        var ownerB = new object();
        
        // We add them one by one to simulate
        _cache.UpdatePriorities(ownerA, new[] { "1.jpg", "2.jpg", "3.jpg", "4.jpg", "5.jpg" });
        _cache.UpdatePriorities(ownerB, new[] { "5.jpg", "4.jpg", "3.jpg", "2.jpg", "1.jpg" });
        
        Thread.Sleep(100);

        // 1 and 5 MUST be there (Score 0)
        Assert.True(_cache.TryGet(new FileInfo("1.jpg"), out _), "1 should be kept");
        Assert.True(_cache.TryGet(new FileInfo("5.jpg"), out _), "5 should be kept");
        
        // 3 is worst (Score 2). It should be gone.
        Assert.False(_cache.TryGet(new FileInfo("3.jpg"), out _), "3 should be evicted");
        
        // 2 and 4 have Score 1. One of them should be kept.
        var has2 = _cache.TryGet(new FileInfo("2.jpg"), out _);
        var has4 = _cache.TryGet(new FileInfo("4.jpg"), out _);
        
        Assert.True(has2 || has4, "Either 2 or 4 should be kept");
    }
}
