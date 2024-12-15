using Nsu.HackathonProblem.Contracts;
using Xunit;
using Xunit.Abstractions;

namespace Nsu.HackathonProblem.KhudorozhkovStrategy.Tests;

public class PerfomanceTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public PerfomanceTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void HarmonicMeanSatisfactionTest()
    {
        int teamLeadsCount = 10;
        int juniorsCount = 10;
        int runs = 10000; // Количество прогонов

        var strategy = new KhudorozhkovStrategy();

        double totalHarmonicMeans = 0;

        for (int i = 0; i < runs; i++)
        {
            var (teamLeads, juniors, teamLeadsWishlists, juniorsWishlists) = GenerateData(teamLeadsCount, juniorsCount);

            var teams = strategy.BuildTeams(teamLeads, juniors, teamLeadsWishlists, juniorsWishlists).ToList();

            var satisfactions = teams.Select(t => ComputeTeamSatisfaction(t, teamLeadsWishlists, juniorsWishlists))
                .ToList();

            double harmonicMean = ComputeHarmonicMean(satisfactions);
            totalHarmonicMeans += harmonicMean;

            if (i % 100 == 0)
                _testOutputHelper.WriteLine(
                    $"[{i}] Current Harmonic Mean: {harmonicMean}. Total Harmonic Mean: {totalHarmonicMeans / (i + 1)}");
        }

        double averageHarmonicMean = totalHarmonicMeans / runs;
        _testOutputHelper.WriteLine($"Average Harmonic Mean of team satisfactions: {averageHarmonicMean}");
    }

    private (Employee[] teamLeads, Employee[] juniors, Wishlist[] teamLeadsWishlists, Wishlist[] juniorsWishlists)
        GenerateData(int teamLeadsCount, int juniorsCount)
    {
        var rnd = new Random();

        var teamLeads = new Employee[teamLeadsCount];
        var juniors = new Employee[juniorsCount];

        for (int i = 0; i < teamLeadsCount; i++)
        {
            teamLeads[i] = new Employee(i + 1, $"TeamLead_{i + 1}");
        }

        for (int j = 0; j < juniorsCount; j++)
        {
            juniors[j] = new Employee(teamLeadsCount + j + 1, $"Junior_{j + 1}");
        }

        var teamLeadsWishlists = teamLeads.Select(tl =>
        {
            var desired = juniors.Select(j => j.Id).OrderBy(x => rnd.Next()).ToArray();
            return new Wishlist(tl.Id, desired);
        }).ToArray();

        var juniorsWishlists = juniors.Select(jr =>
        {
            var desired = teamLeads.Select(tl => tl.Id).OrderBy(x => rnd.Next()).ToArray();
            return new Wishlist(jr.Id, desired);
        }).ToArray();

        return (teamLeads, juniors, teamLeadsWishlists, juniorsWishlists);
    }

    private double ComputeTeamSatisfaction(Team team, Wishlist[] teamLeadsWishlists, Wishlist[] juniorsWishlists)
    {
        int teamLeadSatisfaction = ComputeParticipantSatisfaction(team.TeamLead.Id, team.Junior.Id, teamLeadsWishlists);
        int juniorSatisfaction = ComputeParticipantSatisfaction(team.Junior.Id, team.TeamLead.Id, juniorsWishlists);

        return teamLeadSatisfaction + juniorSatisfaction;
    }

    private int ComputeParticipantSatisfaction(int participantId, int partnerId, Wishlist[] allWishlists)
    {
        var w = allWishlists.First(x => x.EmployeeId == participantId).DesiredEmployees;
        int index = Array.IndexOf(w, partnerId);
        if (index == -1)
            return 0;
        return w.Length - index;
    }

    private double ComputeHarmonicMean(List<double> values)
    {
        if (values.Count == 0)
            return 0;

        double sumInverse = 0;
        foreach (var v in values)
        {
            if (v <= 0) return 0;
            sumInverse += 1.0 / v;
        }

        return values.Count / sumInverse;
    }
}