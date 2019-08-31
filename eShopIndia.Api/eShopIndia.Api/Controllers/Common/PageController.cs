using eShopIndia.API.Utilities;
using eShopIndia.Entity.BaseEntities;
using eShopIndia.Entity.Common.DBEntities;
using eShopIndia.HelperClasses;
using ExceptionHandler;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace eShopIndia.API.Common
{
    [Route("v1/[controller]")]
    public abstract class PageController<T> : BaseController where T : BaseEntity, new()
    {
        protected PageController(IOptions<AppSettings> config) : base(config)
        {
        }

        [HttpGet("search")]
        public virtual MainViewModel<T> SearchList(T SearchContext)
        {
            var message = new ExceptionMessage("Enter->" + MethodBase.GetCurrentMethod());
            List<T> results = null;
            var searchContextList = new List<SearchContextEntity>();
            if (NotValidIdentity())
                SessionExpired(message);
            else
            {
                try
                {
                    results = GridSearch(SearchContext, message);
                    if (results.Count > _config.RowsLimit)
                    {
                        message.Fail();
                        message.LastUserMessage = Messages.GeneriLargeDataErrorMsg;
                        return new MainViewModel<T>(new List<T>(), message);
                    }
                    // SetUserpreferences(Preferences.SearchDefinition, SearchContext);
                }
                catch (Exception ex)
                {
                    LogBaseControllerMessage(ex, message);
                }
            }
            return new MainViewModel<T>(results, message);
        }

        [HttpGet("detail/search")]
        public virtual MainViewModel<T> SearchDetail(T SearchContext)
        {
            var message = new ExceptionMessage("Enter->" + MethodBase.GetCurrentMethod());
            T results = null;
            if (NotValidIdentity())
                SessionExpired(message);
            else
            {
                try
                {
                    results = GetRecordDetail(SearchContext, message);
                }
                catch (Exception ex)
                {
                    LogBaseControllerMessage(ex, message);
                }
            }
            return new MainViewModel<T>(results, message);
        }

        [HttpPost("insertupdate")]
        public virtual JsonResult InsertUpdate([FromBody]T formData)
        {
            ActionFlags action = formData.Flag;
            ExceptionMessage message = new ExceptionMessage("Enter->" + MethodBase.GetCurrentMethod());
            if (NotValidIdentity())
                SessionExpired(message);
            else
            {
                try
                {
                    if (action == ActionFlags.Add)
                    {
                        InternalUpdateNewDetails(formData, message);
                    }
                    else
                    {
                        T workingItem = new T();
                        workingItem = InternalUpdateExistingDetails(formData, message);
                    }
                    if (message.Status)
                    {
                        if (message.UserMessages.Count == 0)
                            message.LastUserMessage = action == ActionFlags.Add ? Messages.RecordCreatedSuccessfully : action == ActionFlags.Update ? Messages.RecordUpdatedSuccessfully : Messages.InsertUpdateCompleted;
                    }
                }
                catch (Exception ex)
                {
                    LogBaseControllerMessage(ex, message);
                }
            }
            return Json(message);
        }

        [HttpDelete("delete")]
        public virtual JsonResult Delete(string objectsToBeDeleted)
        {
            ExceptionMessage message = new ExceptionMessage("Enter->" + MethodBase.GetCurrentMethod());
            if (NotValidIdentity())
                SessionExpired(message);
            else
            {
                try
                {
                    object[] Ids = objectsToBeDeleted.Split(',').ToArray();
                    if (objectsToBeDeleted.Length > 0)
                    {
                        for (int i = 0; i < Ids.Length; i++)
                        {
                            InternalDeleteDetailsById(Ids[i], message);
                        }
                    }
                    if (message.Status)
                    {
                        if (message.UserMessages.Count == 0)
                            message.LastUserMessage = Messages.RecordDeletedSuccessfully;
                    }
                }
                catch (Exception ex)
                {
                    LogBaseControllerMessage(ex, message);
                }
            }
            return Json(message);
        }

        [HttpDelete("deleteentity")]
        public virtual JsonResult DeleteEntity([FromBody]T formData)
        {
            ExceptionMessage message = new ExceptionMessage("Enter->" + MethodBase.GetCurrentMethod());
            if (NotValidIdentity())
                SessionExpired(message);
            else
            {
                try
                {
                    InternalDeleteDetailsById(formData, message);

                    if (message.Status)
                    {
                        if (message.UserMessages.Count == 0)
                            message.LastUserMessage = Messages.RecordDeletedSuccessfully;
                    }
                }
                catch (Exception ex)
                {
                    LogBaseControllerMessage(ex, message);
                }
            }
            return Json(message);
        }

        protected abstract List<T> GridSearch(T searchModel, ExceptionMessage message);

        protected abstract T GetRecordDetail(T searchModel, ExceptionMessage message);

        protected virtual void InternalUpdateNewDetails(T newEntity, ExceptionMessage message)
        {
            message.LastMessage = "UpdateNewDetails method was not overrdidden";
        }

        protected virtual T InternalUpdateExistingDetails(T updatedEntity, ExceptionMessage message)
        {
            message.LastMessage = "UpdateExistingDetails method was not overridden";
            return updatedEntity;
        }

        protected virtual void InternalDeleteDetailsById(object Id, ExceptionMessage message)
        {
            message.LastMessage = "InternalDeleteDetailsById method was not overridden";
        }

        protected virtual void InternalDeleteDetailsById(T deleteEntity, ExceptionMessage message)
        {
            message.LastMessage = "InternalDeleteDetailsById method was not overridden";
        }

    }
}
