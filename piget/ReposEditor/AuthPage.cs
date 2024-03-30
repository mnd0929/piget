using piget.Api;
using System;
using Octokit;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;

namespace piget.ReposEditor
{
    public class AuthPage
    {
        public Credentials GetCredentials()
        {
            Console.Clear();
            Helpers.Logs.Log("<?> ", "Выполняется подготовка к входу с помощью персонального токена (GitHub PAT) \r\n");

            Console.WriteLine("--- Авторизация в GitHub \r\n" +
                          "Требуемые права вашего PAT: repo, write:packages, delete:packages, delete_repo, codespace");

            while (true) 
            {
                try
                {
                    Console.Write("GitHub PAT: ");
                    string pat = Console.ReadLine();

                    Credentials credentials = new Credentials(pat);
                    CheckCredentials(credentials);

                    return credentials;
                }
                catch (Exception ex)
                {
                    Helpers.Logs.LogError("Не удалось войти в GitHub", ex);
                }
            }
        }

        private void CheckCredentials(Credentials credentials)
        {
            var client = new GitHubClient(new ProductHeaderValue("PigetConsoleRemoteLibraryManagerAuthPage"));
            client.Credentials = credentials;

            _ = client.Repository.GetAllForCurrent().Result;
        }
    }
}
