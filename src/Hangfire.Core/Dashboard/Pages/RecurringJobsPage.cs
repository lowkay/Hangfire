﻿using System;
using System.Collections.Generic;
using Hangfire.Common;
using Hangfire.Storage;

namespace Hangfire.Dashboard.Pages
{
    partial class RecurringJobsPage
    {
        private List<RecurringJobDto> GetRecurringJobs()
        {
            var result = new List<RecurringJobDto>();

            using (var connection = Storage.GetConnection())
            {
                var ids = connection.GetAllItemsFromSet("recurring-jobs");

                foreach (var id in ids)
                {
                    var hash = connection.GetAllEntriesFromHash(String.Format("recurring-job:{0}", id));

                    if (hash == null)
                    {
                        result.Add(new RecurringJobDto { Id = id, Removed = true });
                        continue;
                    }

                    var dto = new RecurringJobDto { Id = id };
                    dto.Cron = hash["Cron"];

                    try
                    {
                        var invocationData = JobHelper.FromJson<InvocationData>(hash["Job"]);
                        dto.Job = invocationData.Deserialize();
                    }
                    catch (JobLoadException ex)
                    {
                        dto.LoadException = ex;
                    }

                    if (hash.ContainsKey("NextExecution"))
                    {
                        dto.NextExecution = JobHelper.DeserializeDateTime(hash["NextExecution"]);
                    }

                    if (hash.ContainsKey("LastJobId"))
                    {
                        dto.LastJobId = hash["LastJobId"];

                        var stateData = connection.GetStateData(dto.LastJobId);
                        if (stateData != null)
                        {
                            dto.LastJobState = stateData.Name;
                        }
                    }

                    if (hash.ContainsKey("LastExecution"))
                    {
                        dto.LastExecution = JobHelper.DeserializeDateTime(hash["LastExecution"]);
                    }

                    result.Add(dto);
                }
            }

            return result;
        }

        public class RecurringJobDto
        {
            public string Id { get; set; }
            public string Cron { get; set; }
            public Job Job { get; set; }
            public JobLoadException LoadException { get; set; }
            public DateTime? NextExecution { get; set; }
            public string LastJobId { get; set; }
            public string LastJobState { get; set; }
            public DateTime? LastExecution { get; set; }
            public bool Removed { get; set; }
        }
    }
}