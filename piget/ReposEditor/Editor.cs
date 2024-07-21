using Octokit;
using piget.Api;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using ConsoleToolsCollection.ConsoleSelector;
using ProductHeaderValue = Octokit.ProductHeaderValue;

namespace piget.ReposEditor
{
    public class Editor
    {
        private GitHubClient gitHubClient = null;

        /// <summary>
        /// Репозитории содержащие библиотеки
        /// </summary>
        List<Repository> psRepositories = null;

        /// <summary>
        /// Выбранный репозиторий
        /// </summary>
        Repository currentRepository = null;

        /// <summary>
        /// Содержание выбранного репозитория (Библиотеки)
        /// </summary>
        IReadOnlyList<RepositoryContent> currentRepositoryContentLibraries = null;

        /// <summary>
        /// Содержание выбранного репозитория (Источники)
        /// </summary>
        IReadOnlyList<RepositoryContent> currentRepositoryContentSources = null;

        ConsoleSelector menu = new ConsoleSelector
        {
            Settings = new ConsoleSelectorSettings
            {
                Indentations = new ConsoleSelectorIndentations
                {
                    SelectionRight = 20,
                    SelectionLeft = 20,
                    Text = 3
                },

                MaxHeight = 20
            }
        };

        public void Initialize()
        {
            gitHubClient = new GitHubClient(new ProductHeaderValue("PigetConsoleRemoteLibraryManager"));
            gitHubClient.Credentials = new AuthPage().GetCredentials();

            Helpers.Logs.Log("\r\n<!> ", $"Инициализация аккаунта {gitHubClient.User.Current().Result.Login}");
            Helpers.Logs.Log("<!> ", $"Поиск библиотек в репозиториях");

            psRepositories = SearchLibraryRepositories();

            Console.Clear();

            SelectRepository();
            SelectLibrary();
        }

        public Repository SelectRepository()
        {
            menu.Items.Add(new ConsoleSelectorItem("[+] Создать репозиторий", tag: "newrep"));

            psRepositories.ForEach(rep => 
                menu.Items.Add(new ConsoleSelectorItem(rep.Name + "/", tag: rep)));

            ConsoleSelectorItem answer = menu.Show();
            if (answer.Tag == "newrep" as object)
            {
                CreateRepository();
                return SelectRepository();
            }

            return answer.Tag as Repository;
        }

        public void SelectLibrary()
        {
            
        }

        public void CreateRepository()
        {
            Helpers.Logs.Log("<!> ", $"TODO: Создание библиотеки");
        }

        private List<Repository> SearchLibraryRepositories()
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

            return pigetRemoteScriptLibrariesRepositories;
        }

        private void SearchLibrariesInRepository()
        {
            
        }
    }
}
