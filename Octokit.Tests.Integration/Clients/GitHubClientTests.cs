﻿using Octokit;
using Octokit.Tests.Integration;
using Octokit.Tests.Integration.Helpers;
using System;
using System.Threading.Tasks;
using Octokit.Internal;
using Xunit;

public class GitHubClientTests
{
    public class Monkey
    {
        [IntegrationTest]
        public void Setup()
        {
            var info = new ClientInfo("my-cool-app")
            {
                Credentials = new InMemoryCredentialStore(new Credentials("my-token-here")),
                Server = new Uri("https://my-cool-enterprise.com")
            };

            var http = HttpClientFactory.Create(info);

            var productHeader = new ProductHeaderValue("my-cool-app");
            var connection = new Connection(productHeader, http);

            // TODO: we probably need to guard here that we have enough
            //       information to use this client against the GitHub API
            var github = new GitHubClient(connection);
        }
    }

    public class TheLastApiInfoProperty
    {
        [IntegrationTest]
        public async Task CanRetrieveLastApiInfoWithEtag()
        {
            // To check for etag, I'm using a new repository
            // As per suggestion here -> https://github.com/octokit/octokit.net/pull/855#issuecomment-126966532
            var github = Helper.GetAuthenticatedClient();
            var repoName = Helper.MakeNameWithTimestamp("public-repo");

            using (var context = await github.CreateRepositoryContext(new NewRepository(repoName)))
            {
                var createdRepository = context.Repository;

                var result = github.GetLastApiInfo();

                Assert.True(result.Links.Count == 0);
                Assert.True(result.AcceptedOauthScopes.Count > -1);
                Assert.True(result.OauthScopes.Count > -1);
                Assert.False(string.IsNullOrEmpty(result.Etag));
                Assert.True(result.RateLimit.Limit > 0);
                Assert.True(result.RateLimit.Remaining > -1);
                Assert.NotNull(result.RateLimit.Reset);
            }
        }

        [IntegrationTest]
        public async Task CanRetrieveLastApiInfoWithLinks()
        {
            // To check for links, I'm doing a list of all contributors to the octokit.net project
            // Adapted from suggestion here -> https://github.com/octokit/octokit.net/pull/855#issuecomment-126966532
            var github = Helper.GetAuthenticatedClient();

            await github.Repository.GetAllContributors("octokit", "octokit.net");

            var result = github.GetLastApiInfo();

            Assert.True(result.Links.Count > 0);
            Assert.True(result.AcceptedOauthScopes.Count > -1);
            Assert.True(result.OauthScopes.Count > -1);
            Assert.False(string.IsNullOrEmpty(result.Etag));
            Assert.True(result.RateLimit.Limit > 0);
            Assert.True(result.RateLimit.Remaining > -1);
            Assert.NotNull(result.RateLimit.Reset);
        }

        [PersonalAccessTokenTest]
        public async Task CanRetrieveLastApiInfoAcceptedOauth()
        {
            // To check for OAuth & AcceptedOAuth I'm getting the octokit user
            // Adapted from suggestion here -> https://github.com/octokit/octokit.net/pull/855#issuecomment-126966532
            var github = Helper.GetAuthenticatedClient();

            await github.User.Get("octokit");

            var result = github.GetLastApiInfo();

            Assert.True(result.Links.Count == 0);
            Assert.True(result.AcceptedOauthScopes.Count > 0);
            Assert.True(result.OauthScopes.Count > 0);
            Assert.False(string.IsNullOrEmpty(result.Etag));
            Assert.True(result.RateLimit.Limit > 0);
            Assert.True(result.RateLimit.Remaining > -1);
            Assert.NotNull(result.RateLimit.Reset);
        }
    }
}
