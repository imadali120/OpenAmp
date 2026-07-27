namespace OpenAmp.Application.Recommendations;

public sealed record RecommendationInteraction(int BandId, int SalaId, double Preference);

public sealed record RecommendationCandidate(int SalaId, IReadOnlyList<double> Features);

public sealed record RecommendationScore(
    int SalaId,
    double ContentScore,
    double CollaborativeScore,
    double Alpha,
    double Score);

public sealed class RecommendationEngine
{
    private const double MinimumAlpha = 0.35;
    private const double MaximumAlpha = 0.90;
    private const double ColdStartDecay = 4.0;

    public static double CosineSimilarity(
        IReadOnlyList<double> left,
        IReadOnlyList<double> right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        if (left.Count != right.Count)
        {
            throw new ArgumentException("Vektori moraju imati isti broj elemenata.");
        }

        var dotProduct = 0d;
        var leftNorm = 0d;
        var rightNorm = 0d;
        for (var index = 0; index < left.Count; index++)
        {
            dotProduct += left[index] * right[index];
            leftNorm += left[index] * left[index];
            rightNorm += right[index] * right[index];
        }

        if (leftNorm <= double.Epsilon || rightNorm <= double.Epsilon)
        {
            return 0;
        }

        return Math.Clamp(dotProduct / Math.Sqrt(leftNorm * rightNorm), -1, 1);
    }

    public static double AdjustedCosineSimilarity(
        int firstHallId,
        int secondHallId,
        IReadOnlyCollection<RecommendationInteraction> interactions)
    {
        ArgumentNullException.ThrowIfNull(interactions);
        var matrix = BuildMatrix(interactions);
        return AdjustedCosineSimilarity(firstHallId, secondHallId, matrix);
    }

    public static double CollaborativeScore(
        int bandId,
        int candidateHallId,
        IReadOnlyCollection<RecommendationInteraction> interactions)
    {
        ArgumentNullException.ThrowIfNull(interactions);
        var matrix = BuildMatrix(interactions);
        return CollaborativeScore(bandId, candidateHallId, matrix);
    }

    public static double DynamicAlpha(int historyCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(historyCount);

        var alpha = MinimumAlpha
            + (MaximumAlpha - MinimumAlpha) * Math.Exp(-historyCount / ColdStartDecay);
        return Math.Clamp(alpha, MinimumAlpha, MaximumAlpha);
    }

    public static double HybridScore(
        double contentScore,
        double collaborativeScore,
        double alpha)
    {
        var boundedAlpha = Math.Clamp(alpha, 0, 1);
        var boundedContent = Math.Clamp(contentScore, 0, 1);
        var boundedCollaborative = Math.Clamp(collaborativeScore, 0, 1);
        return boundedAlpha * boundedContent
            + (1 - boundedAlpha) * boundedCollaborative;
    }

    public static IReadOnlyList<RecommendationScore> Rank(
        int bandId,
        IReadOnlyList<double> bandFeatures,
        IReadOnlyCollection<RecommendationCandidate> candidates,
        IReadOnlyCollection<RecommendationInteraction> interactions,
        int limit)
    {
        ArgumentNullException.ThrowIfNull(bandFeatures);
        ArgumentNullException.ThrowIfNull(candidates);
        ArgumentNullException.ThrowIfNull(interactions);
        if (limit <= 0)
        {
            return [];
        }

        var matrix = BuildMatrix(interactions);
        var historyCount = matrix.TryGetValue(bandId, out var bandHistory)
            ? bandHistory.Count
            : 0;
        var alpha = DynamicAlpha(historyCount);

        return candidates
            .Select(candidate =>
            {
                var content = Math.Clamp(
                    CosineSimilarity(bandFeatures, candidate.Features),
                    0,
                    1);
                var collaborative = CollaborativeScore(
                    bandId,
                    candidate.SalaId,
                    matrix);
                return new RecommendationScore(
                    candidate.SalaId,
                    content,
                    collaborative,
                    alpha,
                    HybridScore(content, collaborative, alpha));
            })
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.ContentScore)
            .ThenBy(x => x.SalaId)
            .Take(limit)
            .ToArray();
    }

    private static Dictionary<int, Dictionary<int, double>> BuildMatrix(
        IEnumerable<RecommendationInteraction> interactions) =>
        interactions
            .GroupBy(x => x.BandId)
            .ToDictionary(
                band => band.Key,
                band => band
                    .GroupBy(x => x.SalaId)
                    .ToDictionary(
                        hall => hall.Key,
                        hall => Math.Clamp(hall.Average(x => x.Preference), 0, 1)));

    private static double AdjustedCosineSimilarity(
        int firstHallId,
        int secondHallId,
        Dictionary<int, Dictionary<int, double>> matrix)
    {
        var numerator = 0d;
        var firstNorm = 0d;
        var secondNorm = 0d;
        var commonBands = 0;

        foreach (var bandRatings in matrix.Values)
        {
            if (!bandRatings.TryGetValue(firstHallId, out var firstRating)
                || !bandRatings.TryGetValue(secondHallId, out var secondRating))
            {
                continue;
            }

            var mean = bandRatings.Values.Average();
            var firstCentered = firstRating - mean;
            var secondCentered = secondRating - mean;
            numerator += firstCentered * secondCentered;
            firstNorm += firstCentered * firstCentered;
            secondNorm += secondCentered * secondCentered;
            commonBands++;
        }

        if (commonBands < 2
            || firstNorm <= double.Epsilon
            || secondNorm <= double.Epsilon)
        {
            return 0;
        }

        return Math.Clamp(
            numerator / Math.Sqrt(firstNorm * secondNorm),
            -1,
            1);
    }

    private static double CollaborativeScore(
        int bandId,
        int candidateHallId,
        Dictionary<int, Dictionary<int, double>> matrix)
    {
        if (!matrix.TryGetValue(bandId, out var history) || history.Count == 0)
        {
            return 0.5;
        }

        var weightedScore = 0d;
        var similarityWeight = 0d;
        foreach (var (visitedHallId, preference) in history)
        {
            if (visitedHallId == candidateHallId)
            {
                continue;
            }

            var similarity = AdjustedCosineSimilarity(
                visitedHallId,
                candidateHallId,
                matrix);
            if (similarity <= 0)
            {
                continue;
            }

            weightedScore += similarity * preference;
            similarityWeight += similarity;
        }

        return similarityWeight <= double.Epsilon
            ? 0.5
            : Math.Clamp(weightedScore / similarityWeight, 0, 1);
    }
}
