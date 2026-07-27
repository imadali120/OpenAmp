using OpenAmp.Application.Recommendations;

namespace OpenAmp.Infrastructure.Tests.Recommendations;

public sealed class RecommendationEngineTests
{
    [Fact]
    public void CosineSimilarityReturnsOneForIdenticalVectors()
    {
        var result = RecommendationEngine.CosineSimilarity([1, 0.5, 0], [1, 0.5, 0]);

        Assert.Equal(1, result, 8);
    }

    [Fact]
    public void CosineSimilarityReturnsZeroForOrthogonalVectors()
    {
        var result = RecommendationEngine.CosineSimilarity([1, 0], [0, 1]);

        Assert.Equal(0, result, 8);
    }

    [Fact]
    public void CosineSimilarityThrowsForDifferentDimensions()
    {
        Assert.Throws<ArgumentException>(
            () => RecommendationEngine.CosineSimilarity([1], [1, 2]));
    }

    [Fact]
    public void DynamicAlphaDecreasesAsHistoryGrows()
    {
        var coldStart = RecommendationEngine.DynamicAlpha(0);
        var establishedBand = RecommendationEngine.DynamicAlpha(20);

        Assert.Equal(0.9, coldStart, 8);
        Assert.InRange(establishedBand, 0.35, coldStart);
    }

    [Fact]
    public void HybridScoreAppliesRequestedWeight()
    {
        var result = RecommendationEngine.HybridScore(0.8, 0.4, 0.75);

        Assert.Equal(0.7, result, 8);
    }

    [Fact]
    public void CollaborativeScoreUsesSimilarHallPreferences()
    {
        RecommendationInteraction[] interactions =
        [
            new(1, 1, 0.9), new(1, 2, 0.8), new(1, 3, 0.1),
            new(2, 1, 0.8), new(2, 2, 0.7), new(2, 3, 0.1),
            new(3, 1, 0.4), new(3, 2, 0.3), new(3, 3, 0.9),
            new(4, 1, 0.9)
        ];

        var result = RecommendationEngine.CollaborativeScore(4, 2, interactions);

        Assert.InRange(result, 0.89, 0.91);
    }

    [Fact]
    public void RankPrioritizesContentMatchForColdStart()
    {
        RecommendationCandidate[] halls =
        [
            new(10, [1, 0, 1]),
            new(20, [0, 1, 0])
        ];

        var result = RecommendationEngine.Rank(99, [1, 0, 1], halls, [], 2);

        Assert.Equal(10, result[0].SalaId);
        Assert.Equal(0.9, result[0].Alpha, 8);
        Assert.True(result[0].Score > result[1].Score);
    }
}
