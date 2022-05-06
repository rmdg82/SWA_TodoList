namespace Client.HttpRepository.Interfaces;

public interface ITestRepository
{
    Task<string?> GetTest();
}