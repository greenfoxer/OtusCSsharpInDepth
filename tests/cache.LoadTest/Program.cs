using cache.SimpleStoreSystem;
using NBomber.Contracts;
using NBomber.CSharp;

namespace cache.LoadTest
{
    internal class Program
    {
        private static async Task<Response<object>> SetStep(int id, IScenarioContext context)
        {
            using var client = new SimpleTestClient("127.0.0.1", 55555);
            var userProfileToSend = new UserProfile() { Id = id, CreatedAt = DateTime.Now, UserName = $"Test user {id}" };
            await client.ConnectAsync();
            var result = await client.SetAsync($"user:{id}", userProfileToSend);
            
            return result.IndexOf("SUCCESSFULY ADDED AS JSON USER PROFILE") > -1 ? Response.Ok() : Response.Fail();
        }
        private static async Task<Response<object>> GetStep(int id, IScenarioContext context)
        {
            using var client = new SimpleTestClient("127.0.0.1", 55555);
            await client.ConnectAsync();
            var result = await client.GetAsync($"user:{id}");
            if (result == null)
                return Response.Fail();
            else
                return result.Id == id ? Response.Ok() : Response.Fail();
        }
        static void Main(string[] args)
        {
            var id = 0;
            var loadScenario = Scenario.Create("load_test", async context =>
            {
                var currentId = Interlocked.Increment(ref id);
                var setStep = await Step.Run("set_step", context, () => SetStep(currentId, context));
                var getStep = await Step.Run("get_step", context, () => GetStep(currentId, context));

                return getStep;
            })
                .WithWarmUpDuration(TimeSpan.FromSeconds(10))
                .WithLoadSimulations(Simulation.Inject(100, TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(30)));


            NBomberRunner.RegisterScenarios(loadScenario).Run();

            Console.WriteLine("Press any key...");
            Console.ReadLine();
        }
    }
}
