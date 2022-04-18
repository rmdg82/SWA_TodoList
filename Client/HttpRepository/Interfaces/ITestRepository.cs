using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.HttpRepository.Interfaces;

public interface ITestRepository
{
    Task<string?> GetTest();
}