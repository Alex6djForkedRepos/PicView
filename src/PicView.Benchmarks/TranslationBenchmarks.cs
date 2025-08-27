using BenchmarkDotNet.Attributes;
using PicView.Core.Localization;

namespace PicView.Benchmarks;

[MemoryDiagnoser]
public class TranslationBenchmarks
{
    
    [GlobalSetup]
    public void Setup()
    {
        SetDefaults();
    }
    
    [Benchmark]
    public async ValueTask LoadLanguages()
    {
        await TranslationManager.DetermineAndLoadLanguage();
    }
}