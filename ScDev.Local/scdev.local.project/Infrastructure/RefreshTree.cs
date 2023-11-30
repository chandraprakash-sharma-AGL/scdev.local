﻿using scdev.local.project.Models;
using Nest;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Jobs;
using Sitecore.Shell.Applications.Dialogs.ProgressBoxes;
using Sitecore.Shell.Framework.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace scdev.local.project.Infrastructure
{
    [Serializable]
    public class RefreshTree : Command
    {
        public Handle JobHandle { get; private set; }
        public override void Execute(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "content");
            var item = context.Items[0];

            Assert.IsNotNull(item, "context item cannot be null");
            ProgressBoxMethod progressBoxMethod = new ProgressBoxMethod(TopFeaturesRefresh);
            //Sitecore.Web.UI.Sheer.SheerResponse.YesNoCancel("Do you want to re-indexing the current item and its descendants in Elastic", "500", "500");
            ProgressBox.Execute(string.Format("{0} ({1})", "Re-Index Elastic Tree.",
                item.Paths.ContentPath),
                "Re-indexing the current item and its descendants in Elastic",
                progressBoxMethod,
                new object[] { item });
            //Sitecore.Web.UI.Sheer.SheerResponse.Alert("Refresh tree complete");
        }
        private void Refresh(object[] parameters)
        {
            JobHandle = Sitecore.Context.Job.Handle;
            if (parameters.Length != 1) return;
            var contextItem = parameters[0] as Item;
            if (contextItem == null) return;
            var job = JobManager.GetJob(JobHandle);
            var items = new List<Item>();
            if (contextItem.TemplateID == Templates.NewsArticle.ID) items.AddRange(contextItem.GetChildren().ToList());
            items.AddRange(contextItem.Axes.GetDescendants().Where(s => s.TemplateID == Templates.NewsArticle.ID));

            List<NewsArticle> newsArticles = items.Select(news => new NewsArticle
            {
                Id = news.ID.ToString(),
                Body = news[Templates.NewsArticle.Fields.Body],
                Date = news[Templates.NewsArticle.Fields.Date],
                Image = news[Templates.NewsArticle.Fields.Image],
                Summary = news[Templates.NewsArticle.Fields.Summary],
                Title = news[Templates.NewsArticle.Fields.Title]
            }).ToList();

            if (job != null) job.Status.Messages.Add(string.Format("Indexing: {0} entries", newsArticles.Count));
            var response = IndexNews(newsArticles);
            if (response != null) job.Status.Messages.Add(string.Format("Indexing result: {0}", response.DebugInformation));
        }
        private BulkResponse IndexNews(List<NewsArticle> newsArticles)
        {
            if (newsArticles == null || !newsArticles.Any()) return null; // Connecting to Elasticsearch 
            string protocol = Settings.GetSetting("ElasticSearch.Protocol", "http");
            string host = Settings.GetSetting("ElasticSearch.Host", "localhost");
            string port = Settings.GetSetting("ElasticSearch.Port", "9200");
            var node = new Uri(string.Format("{0}://{1}:{2}", protocol, host, port));
            var settings = new Nest.ConnectionSettings(node).DisableDirectStreaming();
            var client = new Nest.ElasticClient(settings); // Reindexing items 
            var indexName = Settings.GetSetting("ElasticSearch.ArticlesIndex", "article-index");
            var indexerResponse = client.IndexMany(newsArticles, indexName);
            return indexerResponse;
        }
        public void TopFeaturesRefresh(object[] parameters)
        {
            if (parameters.Length != 1) return;
            var contextItem = parameters[0] as Item;
            if (contextItem == null) return;
            Sitecore.Data.Database db = Sitecore.Data.Database.GetDatabase("master");
            Item parentItem = db.GetItem("{D9FD1039-5865-4275-94D9-E6B933C60691}");
            List<Item> childItem = parentItem.GetChildren().ToList();
            List<List<TopFeatures>> items1 = childItem.Select(item =>
            {
                List<Item> brandVarientItems = item.GetChildren()[0].GetChildren().ToList();
                return brandVarientItems.Select(item1 => new TopFeatures()
                {
                    VariantCode = item1.Fields["VariantCode"].Value,
                    VariantName = item1.Fields["VariantName"].Value,
                    DesktopImage = item1.Fields["DesktopImage"].Value,
                    MobileImage = item1.Fields["MobileImage"].Value,
                    TopFeature = item1.Fields["TopFeature"].Value,
                    TopSelected = item1.Fields["TopSelected"].Value
                }).ToList();
            }).ToList();
            List<TopFeatures> topFeatures = items1.SelectMany(item => item).ToList();
            IndexTopFeaturer(topFeatures);
        }
        public  void IndexTopFeaturer(List<TopFeatures> topFeatures)
        {
           // if (topFeatures == null || !topFeatures.Any()) return null; // Connecting to Elasticsearch 
            string protocol = Settings.GetSetting("ElasticSearch.Protocol", "http");
            string host = Settings.GetSetting("ElasticSearch.Host", "localhost");
            string port = Settings.GetSetting("ElasticSearch.Port", "9200");
            var node = new Uri(string.Format("{0}://{1}:{2}", protocol, host, port));
            var settings = new Nest.ConnectionSettings(node).DisableDirectStreaming();
            var client = new Nest.ElasticClient(settings); // Reindexing items 
            //var indexName = Settings.GetSetting("ElasticSearch.ArticlesIndex", "article-index");
            var indexName = "topfeature-index";
            var indexerResponse = client.IndexMany(topFeatures,indexName);
        }
    }
}