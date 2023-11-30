using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Jobs;
using Sitecore.Shell.Applications.Dialogs.ProgressBoxes;
using Sitecore.Shell.Framework.Commands;
using System;

namespace scdev.local.project.Infrastructure
{
    [Serializable]
    public class ReCreateIndexes : Sitecore.Shell.Framework.Commands.Command
    {
        protected Handle JobHandle { get; set; }
        public override void Execute(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            var item = context.Items[0];
            Assert.IsNotNull(item, "context item cannot be null");
            var progressBoxMethod = new ProgressBoxMethod(Recreate);
            ProgressBox.Execute("Recreate Elastic Indexes.", "Recreating all Elastic indexes.", progressBoxMethod, new object[] { item });
        }

        private void Recreate(params object[] parameters)
        {
            JobHandle = Context.Job.Handle;
            if (parameters.Length != 1) return;
            Item item = parameters[0] as Item;
            if (item == null) return;
            var job = JobManager.GetJob(JobHandle);

            // Connecting to Elasticsearch 
            string protocol = Settings.GetSetting("ElasticSearch.Protocol", "http");
            string host = Settings.GetSetting("ElasticSearch.Host", "localhost");
            string port = Settings.GetSetting("ElasticSearch.Port", "9200");
            var node = new Uri(string.Format("{0}://{1}:{2}", protocol, host, port));
            var settings = new Nest.ConnectionSettings(node).DisableDirectStreaming();
            var client = new Nest.ElasticClient(settings); // Reindexing items 
            var indexName = Settings.GetSetting("ElasticSearch.ArticlesIndex", "article-index");
            // Re-creating index 

            if (client.Indices.Exists(indexName).Exists)
            {
                DisplayStatusMessage(job, string.Format("Deleting '{0}' index", indexName));
                var deleteResponse = client.Indices.Delete(indexName);
                DisplayStatusMessage(job, string.Format("The index {0} - has been deleted? - {1}", indexName, deleteResponse.DebugInformation));
            }
            if (!client.Indices.Exists(indexName).Exists)
            {
                DisplayStatusMessage(job, string.Format("Creating '{0}' index", indexName));
                var createResponse = client.Indices.Create(indexName);
                DisplayStatusMessage(job, string.Format("The index {0} - has been created? {1}", indexName, createResponse.Acknowledged));

            }
        }
        private void DisplayStatusMessage(Job job, string message)
        {
            if (job != null) job.Status.Messages.Add(message);
        }
    }
}