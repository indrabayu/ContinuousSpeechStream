using ApiAi.Common;
using ApiAiSDK;
using ApiAiSDK.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App2.ApiDotAi
{
    public class ApiAiUtils
    {
        public static Result Recognize(Android.Content.Context context, string query)
        {
            var config = new AIConfiguration(Keys.ApiDotAi, SupportedLanguage.English);
            var aiService = AIService.CreateService(context, config);
            var request = new AIRequest(query);
            var dataService = new AIDataService(config);
            var response = dataService.Request(request);
            return response.Result;
        }
    }
}