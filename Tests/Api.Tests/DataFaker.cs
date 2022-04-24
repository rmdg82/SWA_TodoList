using Api.Models;
using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Tests;

public class DataFaker
{
    //public List<Todo> Todos { get; init; }
    public List<User> Users { get; init; }

    public DataFaker(int count)
    {
        Init(count);
    }

    public static void Init(int count)
    {
        //var todoFaker = new Faker<Todo>()
        //    .RuleFor(p => p.Id, f => Guid.NewGuid().ToString())
        //    .RuleFor(p => p.Text, f => f.Lorem.Sentence(15, 5))
        //    .RuleFor(p => p.IsCompleted, f => f.Random.Bool())
        //    .RuleFor(p => p.CreatedAt, f => f.Date.Past(1, DateTime.Now.AddDays(-1)))
        //    .RuleFor(p => p.CompletedAt, f => f.Date.Between(DateTime.Now.AddDays(-1), DateTime.Now));

        //string userId = Guid.NewGuid().ToString();

        ////var userFaker = new Faker<User>()
        ////    .RuleFor(p => p.Id, f => userId)
        ////    .RuleFor(p => p.ClientPrincipal.UserId, f => userId)
        ////    .RuleFor(p => p.ClientPrincipal.IdentityProvider, f => "identity-provider");
        //var userFaker = new Faker<User>()
        //    .RuleFor(p => p.ClientPrincipal.UserDetails, f => f.Internet.UserName())
        //    .RuleFor(p => p.ClientPrincipal.UserId, f => userId)
        //    .RuleFor(p => p.ClientPrincipal.IdentityProvider, f => "identity-provider")
        //    .RuleFor(p => p.Id, f => userId);
    }
}