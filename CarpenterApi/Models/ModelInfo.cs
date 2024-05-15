using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CarpenterApi.Models
{
    internal class ModelInfo
    {
        public Guid id;
        public string userId;
        public string model;
        public int maxInputLength;
        public int maxOutputLength;
        public double rating;

        public static async Task DecrementModelInfo(CosmosClient client, CarpenterUser user, string model)
        {
            ModelInfo modelInfo = await GetModelInfo(client, user, model);

            if (modelInfo != null)
            {
                modelInfo.rating--;
                await modelInfo.Update(client);
            }
            else
            {
                modelInfo = new()
                {
                    id = Guid.NewGuid(),
                    userId = user.userId,
                    model = model,
                    rating = 999
                };

                await modelInfo.Write(client);
            }
        }

        public static async Task<ModelInfo> GetModelInfo(CosmosClient client, CarpenterUser user, string model)
        {
            Container container = client.GetDatabase("carpenter-dev").GetContainer("model-infos");
            QueryDefinition queryDefinition = new QueryDefinition("SELECT TOP 1 * FROM c WHERE c.userId = @userId AND c.model = @model")
                .WithParameter("@userId", user.userId)
                .WithParameter("@model", model);

            using var iterator = container.GetItemQueryIterator<ModelInfo>(queryDefinition);
            while (iterator.HasMoreResults)
            {
                var modelInfo = (await iterator.ReadNextAsync()).FirstOrDefault();

                if (modelInfo != null)
                {
                    return modelInfo;
                }
            }

            return null;
        }

        public static async Task<IList<ModelInfo>> GetModelInfos(CosmosClient client, CarpenterUser user)
        {
            Container container = client.GetDatabase("carpenter-dev").GetContainer("model-infos");
            QueryDefinition queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.userId = @userId")
                .WithParameter("@userId", user.userId);

            List<ModelInfo> modelInfos = new();

            using (var iterator = container.GetItemQueryIterator<ModelInfo>(queryDefinition))
            {
                while (iterator.HasMoreResults)
                {
                    var readNext = await iterator.ReadNextAsync();

                    foreach (var modelInfo in readNext)
                    {
                        modelInfos.Add(modelInfo);
                    }
                }
            }

            return modelInfos;
        }

        public async Task Write(CosmosClient client)
        {
            Container container = client.GetDatabase("carpenter-dev").GetContainer("model-infos");
            await container.CreateItemAsync(this);
        }

        public async Task Update(CosmosClient client)
        {
            Container container = client.GetDatabase("carpenter-dev").GetContainer("model-infos");
            await container.ReplaceItemAsync(this, id.ToString());
        }

        public static async Task<(string[], string[])> PickModels(CosmosClient client, CarpenterUser user, int maxInputLength, int maxOutputLength)
        {
            HttpClient httpClient = new();
            StableHordeApi.Client apiClient = new(httpClient)
            {
                BaseUrl = "https://aihorde.net/api"
            };

            var workers = await apiClient.Get_workersAsync(MessageGeneration.ApiKey, null, "text", null);
            var modelInfos = await GetModelInfos(client, user);

            Dictionary<string, double> modelRatings = new();

            foreach(var worker in workers)
            {
                if(worker.Max_context_length >= maxInputLength && worker.Max_length >= maxOutputLength)
                {
                    foreach (var workerModel in worker.Models)
                    {
                        if (!modelRatings.ContainsKey(workerModel))
                        {
                            ModelInfo modelInfo = (from m in modelInfos where m.model == workerModel && m.maxInputLength == maxInputLength && m.maxOutputLength == maxOutputLength select m).SingleOrDefault();

                            if (modelInfo == null)
                            {
                                modelInfo = new()
                                {
                                    id = Guid.NewGuid(),
                                    userId = user.userId,
                                    model = workerModel,
                                    maxInputLength = maxInputLength,
                                    maxOutputLength = maxOutputLength,
                                    rating = 1000
                                };

                                await modelInfo.Write(client);
                            }

                            modelRatings.Add(workerModel, modelInfo.rating);
                        }
                    }
                }
            }

            int modelCount = modelRatings.Count / 4;
            modelCount = Math.Max(modelCount, 1);

            List<string> models1 = new();
            List<string> models2 = new();

            for (int i = 0; i < modelCount; i++)
            {
                string model1 = GetRandomModel(modelRatings);
                models1.Add(model1);
                modelRatings.Remove(model1);

                string model2 = GetRandomModel(modelRatings);
                models2.Add(model2);
                modelRatings.Remove(model2);
            }

            return (models1.ToArray(), models2.ToArray());
        }

        public static string GetRandomModel(Dictionary<string, double> modelRatings)
        {
            double total = 0;
            double power = 2;

            foreach(var modelRating in modelRatings)
            {
                total += Math.Pow(modelRating.Value, power);
            }

            double rand = Random.Shared.NextDouble() * total;
            double counter = 0;

            foreach(var modelRating in modelRatings)
            {
                counter += Math.Pow(modelRating.Value, power);

                if(counter >= rand)
                {
                    return modelRating.Key;
                }
            }

            return modelRatings.Last().Key;
        }

        public static async Task CompareModels(CosmosClient client, CarpenterUser user, string winnerModel, string loserModel)
        {
            if (winnerModel != loserModel)
            {

                ModelInfo winnerInfo = await GetModelInfo(client, user, winnerModel);
                ModelInfo loserInfo = await GetModelInfo(client, user, loserModel);

                (double, double) ratings = UpdateRatings(winnerInfo.rating, loserInfo.rating);
                winnerInfo.rating = ratings.Item1;
                loserInfo.rating = ratings.Item2;

                await winnerInfo.Update(client);
                await loserInfo.Update(client);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ra">winner's rating</param>
        /// <param name="rb">loser's rating</param>
        /// <returns></returns>
        public static (double, double) UpdateRatings(double ra, double rb)
        {
            double k = 10;

            double qa = Math.Pow(10, ra / 400);
            double qb = Math.Pow(10, rb / 400);

            double ea = qa / (qa + qb);

            double delta = k * (1 - ea);

            double rap = ra + delta;
            double rbp = rb - delta;

            if(rbp < 0)
            {
                rbp = 0;
            }

            return (rap, rbp);
        }
    }
}
