// Licensed to the Sio I/O Foundation under one or more agreements.
// The Sio I/O Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sio.Domain.Core.ViewModels;
using Sio.Cms.Lib.Models.Cms;
using Sio.Cms.Lib;
using Sio.Cms.Lib.Services;
using static Sio.Cms.Lib.SioEnums;
using System.Linq.Expressions;
using Sio.Cms.Lib.ViewModels.SioArticles;
using System.Web;
using Microsoft.Extensions.Caching.Memory;
using Sio.Cms.Lib.ViewModels;

namespace Sio.Cms.Api.Controllers.v1
{
    [Produces("application/json")]
    [Route("api/v1/{culture}/article")]
    public class ApiArticleController :
        BaseGenericApiController<SioCmsContext, SioArticle>
    {
        public ApiArticleController(IMemoryCache memoryCache, Microsoft.AspNetCore.SignalR.IHubContext<Hub.PortalHub> hubContext) : base(memoryCache, hubContext)
        {
        }

        #region Get

        // GET api/article/id
        [HttpGet, HttpOptions]
        [Route("delete/{id}")]
        public async Task<RepositoryResponse<SioArticle>> DeleteAsync(int id)
        {
            return await base.DeleteAsync<RemoveViewModel>(
                model => model.Id == id && model.Specificulture == _lang, true);
        }

        // GET api/articles/id
        [HttpGet, HttpOptions]
        [Route("details/{id}/{viewType}")]
        [Route("details/{viewType}")]
        public async Task<ActionResult<JObject>> Details(string viewType, int? id)
        {
            string msg = string.Empty;
            switch (viewType)
            {
                case "portal":
                    if (id.HasValue)
                    {
                        Expression<Func<SioArticle, bool>> predicate = model => model.Id == id && model.Specificulture == _lang;
                        var portalResult = await base.GetSingleAsync<UpdateViewModel>($"{viewType}_{id}", predicate);
                        if (portalResult.IsSucceed)
                        {
                            portalResult.Data.DetailsUrl = SioCmsHelper.GetRouterUrl("Article", new { id = portalResult.Data.Id, SeoName = portalResult.Data.SeoName }, Request, Url);
                        }

                        return Ok(JObject.FromObject(portalResult));
                    }
                    else
                    {
                        var model = new SioArticle()
                        {
                            Specificulture = _lang,
                            Status = SioService.GetConfig<int>("DefaultStatus"),
                            Priority = UpdateViewModel.Repository.Max(a => a.Priority).Data + 1
                        };

                        RepositoryResponse<UpdateViewModel> result = await base.GetSingleAsync<UpdateViewModel>($"{viewType}_default", null, model);
                        return Ok(JObject.FromObject(result));
                    }
                default:
                    if (id.HasValue)
                    {
                        var beResult = await ReadMvcViewModel.Repository.GetSingleModelAsync(model => model.Id == id && model.Specificulture == _lang).ConfigureAwait(false);
                        if (beResult.IsSucceed)
                        {
                            beResult.Data.DetailsUrl = SioCmsHelper.GetRouterUrl("Article", new { id = beResult.Data.Id, beResult.Data.SeoName }, Request, Url);
                        }
                        return Ok(JObject.FromObject(beResult));
                    }
                    else
                    {
                        var model = new SioArticle();
                        RepositoryResponse<ReadMvcViewModel> result = new RepositoryResponse<ReadMvcViewModel>()
                        {
                            IsSucceed = true,
                            Data = new ReadMvcViewModel(model)
                            {
                                Specificulture = _lang,
                                Status = SioContentStatus.Preview,
                            }
                        };
                        return Ok(JObject.FromObject(result));
                    }
            }
        }


        #endregion Get

        #region Post

        // POST api/article
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin, Admin")]
        [HttpPost, HttpOptions]
        [Route("save")]
        public async Task<RepositoryResponse<UpdateViewModel>> Save([FromBody]UpdateViewModel model)
        {
            if (model != null)
            {
                model.CreatedBy = User.Claims.FirstOrDefault(c => c.Type == "Username")?.Value;
                var result = await base.SaveAsync<UpdateViewModel>(model, true);
                if (result.IsSucceed)
                {
                    if (model.Status == SioEnums.SioContentStatus.Schedule)
                    {
                        DateTime dtPublish = DateTime.UtcNow;
                        if (model.PublishedDateTime.HasValue)
                        {
                            dtPublish = model.PublishedDateTime.Value;
                        }
                        SioService.SetConfig(SioConstants.ConfigurationKeyword.NextSyncContent, dtPublish);
                        SioService.SaveSettings();
                        SioService.Reload();
                    }
                }
                return result;
            }
            return new RepositoryResponse<UpdateViewModel>() { Status = 501 };
        }

        [HttpPost, HttpOptions]
        [Route("save-list")]
        public async Task<RepositoryResponse<List<SyncViewModel>>> SaveList([FromBody]List<SyncViewModel> models)
        {
            if (models != null)
            {
                return await base.SaveListAsync(models, true);
            }
            else
            {
                return new RepositoryResponse<List<SyncViewModel>>();
            }
        }
        // POST api/article
        [HttpPost, HttpOptions]
        [Route("save/{id}")]
        public async Task<RepositoryResponse<SioArticle>> SaveFields(int id, [FromBody]List<EntityField> fields)
        {
            if (fields != null)
            {
                var result = new RepositoryResponse<SioArticle>() { IsSucceed = true };
                foreach (var property in fields)
                {
                    if (result.IsSucceed)
                    {
                        result = await ReadListItemViewModel.Repository.UpdateFieldsAsync(c => c.Id == id && c.Specificulture == _lang, fields).ConfigureAwait(false);
                    }
                    else
                    {
                        break;
                    }

                }
                return result;
            }
            return new RepositoryResponse<SioArticle>();
        }

        // GET api/article
        [HttpPost, HttpOptions]
        [Route("list")]
        public async Task<ActionResult<JObject>> GetList(
            [FromBody] RequestPaging request)
        {
            var query = HttpUtility.ParseQueryString(request.Query ?? "");
            bool isPage = int.TryParse(query.Get("page_id"), out int pageId);
            bool isNotPage = int.TryParse(query.Get("not_page_id"), out int notPageId);
            bool isModule = int.TryParse(query.Get("module_id"), out int moduleId);
            bool isNotModule = int.TryParse(query.Get("not_module_id"), out int notModuleId);
            ParseRequestPagingDate(request);
            Expression<Func<SioArticle, bool>> predicate = model =>
                        model.Specificulture == _lang
                        && (!request.Status.HasValue || model.Status == request.Status.Value)
                        && (!isPage || model.SioPageArticle.Any(nav => nav.CategoryId == pageId && nav.ArticleId == model.Id && nav.Specificulture == _lang))
                        && (!isNotPage || !model.SioPageArticle.Any(nav => nav.CategoryId == notPageId && nav.ArticleId == model.Id && nav.Specificulture == _lang))
                        && (!isModule || model.SioModuleArticle.Any(nav => nav.ModuleId == moduleId && nav.ArticleId == model.Id))
                        && (!isNotModule || !model.SioModuleArticle.Any(nav => nav.ModuleId == notModuleId && nav.ArticleId == model.Id))
                        && (string.IsNullOrWhiteSpace(request.Keyword)
                            || (model.Title.Contains(request.Keyword)
                            || model.Excerpt.Contains(request.Keyword)))
                        && (!request.FromDate.HasValue
                            || (model.CreatedDateTime >= request.FromDate.Value)
                        )
                        && (!request.ToDate.HasValue
                            || (model.CreatedDateTime <= request.ToDate.Value)
                        );

            var nextSync = PublishArticles();
            string key = $"{nextSync}_{request.Key}_{request.Query}_{request.PageSize}_{request.PageIndex}";

            switch (request.Key)
            {
                case "mvc":
                    var mvcResult = await base.GetListAsync<ReadMvcViewModel>(key, request, predicate);
                    if (mvcResult.IsSucceed)
                    {
                        mvcResult.Data.Items.ForEach(a =>
                        {
                            a.DetailsUrl = SioCmsHelper.GetRouterUrl(
                                "Article", new { id = a.Id, seoName = a.SeoName }, Request, Url);
                        });
                    }

                    return Ok(JObject.FromObject(mvcResult));
                case "portal":
                    var portalResult = await base.GetListAsync<UpdateViewModel>(key, request, predicate);
                    if (portalResult.IsSucceed)
                    {
                        portalResult.Data.Items.ForEach(a =>
                        {
                            a.DetailsUrl = SioCmsHelper.GetRouterUrl(
                                "Article", new { id = a.Id, seoName = a.SeoName }, Request, Url);
                        });
                    }

                    return Ok(JObject.FromObject(portalResult));
                default:

                    var listItemResult = await base.GetListAsync<ReadListItemViewModel>(key, request, predicate);
                    if (listItemResult.IsSucceed)
                    {
                        listItemResult.Data.Items.ForEach((Action<ReadListItemViewModel>)(a =>
                        {
                            a.DetailsUrl = SioCmsHelper.GetRouterUrl(
                                "Article", new { id = a.Id, seoName = a.SeoName }, Request, Url);
                        }));
                    }

                    return JObject.FromObject(listItemResult);
            }
        }
        // POST api/update-infos
        [HttpPost, HttpOptions]
        [Route("update-infos")]
        public async Task<RepositoryResponse<List<ReadListItemViewModel>>> UpdateInfos([FromBody]List<ReadListItemViewModel> models)
        {
            if (models != null)
            {
                return await base.SaveListAsync(models, false);
            }
            else
            {
                return new RepositoryResponse<List<ReadListItemViewModel>>();
            }
        }

        [HttpPost, HttpOptions]
        [Route("apply-list")]
        public async Task<ActionResult<JObject>> ListActionAsync([FromBody]ListAction<int> data)
        {
            Expression<Func<SioArticle, bool>> predicate = model =>
                       model.Specificulture == _lang
                       && data.Data.Contains(model.Id);
            var result = new RepositoryResponse<bool>();
            switch (data.Action)
            {
                case "Delete":
                    return Ok(JObject.FromObject(await base.DeleteListAsync<RemoveViewModel>(true, predicate, false)));
                case "Export":
                    return Ok(JObject.FromObject(await base.ExportListAsync(predicate, SioStructureType.Module)));
                default:
                    return JObject.FromObject(new RepositoryResponse<bool>());
            }
        }
        #endregion Post

        DateTime? PublishArticles()
        {
            var nextSync = SioService.GetConfig<DateTime?>(SioConstants.ConfigurationKeyword.NextSyncContent);
            if (nextSync.HasValue && nextSync.Value <= DateTime.UtcNow)
            {
                var publishedArticles = ReadListItemViewModel.Repository.GetModelListBy(
                    a => a.Status == (int)SioContentStatus.Schedule
                        && (!a.PublishedDateTime.HasValue || a.PublishedDateTime.Value <= DateTime.UtcNow)
                        );
                publishedArticles.Data.ForEach(a => a.Status = SioContentStatus.Published);
                base.SaveList(publishedArticles.Data, false);
                var next = ReadListItemViewModel.Repository.Min(a => a.Type == (int)SioContentStatus.Schedule,
                            a => a.PublishedDateTime);
                nextSync = next.Data;
                SioService.SetConfig(SioConstants.ConfigurationKeyword.NextSyncContent, nextSync);
                SioService.SaveSettings();
                SioService.Reload();
                return nextSync;
            }
            else
            {
                return nextSync;
            }
        }
    }
}
