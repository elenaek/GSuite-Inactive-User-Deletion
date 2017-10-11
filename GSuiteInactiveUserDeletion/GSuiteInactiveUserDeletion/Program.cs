using Google.Apis.Auth.OAuth2;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GSuiteInactiveUserDeletion
{
    class Program
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/admin-directory_v1-dotnet-quickstart.json
        static string[] Scopes = { DirectoryService.Scope.AdminDirectoryUserReadonly,
            DirectoryService.Scope.AdminDirectoryGroupReadonly,
            };
        static string ApplicationName = "GSuite User Management";
        static string apiKey = "";

        static void Main(string[] args)
        {
            UserCredential credential;

            using (var stream =
                new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/GSuite-User-Management.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Directory API service.
            var service = new DirectoryService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
                //ApiKey = 
            });

            // Define parameters of request.
            UsersResource.ListRequest request = service.Users.List();
            request.Customer = "my_customer";
            request.MaxResults = 100;
            request.OrderBy = UsersResource.ListRequest.OrderByEnum.Email;

           

            // List users.
            IList<User> users = request.Execute().UsersValue;
            if (users != null && users.Count > 0)
            {
                foreach (var userItem in users)
                {
                    if (DateTime.Compare(userItem.LastLoginTime ?? DateTime.Now, DateTime.Now.AddMonths(-2)) < 0)
                    {
                        //Send SMTP Warning that account will be deleted
                        //Console.WriteLine($"{userItem.PrimaryEmail}");

                        GroupsResource.ListRequest userGroupsRequest = service.Groups.List();
                        userGroupsRequest.UserKey = userItem.Id;
                        var userGroups = userGroupsRequest.Execute().GroupsValue;

                        if (userGroups != null)
                        {
                            foreach (var group in userGroups)
                            {
                                if (group.Name == "NoAutoDelete")
                                {
                                    Console.WriteLine(userItem.PrimaryEmail);
                                }
                                //Console.WriteLine($"{group.Name}");
                            }
                        }
                    }
                    
                }
                
            }
            else
            {
                Console.WriteLine("No users found.");
            }
            Console.Read();

        }
    }
}
