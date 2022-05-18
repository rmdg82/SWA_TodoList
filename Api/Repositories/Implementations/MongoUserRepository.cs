using Api.Models;
using Api.Repositories.Interfaces;
using MongoDB.Driver;

namespace Api.Repositories.Implementations;

public class MongoUserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _userCollection;

    public MongoUserRepository(MongoClient mongoClient, string databaseId, string collectionId)
    {
        if (string.IsNullOrEmpty(databaseId))
        {
            throw new ArgumentException($"'{nameof(databaseId)}' cannot be null or empty.", nameof(databaseId));
        }

        if (string.IsNullOrEmpty(collectionId))
        {
            throw new ArgumentException($"'{nameof(collectionId)}' cannot be null or empty.", nameof(collectionId));
        }

        var database = mongoClient.GetDatabase(databaseId);
        _userCollection = database.GetCollection<User>(collectionId);
    }

    public async Task<User?> CreateUser(ClientPrincipal clientPrincipal)
    {
        if (clientPrincipal is null)
        {
            throw new ArgumentNullException(nameof(clientPrincipal));
        }

        var isUserAlreadyPresent = await _userCollection.Find(u => u.Id == clientPrincipal.UserId).CountDocumentsAsync() != 0;

        if (!isUserAlreadyPresent)
        {
            var user = new User
            {
                Id = clientPrincipal.UserId,
                ClientPrincipal = clientPrincipal,
                Todos = new List<Todo>()
            };

            await _userCollection.InsertOneAsync(user);

            return user;
        }

        return null;
    }

    public async Task<User?> GetUser(string id)
    {
        return await _userCollection.Find(user => user.Id == id).FirstOrDefaultAsync();
    }
}