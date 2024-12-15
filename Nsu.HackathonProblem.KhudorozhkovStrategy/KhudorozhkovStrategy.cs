using Nsu.HackathonProblem.Contracts;

namespace Nsu.HackathonProblem.KhudorozhkovStrategy;

public class KhudorozhkovStrategy : ITeamBuildingStrategy
{
    private readonly Random _rnd = new();

    private const int PopulationSize = 400; // Размер популяции
    private const int Generations = 20; // Количество поколений
    private const double MutationRate = 0.2; // Вероятность мутации
    private const double CrossoverRate = 0.9; // Вероятность кроссовера

    public IEnumerable<Team> BuildTeams(IEnumerable<Employee> teamLeads, IEnumerable<Employee> juniors,
        IEnumerable<Wishlist> teamLeadsWishlists, IEnumerable<Wishlist> juniorsWishlists)
    {
        var leaders = teamLeads.ToList();
        var jr = juniors.ToList();

        var leaderPrefs = teamLeadsWishlists.ToDictionary(w => w.EmployeeId, w => w.DesiredEmployees);
        var juniorPrefs = juniorsWishlists.ToDictionary(w => w.EmployeeId, w => w.DesiredEmployees);

        // Генерируем начальную популяцию
        var population = new List<int[]>();
        // В качестве первого решения берем результат венгерского алгоритма (хорошее решение, которое позволяет быстрее найти подходящий максимум)
        var hungarianSolution = GetHungarianAssignments(leaders, jr, leaderPrefs, juniorPrefs);
        population.Add(hungarianSolution);

        int n = leaders.Count;

        for (int i = 1; i < PopulationSize; i++)
        {
            population.Add(GenerateRandomSolution(n));
        }

        for (int gen = 0; gen < Generations; gen++)
        {
            // Оцениваем гармоническое каждого решения (фитнес)
            var scored = population.Select(p => (p, Score(p, leaders, jr, leaderPrefs, juniorPrefs))).ToList();

            // Сортируем по убыванию фитнеса (чем выше — тем лучше)
            scored.Sort((a, b) => b.Item2.CompareTo(a.Item2));

            // Создаем новую популяцию, сохраняем в нем лучшее решение 
            var newPopulation = new List<int[]>();
            var best = scored.First();
            newPopulation.Add(best.p);

            // Устраиваем отбор турниром
            while (newPopulation.Count < PopulationSize)
            {
                var parent1 = TournamentSelect(scored);
                var parent2 = TournamentSelect(scored);

                int[] child1, child2;
                if (_rnd.NextDouble() < CrossoverRate)
                {
                    (child1, child2) = Crossover(parent1, parent2, n);
                }
                else
                {
                    child1 = (int[])parent1.Clone();
                    child2 = (int[])parent2.Clone();
                }

                Mutate(child1);
                Mutate(child2);

                newPopulation.Add(child1);
                if (newPopulation.Count < PopulationSize)
                    newPopulation.Add(child2);
            }

            population = newPopulation;
        }

        // После прогона всех поколений берем лучшее решение
        var finalScored = population.Select(p => (p, Score(p, leaders, jr, leaderPrefs, juniorPrefs))).ToList();
        finalScored.Sort((a, b) => b.Item2.CompareTo(a.Item2));

        var finalBest = finalScored.First().p;
        return CreateTeams(leaders, jr, finalBest);
    }

    private int[] GetHungarianAssignments(List<Employee> leaders, List<Employee> juniors,
        Dictionary<int, int[]> leaderPrefs, Dictionary<int, int[]> juniorPrefs)
    {
        int n = leaders.Count;
        int[,] cost = new int[n, n];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                int leaderId = leaders[i].Id;
                int juniorId = juniors[j].Id;

                int ls = CalcPref(leaderPrefs.GetValueOrDefault(leaderId, []), juniorId);
                int js = CalcPref(juniorPrefs.GetValueOrDefault(juniorId, []), leaderId);
                cost[i, j] = -(ls + js);
            }
        }

        var assignments = HungarianAlgorithm.HungarianAlgorithm.FindAssignments(cost);
        return assignments;
    }

    private int[] GenerateRandomSolution(int n)
    {
        var perm = Enumerable.Range(0, n).ToList();
        Shuffle(perm);
        return perm.ToArray();
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int k = _rnd.Next(i + 1);
            (list[i], list[k]) = (list[k], list[i]);
        }
    }

    private double Score(int[] solution, List<Employee> leaders, List<Employee> juniors,
        Dictionary<int, int[]> leaderPrefs, Dictionary<int, int[]> juniorPrefs)
    {
        var teams = CreateTeams(leaders, juniors, solution);
        return ComputeHarmonicMean(teams, leaderPrefs, juniorPrefs);
    }

    private double ComputeHarmonicMean(IEnumerable<Team> teams, Dictionary<int, int[]> leaderPrefs,
        Dictionary<int, int[]> juniorPrefs)
    {
        var satisfactions = teams.Select(t =>
        {
            int ls = CalcPref(leaderPrefs.GetValueOrDefault(t.TeamLead.Id, []), t.Junior.Id);
            int js = CalcPref(juniorPrefs.GetValueOrDefault(t.Junior.Id, []), t.TeamLead.Id);
            double s = ls + js;
            return s > 0 ? s : 0.0001;
        }).ToList();

        double sumInv = satisfactions.Sum(x => 1.0 / x);
        return satisfactions.Count / sumInv;
    }

    private int CalcPref(int[] arr, int id)
    {
        int idx = Array.IndexOf(arr, id);
        return idx == -1 ? 0 : arr.Length - idx;
    }

    private (int[], int[]) Crossover(int[] parent1, int[] parent2, int n)
    {
        int point = _rnd.Next(n);
        var child1 = new int[n];
        var child2 = new int[n];

        Array.Copy(parent1, 0, child1, 0, point);
        Array.Copy(parent2, point, child1, point, n - point);

        Array.Copy(parent2, 0, child2, 0, point);
        Array.Copy(parent1, point, child2, point, n - point);

        return (child1, child2);
    }

    private void Mutate(int[] solution)
    {
        if (_rnd.NextDouble() < MutationRate)
        {
            int n = solution.Length;
            int i = _rnd.Next(n);
            int j = _rnd.Next(n);
            (solution[i], solution[j]) = (solution[j], solution[i]);
        }
    }

    private int[] TournamentSelect(List<(int[] sol, double score)> population)
    {
        int tSize = 3;
        double bestScore = double.NegativeInfinity;
        int[] best = [];
        for (int i = 0; i < tSize; i++)
        {
            var p = population[_rnd.Next(population.Count)];
            if (p.score > bestScore)
            {
                bestScore = p.score;
                best = p.sol;
            }
        }

        return best;
    }

    private static List<Team> CreateTeams(List<Employee> leaders, List<Employee> juniors, int[] assignments)
    {
        var result = new List<Team>();
        for (int i = 0; i < assignments.Length; i++)
        {
            int j = assignments[i];
            if (j >= 0 && j < juniors.Count)
            {
                result.Add(new Team(leaders[i], juniors[j]));
            }
        }

        return result;
    }
}