using Octokit;
using piget.Api;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using ProductHeaderValue = Octokit.ProductHeaderValue;

namespace piget.ReposEditor
{
    public class Editor
    {
        private GitHubClient gitHubClient = null;

        List<Repository> repositories = null;

        Repository currentRepository = null;

        RepositoryContent currentLibraryContent = null;

        public void Initialize()
        {
            gitHubClient = new GitHubClient(new ProductHeaderValue("PigetConsoleRemoteLibraryManager"));
            gitHubClient.Credentials = new AuthPage().GetCredentials();

            Helpers.Logs.Log("\r\n<!> ", $"Инициализация аккаунта {gitHubClient.User.Current().Result.Login}");
            Helpers.Logs.Log("<!> ", $"Поиск библиотек в репозиториях");

            SearchLibraryRepositories();

            Console.Clear();

            SelectRepository();
            SelectLibrary();
        }

        public void SelectRepository()
        {
            ConsoleMenu menu = new ConsoleMenu
            {
                HideMenuAfterSuccessfulSelection = true
            };

            repositories.ForEach(rep => 
                menu.AnswerOptions.Add(rep.Name + string.Concat(Enumerable.Repeat(" ", Console.WindowWidth - rep.Name.Length - 1))));

            menu.AnswerOptions.Add("[+] Создать репозиторий");

            int answerIndex = menu.GetAnswer();
            if (answerIndex == menu.AnswerOptions.Count - 1)
            {
                CreateLibrary();
                return;
            }

            currentRepository = repositories[answerIndex];
        }

        public void SelectLibrary()
        {
            ConsoleMenu menu = new ConsoleMenu
            {
                HideMenuAfterSuccessfulSelection = true
            };

            repositories.ForEach(rep =>
                menu.AnswerOptions.Add(rep.Name + string.Concat(Enumerable.Repeat(" ", Console.WindowWidth - rep.Name.Length - 1))));

            menu.AnswerOptions.Add("[+] Создать репозиторий");

            int answerIndex = menu.GetAnswer();
            if (answerIndex == menu.AnswerOptions.Count - 1)
            {
                CreateLibrary();
                return;
            }

            currentRepository = repositories[answerIndex];
        }

        public void CreateLibrary()
        {
            Helpers.Logs.Log("<!> ", $"TODO: Создание библиотеки");
        }

        private void SearchLibraryRepositories()
        {
            List<Repository> pigetRemoteScriptLibrariesRepositories = new List<Repository>();
            IReadOnlyList<Repository> repositories = gitHubClient.Repository.GetAllForCurrent().Result;
            for (int i = 0; i < repositories.Count; i++)
            {
                Repository repository = repositories[i];
                
                Helpers.ProgressIndicator.WriteProgressLine(Helpers.ProgressIndicator.GetPercent(repositories.Count, i), 20);

                try
                {
                    IReadOnlyList<RepositoryContent> contents = gitHubClient.Repository.Content.GetAllContents(repository.Id).Result;
                    foreach (RepositoryContent content in contents)
                    {
                        if (Path.GetExtension(content.Name) == ".pgtlb")
                        {
                            pigetRemoteScriptLibrariesRepositories.Add(repository);
                            break;
                        }
                    }
                }
                catch { }
            }

            this.repositories = pigetRemoteScriptLibrariesRepositories;
        }

        private void SearchLibrariesInRepository()
        {
            
        }
    }
}
