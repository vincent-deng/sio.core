﻿using System;
using System.Collections.Generic;
using Swastika.Cms.Lib.Models.Cms;
using Swastika.Domain.Data.ViewModels;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using Swastika.Cms.Lib;
using Swastika.Domain.Core.ViewModels;
using System.Threading.Tasks;
using Microsoft.Data.OData.Query;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Swastika.Cms.Lib.ViewModels.Info
{
    public class InfoProductViewModel
       : ViewModelBase<SiocCmsContext, SiocProduct, InfoProductViewModel>
    {
        #region Properties

        #region Models
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("template")]
        public string Template { get; set; }
        [JsonProperty("thumbnail")]
        public string Thumbnail { get; set; }
        [JsonProperty("image")]
        public string Image { get; set; }
        [JsonProperty("price")]
        public double Price { get; set; }
        [JsonProperty("priceUnit")]
        public string PriceUnit { get; set; }
        [JsonProperty("icon")]
        public string Icon { get; set; }
        [Required]
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("excerpt")]
        public string Excerpt { get; set; }
        [JsonProperty("content")]
        public string Content { get; set; }
        [JsonProperty("seoName")]
        public string SeoName { get; set; }
        [JsonProperty("seoTitle")]
        public string SeoTitle { get; set; }
        [JsonProperty("seoDescription")]
        public string SeoDescription { get; set; }
        [JsonProperty("seoKeywords")]
        public string SeoKeywords { get; set; }
        [JsonProperty("source")]
        public string Source { get; set; }
        [JsonProperty("views")]
        public int? Views { get; set; }
        [JsonProperty("type")]
        public int Type { get; set; }
        [JsonProperty("createdDateTime")]
        public DateTime CreatedDateTime { get; set; }
        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }
        [JsonProperty("lastModified")]
        public DateTime? LastModified { get; set; }
        [JsonProperty("modifiedBy")]
        public string ModifiedBy { get; set; }
        [JsonProperty("isVisible")]
        public bool IsVisible { get; set; }
        [JsonProperty("isDeleted")]
        public bool IsDeleted { get; set; }
        [JsonProperty("tags")]
        public string Tags { get; set; }
        #endregion

        #region Views
        [JsonProperty("domain")]
        public string Domain { get; set; } = "/";
        [JsonProperty("imageUrl")]
        public string ImageUrl
        {
            get
            {
                if (Image != null && Image.IndexOf("http") == -1)
                {
                    return SWCmsHelper.GetFullPath(new string[] {
                    Domain,  Image
                });
                }
                else
                {
                    return Image;
                }

            }
        }
        [JsonProperty("thumbnailUrl")]
        public string ThumbnailUrl
        {
            get
            {
                if (Thumbnail != null && Thumbnail.IndexOf("http") == -1)
                {
                    return SWCmsHelper.GetFullPath(new string[] {
                    Domain,  Thumbnail
                });
                }
                else
                {
                    return Thumbnail;
                }

            }
        }

        [JsonProperty("detailsUrl")]
        public string DetailsUrl { get; set; }
        #endregion

        #endregion

        #region Contructors

        public InfoProductViewModel() : base()
        {
        }

        public InfoProductViewModel(SiocProduct model, SiocCmsContext _context = null, IDbContextTransaction _transaction = null) : base(model, _context, _transaction)
        {
        }

        #endregion

        #region Expands

        public static async Task<RepositoryResponse<PaginationModel<InfoProductViewModel>>> GetModelListByCategoryAsync(
            int categoryId, string specificulture
            , string orderByPropertyName, OrderByDirection direction
            , int? pageSize = 1, int? pageIndex = 0
            , SiocCmsContext _context = null, IDbContextTransaction _transaction = null)
        {
            SiocCmsContext context = _context ?? new SiocCmsContext();
            var transaction = _transaction ?? context.Database.BeginTransaction();
            try
            {
                
                var query = context.SiocCategoryProduct.Include(ac => ac.SiocProduct)
                    .Where(ac =>
                    ac.CategoryId == categoryId && ac.Specificulture == specificulture
                    && !ac.SiocProduct.IsDeleted && ac.SiocProduct.IsVisible).Select(ac => ac.SiocProduct);
                PaginationModel<InfoProductViewModel> result = await Repository.ParsePagingQueryAsync(
                    query, orderByPropertyName
                    , direction,
                    pageSize, pageIndex, context, transaction
                    );
                return new RepositoryResponse<PaginationModel<InfoProductViewModel>>()
                {
                    IsSucceed = true,
                    Data = result
                };
            }
            // TODO: Add more specific exeption types instead of Exception only
            catch (Exception ex)
            {
                Repository.LogErrorMessage(ex);
                if (_transaction == null)
                {
                    //if current transaction is root transaction
                    transaction.Rollback();
                }

                return new RepositoryResponse<PaginationModel<InfoProductViewModel>>()
                {
                    IsSucceed = false,
                    Data = null,
                    Exception = ex
                };
            }
            finally
            {
                if (_context == null)
                {
                    //if current Context is Root
                    context.Dispose();
                }
            }

        }

        #region Sync
        public static RepositoryResponse<PaginationModel<InfoProductViewModel>> GetModelListByCategory(
           int categoryId, string specificulture
           , string orderByPropertyName, OrderByDirection direction
           , int? pageSize = 1, int? pageIndex = 0
           , SiocCmsContext _context = null, IDbContextTransaction _transaction = null)
        {
            SiocCmsContext context = _context ?? new SiocCmsContext();
            var transaction = _transaction ?? context.Database.BeginTransaction();
            try
            {
                var query = context.SiocCategoryProduct.Include(ac => ac.SiocProduct)
                    .Where(ac =>
                    ac.CategoryId == categoryId && ac.Specificulture == specificulture
                    && !ac.SiocProduct.IsDeleted && ac.SiocProduct.IsVisible).Select(ac => ac.SiocProduct);
                PaginationModel<InfoProductViewModel> result = Repository.ParsePagingQuery(
                    query, orderByPropertyName
                    , direction,
                    pageSize, pageIndex, context, transaction
                    );
                return new RepositoryResponse<PaginationModel<InfoProductViewModel>>()
                {
                    IsSucceed = true,
                    Data = result
                };
            }
            // TODO: Add more specific exeption types instead of Exception only
            catch (Exception ex)
            {
                Repository.LogErrorMessage(ex);
                if (_transaction == null)
                {
                    //if current transaction is root transaction
                    transaction.Rollback();
                }

                return new RepositoryResponse<PaginationModel<InfoProductViewModel>>()
                {
                    IsSucceed = false,
                    Data = null,
                    Exception = ex
                };
            }
            finally
            {
                if (_context == null)
                {
                    //if current Context is Root
                    context.Dispose();
                }
            }

        }

        public static RepositoryResponse<PaginationModel<InfoProductViewModel>> GetModelListByModule(
          int ModuleId, string specificulture
          , string orderByPropertyName, OrderByDirection direction
          , int? pageSize = 1, int? pageIndex = 0
          , SiocCmsContext _context = null, IDbContextTransaction _transaction = null)
        {
            SiocCmsContext context = _context ?? new SiocCmsContext();
            var transaction = _transaction ?? context.Database.BeginTransaction();
            try
            {
                var query = context.SiocModuleProduct.Include(ac => ac.SiocProduct)
                    .Where(ac =>
                    ac.ModuleId == ModuleId && ac.Specificulture == specificulture
                    && !ac.SiocProduct.IsDeleted && ac.SiocProduct.IsVisible).Select(ac => ac.SiocProduct);
                PaginationModel<InfoProductViewModel> result = Repository.ParsePagingQuery(
                    query, orderByPropertyName
                    , direction,
                    pageSize, pageIndex, context, transaction
                    );
                return new RepositoryResponse<PaginationModel<InfoProductViewModel>>()
                {
                    IsSucceed = true,
                    Data = result
                };
            }
            // TODO: Add more specific exeption types instead of Exception only
            catch (Exception ex)
            {
                Repository.LogErrorMessage(ex);
                if (_transaction == null)
                {
                    //if current transaction is root transaction
                    transaction.Rollback();
                }

                return new RepositoryResponse<PaginationModel<InfoProductViewModel>>()
                {
                    IsSucceed = false,
                    Data = null,
                    Exception = ex
                };
            }
            finally
            {
                if (_context == null)
                {
                    //if current Context is Root
                    context.Dispose();
                }
            }

        }
        #endregion
        #endregion
    }
}