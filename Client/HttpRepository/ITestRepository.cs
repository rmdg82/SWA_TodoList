using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.HttpRepository;

public interface ITestRepository
{
    Task<string?> GetHelloWorld();
}